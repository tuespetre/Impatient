using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using System.Collections;
using System.Collections.Generic;
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

                var setter = ((IRuntimePropertyBase)include).MaterializationSetter;
                var getter = include.GetGetter();

                setter.SetClrValue(entry.Entity, getter.GetClrValue(entity));

                entry.SetIsLoaded(include, true);
            }
        }

        return entry.Entity;
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
