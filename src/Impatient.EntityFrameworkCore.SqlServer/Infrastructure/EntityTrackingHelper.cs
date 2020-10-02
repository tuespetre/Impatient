using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure
{
    public static class EntityTrackingHelper
    {
        public static MethodInfo GetEntityUsingStateManagerMethodInfo { get; }
            = typeof(EntityTrackingHelper)
                .GetMethod(nameof(GetEntityUsingStateManager), BindingFlags.NonPublic | BindingFlags.Static);

        public static MethodInfo GetEntityUsingIdentityMapMethodInfo { get; }
            = typeof(EntityTrackingHelper)
                .GetMethod(nameof(GetEntityUsingIdentityMap), BindingFlags.NonPublic | BindingFlags.Static);

        public static MethodInfo TrackEntitiesMethodInfo { get; }
            = typeof(EntityTrackingHelper)
                .GetMethod(nameof(TrackEntities), BindingFlags.NonPublic | BindingFlags.Static);

        private static object GetEntityUsingStateManager(
            EFCoreDbCommandExecutor executor, 
            IEntityType entityType, 
            object[] keyValues, 
            object entity, 
            object[] shadowPropertyValues, 
            List<INavigation> includes)
        {
            var entry = executor.StateManager.TryGetEntry(entityType.FindPrimaryKey(), keyValues);

            if (entry is null)
            {
                if (shadowPropertyValues.Length == 0)
                {
                    entry = executor.EntryFactory.Create(executor.StateManager, entityType, entity);

                    executor.StateManager.StartTrackingFromQuery(entityType, entity, ValueBuffer.Empty);
                }
                else
                {
                    var valueBuffer = new ValueBuffer(shadowPropertyValues);

                    entry = executor.EntryFactory.Create(executor.StateManager, entityType, entity, valueBuffer);

                    executor.StateManager.StartTrackingFromQuery(entityType, entity, valueBuffer);
                }

                for (var i = 0; i < includes.Count; i++)
                {
                    entry.SetIsLoaded(includes[i], true);
                }
            }
            else
            {
                if (entry.EntityState == EntityState.Detached)
                {
                    entry.MarkUnchangedFromQuery();
                }

                for (var i = 0; i < includes.Count; i++)
                {
                    var include = includes[i];

                    include.AsNavigation().Setter.SetClrValue(entry.Entity, include.GetGetter().GetClrValue(entity));

                    entry.SetIsLoaded(include, true);
                }
            }

            return entry.Entity;
        }

        private static object GetEntityUsingIdentityMap(
            EFCoreDbCommandExecutor executor, 
            IEntityType entityType, 
            object[] keyValues, 
            object entity, 
            object[] shadowPropertyValues, 
            List<INavigation> includes)
        {
            if (entity is null)
            {
                return null;
            }

            if (!executor.TryGetEntity(entityType, keyValues, out var info))
            {
                Debug.Assert(info.Entity is null);

                info = new EntityMaterializationInfo
                {
                    Entity = entity,
                    KeyValues = keyValues,
                    ShadowPropertyValues = shadowPropertyValues,
                    EntityType = entityType,
                    Key = entityType.FindPrimaryKey()
                };

                if (includes.Count != 0)
                {
                    info.Includes = new HashSet<INavigation>();
                }

                executor.CacheEntity(entityType, keyValues, info);
            }
            else if (includes.Count != 0 && info.Includes is null)
            {
                info.Includes = new HashSet<INavigation>();

                executor.CacheEntity(entityType, keyValues, info);
            }

            for (var i = 0; i < includes.Count; i++)
            {
                var include = includes[i];

                if (!info.Includes.Contains(include))
                {
                    FixupNavigation(include, entity, info.Entity);

                    info.Includes.Add(include);
                }
            }

            return info.Entity;
        }

        private static IEnumerable IterateSource(IEnumerable source)
        {
            if (source is IList list)
            {
                var count = list.Count;

                for (var i = 0; i < count; i++)
                {
                    yield return list[i];
                }
            }
            else
            {
                foreach (var item in source)
                {
                    yield return item;
                }
            }
        }

        private static IEnumerable TrackEntities(
            IEnumerable source,
            EFCoreDbCommandExecutor executor,
            MaterializerAccessorInfo[] accessorInfos)
        {
            if (accessorInfos is null)
            {
                foreach (var item in IterateSource(source))
                {
                    yield return item;
                }

                yield break;
            }

            foreach (var item in IterateSource(source))
            {
                var result = item;

                foreach (var accessorInfo in accessorInfos)
                {
                    var value = accessorInfo.GetValue(item);

                    if (value is null)
                    {
                        continue;
                    }

                    if (ReferenceEquals(value, item))
                    {
                        HandleEntry(executor, ref result, accessorInfo.EntityType);

                        continue;
                    }

                    if (value is IQueryable queryable)
                    {
                        var expression = queryable.Expression;

                        if (expression is MethodCallExpression methodCall
                            && methodCall.Method.DeclaringType == typeof(Queryable)
                            && methodCall.Method.Name.Equals(nameof(Queryable.AsQueryable)))
                        {
                            Debug.Assert(methodCall.Arguments.Count == 1);

                            expression = methodCall.Arguments[0];
                        }

                        if (expression is ConstantExpression constant)
                        {
                            value = constant.Value;
                        }
                    }

                    if (value is IList list)
                    {
                        var i = 0;

                        foreach (var subvalue in TrackEntities(list, executor, accessorInfo.SubAccessors))
                        {
                            list[i] = subvalue;
                            i++;
                        }

                        continue;
                    }

                    if (value is IEnumerable enumerable)
                    {
                        // The whole point of the IList block above is that
                        // we need to update references to cached entities
                        // within the list. We should make sure that every 
                        // materialized IEnumerable is a list.

                        // If there is some kind of issue with entities
                        // not being tracked, uncommenting the below line
                        // might be a good place to start.

                        // Debugger.Break();

                        foreach (var subvalue in TrackEntities(enumerable, executor, accessorInfo.SubAccessors))
                        {
                        }

                        continue;
                    }

                    var copy = value;

                    HandleEntry(executor, ref value, accessorInfo.EntityType);

                    if (!ReferenceEquals(copy, value))
                    {
                        accessorInfo.SetValue?.Invoke(result, value);
                    }
                }

                yield return result;
            }
        }

        private static void HandleEntry<TEntity>(EFCoreDbCommandExecutor executor, ref TEntity entity, IEntityType entityType)
        {
            if (entityType is null || !executor.TryGetEntity(entity, entityType, out var info))
            {
                return;
            }

            var cached = entity;

            var entry 
                = executor.StateManager.TryGetEntry(info.Key, info.KeyValues) 
                ?? executor.StateManager.TryGetEntry(entity);

            if (entry is null)
            {
                if (info.ShadowPropertyValues.Length == 0)
                {
                    entry = executor.EntryFactory.Create(executor.StateManager, info.EntityType, entity);

                    executor.StateManager.StartTrackingFromQuery(entityType, entity, ValueBuffer.Empty);
                }
                else
                {
                    var valueBuffer = new ValueBuffer(info.ShadowPropertyValues);

                    entry = executor.EntryFactory.Create(executor.StateManager, info.EntityType, entity, valueBuffer);

                    executor.StateManager.StartTrackingFromQuery(entityType, entity, valueBuffer);
                }
            }
            else
            {
                cached = (TEntity)entry.Entity;

                if (entry.EntityState == EntityState.Detached)
                {
                    entry.MarkUnchangedFromQuery();
                }
            }

            if (info.Includes is not null)
            {
                foreach (INavigation include in info.Includes)
                {
                    entry.SetIsLoaded(include, true);

                    // Test that demonstrates the necessity of fixup:
                    // Include_collection_principal_already_tracked
                    FixupNavigation(include, entity, cached);
                }
            }

            entity = cached;
        }

        private static void FixupNavigation(INavigation navigation, object entity, object cached)
        {
            var value = navigation.GetGetter().GetClrValue(entity);

            if (value is null)
            {
                return;
            }

            if (navigation.FieldInfo is not null)
            {
                navigation.FieldInfo.SetValue(cached, value);
            }
            else
            {
                navigation.AsNavigation().Setter.SetClrValue(cached, value);
            }

            var inverse = navigation.Inverse;

            if (inverse is null)
            {
                return;
            }

            if (inverse.IsCollection)
            {
                var collection = inverse.GetCollectionAccessor();

                collection.Add(value, cached, true);
            }
            else
            {
                var setter = inverse.AsNavigation().Setter;

                if (value is IEnumerable enumerable)
                {
                    foreach (var item in enumerable)
                    {
                        setter.SetClrValue(item, cached);
                    }
                }
                else
                {
                    setter.SetClrValue(value, cached);
                }
            }
        }
    }
}
