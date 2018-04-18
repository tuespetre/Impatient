using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        public static readonly MethodInfo GetEntityUsingStateManagerMethodInfo
            = typeof(EntityTrackingHelper)
                .GetMethod(nameof(GetEntityUsingStateManager), BindingFlags.NonPublic | BindingFlags.Static);

        public static readonly MethodInfo GetEntityUsingIdentityMapMethodInfo
            = typeof(EntityTrackingHelper)
                .GetMethod(nameof(GetEntityUsingIdentityMap), BindingFlags.NonPublic | BindingFlags.Static);

        public static readonly MethodInfo TrackEntitiesMethodInfo
            = typeof(EntityTrackingHelper)
                .GetMethod(nameof(TrackEntities), BindingFlags.NonPublic | BindingFlags.Static);

        private static object GetEntityUsingStateManager(
            EFCoreDbCommandExecutor executor,
            IEntityType entityType,
            IKey key,
            object[] keyValues,
            object entity,
            IProperty[] shadowProperties,
            object[] shadowPropertyValues,
            List<INavigation> loadedNavigations)
        {
            var stateManager = executor.CurrentDbContext.GetDependencies().StateManager;

            var entry = stateManager.TryGetEntry(key, keyValues);

            if (entry == null)
            {
                entry = stateManager.GetOrCreateEntry(entity);

                for (var i = 0; i < shadowProperties.Length; i++)
                {
                    entry.SetProperty(shadowProperties[i], shadowPropertyValues[i], false);
                }

                stateManager.StartTracking(entry);

                entry.MarkUnchangedFromQuery(null);
            }

            foreach (var navigation in loadedNavigations)
            {
                entry.SetIsLoaded(navigation);
            }

            return entry.Entity;
        }

        private static object GetEntityUsingIdentityMap(
            EFCoreDbCommandExecutor executor,
            IEntityType entityType,
            IKey key,
            object[] keyValues,
            object entity,
            IProperty[] shadowProperties,
            object[] shadowPropertyValues,
            List<INavigation> includes)
        {
            if (entity == null)
            {
                return null;
            }

            var cached = entity;

            if (!executor.TryGetEntity(entityType, keyValues, ref cached, includes, out var cachedIncludes))
            {
                cachedIncludes = false;

                executor.CacheEntity(entityType, key, keyValues, entity, shadowProperties, shadowPropertyValues, includes);
            }

            if (!cachedIncludes)
            {
                foreach (var navigation in includes)
                {
                    FixupNavigation(navigation, entity, cached);
                }
            }

            return cached;
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
            if (accessorInfos == null)
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

                    if (value == null)
                    {
                        continue;
                    }

                    if (ReferenceEquals(value, item))
                    {
                        HandleEntry(executor, ref result, accessorInfo.EntityType);

                        continue;
                    }

                    if (value is IQueryable queryable && queryable.Expression is ConstantExpression constant)
                    {
                        value = constant.Value;
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
            if (entityType == null)
            {
                return;
            }

            var info = executor.GetMaterializationInfo(entity, entityType);

            if (info == null)
            {
                return;
            }

            var stateManager = executor.CurrentDbContext.GetDependencies().StateManager;

            var entry = stateManager.TryGetEntry(info.Key, info.KeyValues);

            var cached = entity;

            if (entry == null)
            {
                var clrType = entity.GetType();

                if (entityType.ClrType != entity.GetType())
                {
                    entityType = entityType.GetDerivedTypes().Single(t => t.ClrType == clrType);
                }

                entry = stateManager.GetOrCreateEntry(entity, entityType);

                for (var i = 0; i < info.ShadowProperties.Length; i++)
                {
                    entry.SetProperty(info.ShadowProperties[i], info.ShadowPropertyValues[i], false);
                }

                stateManager.StartTracking(entry);

                entry.MarkUnchangedFromQuery(null);
            }
            else
            {
                cached = (TEntity)entry.Entity;
            }

            foreach (var set in info.Includes)
            {
                foreach (var navigation in set)
                {
                    entry.SetIsLoaded(navigation);

                    // TODO: See if we can avoid the double fixup

                    FixupNavigation(navigation, entity, cached);
                }
            }

            entity = cached;
        }

        private static void FixupNavigation(INavigation navigation, object entity, object cached)
        {
            var value = navigation.GetGetter().GetClrValue(entity);

            if (value == null)
            {
                return;
            }

            navigation.GetSetter().SetClrValue(cached, value);

            var inverse = navigation.FindInverse();

            if (inverse == null)
            {
                return;
            }

            if (inverse.IsCollection())
            {
                var collection = inverse.GetCollectionAccessor();

                collection.Add(value, cached);
            }
            else
            {
                var setter = inverse.GetSetter();

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
