using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
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

            if (!executor.TryGetEntity(entityType.ClrType, keyValues, ref cached, includes, out var includesCached))
            {
                includesCached = false;

                executor.CacheEntity(entityType, key, keyValues, entity, shadowProperties, shadowPropertyValues, includes);
            }

            if (!includesCached)
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
                    else if (ReferenceEquals(value, item))
                    {
                        HandleEntry(executor, ref result, accessorInfo.Type);
                    }
                    else if (value is IList list)
                    {
                        var i = 0;

                        foreach (var subvalue in TrackEntities(list, executor, accessorInfo.SubAccessors))
                        {
                            list[i] = subvalue;
                            i++;
                        }
                    }
                    else if (value is IEnumerable enumerable)
                    {
                        foreach (var subvalue in TrackEntities(enumerable, executor, accessorInfo.SubAccessors))
                        {
                            // no-op
                        }
                    }
                    else
                    {
                        var copy = value;

                        HandleEntry(executor, ref value, accessorInfo.Type);

                        if (!ReferenceEquals(copy, value))
                        {
                            accessorInfo.SetValue?.Invoke(result, value);
                        }
                    }
                }

                yield return result;
            }
        }

        private static void HandleEntry<TEntity>(EFCoreDbCommandExecutor executor, ref TEntity entity, Type type)
        {
            var info = executor.GetMaterializationInfo(entity, type);

            if (info != null)
            {
                var stateManager = executor.CurrentDbContext.GetDependencies().StateManager;

                var entry = stateManager.TryGetEntry(info.Key, info.KeyValues);

                var cached = entity;

                if (entry == null)
                {
                    entry = stateManager.GetOrCreateEntry(entity);

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
        }

        private static void FixupNavigation(INavigation navigation, object entity, object cached)
        {
            var inverse = navigation.FindInverse();

            if (inverse == null)
            {
                return;
            }

            var value = navigation.GetGetter().GetClrValue(entity);

            if (value == null)
            {
                return;
            }

            navigation.GetSetter().SetClrValue(cached, value);

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
