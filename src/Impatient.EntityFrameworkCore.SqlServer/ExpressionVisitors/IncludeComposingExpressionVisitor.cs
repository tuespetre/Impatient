using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.Extensions;
using Impatient.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class IncludeComposingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo queryableSelectMethodInfo
            = ReflectionExtensions.GetGenericMethodDefinition((IQueryable<object> o) => o.Select(x => x));

        private static readonly MethodInfo enumerableSelectMethodInfo
            = ReflectionExtensions.GetGenericMethodDefinition((IEnumerable<object> o) => o.Select(x => x));

        private static readonly MethodInfo queryableCastMethodInfo
            = ReflectionExtensions.GetGenericMethodDefinition((IQueryable o) => o.Cast<object>());

        private static readonly MethodInfo queryableOfTypeMethodInfo
            = ReflectionExtensions.GetGenericMethodDefinition((IQueryable o) => o.OfType<object>());

        private readonly IModel model;
        private readonly DescriptorSet descriptorSet;

        public IncludeComposingExpressionVisitor(
            IModel model,
            DescriptorSet descriptorSet)
        {
            this.model = model;
            this.descriptorSet = descriptorSet;
        }

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case MethodCallExpression call
                when IsIncludeOrThenIncludeMethod(call.Method):
                {
                    var path = new List<PropertyInfo>();
                    var paths = new List<IList<PropertyInfo>> { path };
                    var inner = call.Arguments[0];
                    var type = inner.Type.GetSequenceType();

                    do
                    {
                        switch (call.Arguments[1].UnwrapLambda() ?? call.Arguments[1])
                        {
                            case LambdaExpression lambdaExpression:
                            {
                                path.InsertRange(0, ProcessIncludeLambda(lambdaExpression));

                                break;
                            }

                            case ConstantExpression constantExpression:
                            {
                                var argument = (string)((ConstantExpression)call.Arguments[1]).Value;

                                var names = argument.Split('.').Select(p => p.Trim()).ToArray();

                                path.InsertRange(0, ProcessIncludeString(type, names));

                                break;
                            }

                            default:
                            {
                                throw new NotSupportedException($"Include argument expression of type {call.Arguments[1].NodeType} not supported");
                            }
                        }

                        if (IsIncludeOrThenIncludeMethod(call.Method) && !IsThenIncludeMethod(call.Method))
                        {
                            // Paths are inserted at the beginning to preserve the 
                            // semantic order of includes defined by the query.
                            path = new List<PropertyInfo>();
                            paths.Insert(0, path);
                        }

                        inner = call.Arguments[0];
                        type = inner.Type.GetSequenceType();
                        call = inner as MethodCallExpression;
                    }
                    while (IsIncludeOrThenIncludeMethod(call?.Method));

                    if (path.Count == 0)
                    {
                        paths.Remove(path);
                    }

                    var innerSequenceType = inner.Type.GetSequenceType();

                    var entityType 
                        = model.GetEntityTypes()
                            .Where(t => !t.IsOwned())
                            .FirstOrDefault(t => t.ClrType == innerSequenceType);

                    if (entityType == null)
                    {
                        throw new NotSupportedException(
                            CoreStrings.IncludeNotSpecifiedDirectlyOnEntityType(
                                $"Include(\"{string.Join('.', paths.First().Select(m => m.Name))}\")",
                                paths.First().First().Name));
                    }

                    var parameter 
                        = Expression.Parameter(
                            innerSequenceType, 
                            entityType.Relational().TableName.Substring(0, 1).ToLower());

                    var includeAccessors
                        = BuildIncludeAccessors(
                            entityType,
                            parameter,
                            paths.AsEnumerable(),
                            new INavigation[0]).ToArray();

                    var includeExpression
                        = new IncludeExpression(
                            parameter,
                            includeAccessors.Select(i => i.expression),
                            includeAccessors.Select(i => i.path));

                    return Expression.Call(
                        queryableSelectMethodInfo.MakeGenericMethod(parameter.Type, parameter.Type),
                        Visit(inner),
                        Expression.Lambda(includeExpression, parameter));
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }

        private IEnumerable<PropertyInfo> ProcessIncludeString(Type type, IList<string> names)
        {
            var properties = new List<PropertyInfo>(names.Count);

            foreach (var name in names)
            {
                var member = (from d in descriptorSet.NavigationDescriptors
                              where d.Member.Name == name
                              where d.Member.DeclaringType.IsAssignableFrom(type)
                              select d.Member).FirstOrDefault();

                if (member == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.IncludeNotSpecifiedDirectlyOnEntityType(
                            $"Include(\"{string.Join('.', names)}\")",
                            name));
                }

                properties.Add((PropertyInfo)member);

                type = member.GetMemberType();

                if (type.IsSequenceType())
                {
                    type = type.GetSequenceType();
                }
            }

            return properties;
        }

        private IEnumerable<PropertyInfo> ProcessIncludeLambda(LambdaExpression lambdaExpression)
        {
            var properties = lambdaExpression.GetComplexPropertyAccess();

            var entityType 
                = model.GetEntityTypes()
                    .SingleOrDefault(t => !t.IsOwned() && t.ClrType == lambdaExpression.Parameters[0].Type);

            if (entityType == null)
            {
                throw new NotSupportedException(
                    CoreStrings.IncludeNotSpecifiedDirectlyOnEntityType(
                        $"Include(\"{string.Join('.', properties.Select(p => p.Name))}\")",
                        properties.First().Name));
            }

            foreach (var property in lambdaExpression.GetComplexPropertyAccess())
            {
                var navigation = entityType.FindNavigation(property);

                if (navigation == null)
                {
                    throw new InvalidOperationException(
                        CoreStrings.IncludeBadNavigation(property, entityType.DisplayName()));
                }

                yield return property;

                entityType = navigation.GetTargetType();
            }
        }

        private IEnumerable<(Expression expression, IList<INavigation> path)> BuildIncludeAccessors(
            IEntityType entityType,
            Expression baseExpression,
            IEnumerable<IEnumerable<PropertyInfo>> paths,
            IList<INavigation> previousPath)
        {            
            foreach (var pathset in paths.GroupBy(p => p.First(), p => p.Skip(1)))
            {
                var includedProperty = pathset.Key;

                var navigation = entityType.FindNavigation(includedProperty);

                if (navigation == null)
                {
                    // The navigation may be null in some inheritance scenarios.
                    continue;
                }

                // TODO: Make sure this gets into the include projection rewriting visitor!
                // entityMaterializationExpression = entityMaterializationExpression.IncludeNavigation(navigation);

                var includedExpression = Expression.MakeMemberAccess(baseExpression, includedProperty) as Expression;

                var currentPath = previousPath.ToList();

                currentPath.Add(navigation);

                if (pathset.Any(p => p.Any()))
                {
                    if (includedProperty.GetMemberType().IsSequenceType())
                    {
                        var sequenceType = includedProperty.GetMemberType().GetSequenceType();
                        var innerParameter = Expression.Parameter(sequenceType);

                        var innerIncludes
                            = BuildIncludeAccessors(
                                navigation.GetTargetType(),
                                innerParameter,
                                pathset,
                                new INavigation[0]).ToArray();

                        var includeExpression
                            = new IncludeExpression(
                                innerParameter,
                                innerIncludes.Select(i => i.expression),
                                innerIncludes.Select(i => i.path));

                        var sequenceExpression
                            = (Expression)Expression.Call(
                                enumerableSelectMethodInfo.MakeGenericMethod(sequenceType, sequenceType),
                                includedExpression,
                                Expression.Lambda(includeExpression, innerParameter));
                        
                        yield return (sequenceExpression, currentPath);
                    }
                    else
                    {
                        var innerIncludes 
                            = BuildIncludeAccessors(
                                navigation.GetTargetType(), 
                                includedExpression, 
                                pathset,
                                currentPath);

                        yield return (includedExpression, currentPath);

                        foreach (var innerInclude in innerIncludes)
                        {
                            yield return innerInclude;
                        }
                    }
                }
                else
                {
                    yield return (includedExpression, currentPath);
                }
            }
        }

        private static bool IsIncludeOrThenIncludeMethod(MethodInfo method)
        {
            return method?.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                && method.Name.EndsWith(nameof(EntityFrameworkQueryableExtensions.Include));
        }

        private static bool IsThenIncludeMethod(MethodInfo method)
        {
            return method?.DeclaringType == typeof(EntityFrameworkQueryableExtensions)
                && method.Name.Equals(nameof(EntityFrameworkQueryableExtensions.ThenInclude));
        }
    }
}
