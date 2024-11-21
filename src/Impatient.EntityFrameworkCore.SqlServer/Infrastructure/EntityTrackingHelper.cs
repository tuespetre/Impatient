using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure;

public static class EntityTrackingHelper
{
    public static MethodInfo GetEntityUsingStateManagerMethodInfo { get; }
        = typeof(EntityTrackingHelper)
            .GetMethod(nameof(GetEntityUsingStateManager), BindingFlags.NonPublic | BindingFlags.Static);

    public static MethodInfo NoTrackingInverseFixupMethodInfo { get; }
        = typeof(EntityTrackingHelper)
            .GetMethod(nameof(NoTrackingInverseFixup), BindingFlags.NonPublic | BindingFlags.Static);

    private static object GetEntityUsingStateManager(
        EFCoreDbCommandExecutor executor,
        bool ephemeral,
        IEntityType entityType,
        IKey primaryKey,
        object[] keyValues,
        object entity,
        object[] shadowPropertyValues,
        List<INavigationBase> includes)
    {
        var stateManager
            = ephemeral
                ? executor.EphemeralStateManager
                : executor.PersistentStateManager;

        var entry = stateManager.TryGetEntry(primaryKey, keyValues);

        if (entry is null)
        {
            var buffer
                = shadowPropertyValues.Length == 0
                    ? ValueBuffer.Empty
                    : new ValueBuffer(shadowPropertyValues);

            entry = stateManager.StartTrackingFromQuery(entityType, entity, buffer);

            for (var i = 0; i < includes.Count; i++)
            {
                var include = includes[i];

                entry.SetIsLoaded(include, true);

                SkipNavigationFixup(stateManager, include, entry);
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

                var setter = ((IRuntimePropertyBase)include).MaterializationSetter;
                var getter = include.GetGetter();

                setter.SetClrValue(entry.Entity, getter.GetClrValue(entity));

                entry.SetIsLoaded(include, true);

                SkipNavigationFixup(stateManager, include, entry);
            }
        }

        return entry.Entity;
    }

    private static void SkipNavigationFixup(
        IStateManager stateManager,
        INavigationBase navigation,
        InternalEntityEntry entry)
    {
        if (navigation is not ISkipNavigation skip)
        {
            return;
        }

        var collection = skip.GetGetter().GetClrValue(entry.Entity);

        if (collection is null)
        {
            return;
        }

        var fk1 = skip.ForeignKey;
        var fk2 = skip.Inverse.ForeignKey;
        var props = fk1.Properties.Concat(fk2.Properties).ToList();
        var key = skip.JoinEntityType.FindKey(props);
        var swizzle = false;

        if (key is null)
        {
            props = fk2.Properties.Concat(fk1.Properties).ToList();
            key = skip.JoinEntityType.FindKey(props);

            if (key is null)
            {
                // maybe raise an exception or something?
                return;
            }

            // this probably means the skip navigation is a self-navigation
            swizzle = true;
        }

        var entry1 = entry;

        foreach (var item in (IEnumerable)collection)
        {
            var entry2 = stateManager.TryGetEntry(item, skip.TargetEntityType);

            var keyValues = new object[props.Count];
            var index = 0;

            if (!swizzle)
            {
                foreach (var keyProperty in fk1.PrincipalKey.Properties)
                {
                    keyValues[index++] = entry1[keyProperty];
                }
            }

            foreach (var keyProperty in fk2.PrincipalKey.Properties)
            {
                keyValues[index++] = entry2[keyProperty];
            }

            if (swizzle)
            {
                foreach (var keyProperty in fk1.PrincipalKey.Properties)
                {
                    keyValues[index++] = entry1[keyProperty];
                }
            }

            var joinEntry = stateManager.TryGetEntry(key, keyValues);

            if (joinEntry is null)
            {
                var joinEntity = Activator.CreateInstance(skip.JoinEntityType.ClrType);

                SkipNavigationPropertyFixup(joinEntity, entry1.Entity, fk1);
                SkipNavigationPropertyFixup(joinEntity, entry2.Entity, fk2);

                joinEntry = stateManager.StartTrackingFromQuery(skip.JoinEntityType, joinEntity, ValueBuffer.Empty);
            }

            entry1.AddToCollectionSnapshot(navigation, item);
        }
    }

    private static void SkipNavigationPropertyFixup(
        object dependentEntry,
        object principalEntry,
        IForeignKey foreignKey)
    {
        var principalProperties = foreignKey.PrincipalKey.Properties;
        var dependentProperties = foreignKey.Properties;

        for (var i = 0; i < foreignKey.Properties.Count; i++)
        {
            var principalProperty = principalProperties[i];
            var dependentProperty = dependentProperties[i];

            ((IRuntimePropertyBase)dependentProperty).MaterializationSetter.SetClrValue(dependentEntry, principalProperty.GetGetter().GetClrValue(principalEntry));
        }
    }

    private static object NoTrackingInverseFixup(
        object entity,
        List<INavigationBase> includes)
    {
        for (var i = 0; i < includes.Count; i++)
        {
            var include = includes[i];
            var obj = include.GetGetter().GetClrValue(entity);
            var setter = ((IRuntimePropertyBase)include.Inverse).MaterializationSetter;

            if (obj is not null)
            {
                if (include.IsCollection)
                {
                    foreach (var item in (IEnumerable)obj)
                    {
                        setter.SetClrValue(item, entity);
                    }
                }
                else
                {
                    setter.SetClrValue(obj, entity);
                }
            }
        }

        return entity;
    }
}
