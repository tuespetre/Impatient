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

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure;

public static class EntityTrackingHelper
{
    public static MethodInfo GetEntityUsingStateManagerMethodInfo { get; }
        = typeof(EntityTrackingHelper)
            .GetMethod(nameof(GetEntityUsingStateManager), BindingFlags.NonPublic | BindingFlags.Static);

    public static MethodInfo TrackEntitiesMethodInfo { get; }
        = typeof(EntityTrackingHelper)
            .GetMethod(nameof(TrackEntities), BindingFlags.NonPublic | BindingFlags.Static);

    private static object GetEntityUsingStateManager(
        EFCoreDbCommandExecutor executor,
        bool ephemeral,
        IEntityType entityType,
        object[] keyValues,
        object entity,
        object[] shadowPropertyValues,
        List<INavigationBase> includes)
    {
        var stateManager = ephemeral ? executor.EphemeralStateManager : executor.PersistentStateManager;

        var entry = stateManager.TryGetEntry(entityType.FindPrimaryKey(), keyValues);

        if (entry is null)
        {
            var buffer = shadowPropertyValues.Length == 0
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

                ((IRuntimePropertyBase)include).MaterializationSetter.SetClrValue(entry.Entity, include.GetGetter().GetClrValue(entity));

                entry.SetIsLoaded(include, true);
            }
        }

        return entry.Entity;
    }

    private static IEnumerable TrackEntities(
        IEnumerable source,
        EFCoreDbCommandExecutor executor,
        bool ephemeral,
        MaterializerAccessorInfo[] accessorInfos)
    {
        var stateManager = ephemeral ? executor.EphemeralStateManager : executor.PersistentStateManager;

        foreach (var item in IterateSource(source))
        {
            var result = item;

            foreach (var accessorInfo in accessorInfos)
            {
                var value = accessorInfo.GetValue(item);

                if (value is null || ReferenceEquals(value, item))
                {
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

                    foreach (var subvalue in TrackEntities(list, executor, ephemeral, accessorInfo.SubAccessors))
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

                    foreach (var subvalue in TrackEntities(enumerable, executor, ephemeral, accessorInfo.SubAccessors))
                    {
                    }

                    continue;
                }
            }

            yield return result;
        }

        static IEnumerable IterateSource(IEnumerable source)
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
    }
}
