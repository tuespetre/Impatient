using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.Extensions;
using Impatient.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    var currentSet = new List<List<MemberInfo>> { new List<MemberInfo>() };
                    var paths = new List<List<MemberInfo>>();
                    var inner = call.Arguments[0];
                    var type = inner.Type.GetSequenceType();

                    do
                    {
                        switch (call.Arguments[1].UnwrapLambda() ?? call.Arguments[1])
                        {
                            case LambdaExpression lambdaExpression:
                            {
                                foreach (var path in currentSet)
                                {
                                    path.InsertRange(0, ProcessIncludeLambda(lambdaExpression));
                                }

                                break;
                            }

                            case ConstantExpression constantExpression:
                            {
                                var argument = (string)((ConstantExpression)call.Arguments[1]).Value;
                                var names = argument.Split('.').Select(p => p.Trim()).ToArray();
                                var startCount = currentSet.Count;
                                var resolvedPaths = ResolveIncludePaths(type, names);

                                for (var i = 0; i < startCount; i++)
                                {
                                    foreach (var resolvedPath in resolvedPaths)
                                    {
                                        currentSet.Add(resolvedPath.Concat(currentSet[i]).ToList());
                                    }
                                }

                                currentSet.RemoveRange(0, startCount);

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
                            paths.InsertRange(0, currentSet);
                            currentSet = new List<List<MemberInfo>> { new List<MemberInfo>() };
                        }

                        inner = call.Arguments[0];
                        type = inner.Type.GetSequenceType();
                        call = inner as MethodCallExpression;
                    }
                    while (IsIncludeOrThenIncludeMethod(call?.Method));

                    if (currentSet.Any(p => p.Any()))
                    {
                        paths.InsertRange(0, currentSet);
                    }

                    var innerSequenceType = inner.Type.GetSequenceType();

                    var entityType
                        = model.GetEntityTypes()
                            .Where(t => !t.IsOwned())
                            .FirstOrDefault(t => t.ClrType == innerSequenceType);

                    if (entityType == null)
                    {
                        // TODO:
                        throw new NotSupportedException();
                        /*throw new NotSupportedException(
                            CoreStrings.IncludeNotSpecifiedDirectlyOnEntityType(
                                $"Include(\"{string.Join('.', paths.First().Select(m => m.Name))}\")",
                                paths.First().First().Name));*/
                    }

                    var parameter
                        = Expression.Parameter(
                            innerSequenceType,
                            entityType.GetTableName().Substring(0, 1).ToLower());

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

        private List<List<MemberInfo>> ResolveIncludePaths(Type type, string[] names)
        {
            var entityType
                = model.GetEntityTypes()
                    .SingleOrDefault(t => !t.IsOwned() && t.ClrType == type);

            if (entityType == null)
            {
                // TODO: this
                throw new NotSupportedException();
                /*throw new NotSupportedException(
                    CoreStrings.IncludeNotSpecifiedDirectlyOnEntityType(
                        $"Include(\"{string.Join('.', names)}\")",
                        names[0]));*/
            }

            int depth = 0;

            var paths = ResolveIncludePaths(names, ref depth, ref entityType).Select(p => p.ToList()).ToList();

            if (!paths.Any(p => p.Count() == names.Length))
            {
                // TODO: this
                throw new InvalidOperationException();
                /*throw new InvalidOperationException(
                    CoreStrings.IncludeBadNavigation(names[depth], entityType.DisplayName()));*/
            }

            return paths;
        }

        private List<List<MemberInfo>> ResolveIncludePaths(string[] names, ref int depth, ref IEntityType entityType)
        {
            var navigations = entityType.FindDerivedNavigations(names[depth]);

            if (entityType.FindNavigation(names[depth]) is INavigation nonderived)
            {
                navigations = navigations.Prepend(nonderived);
            }

            var result = new List<List<MemberInfo>>();

            if (!navigations.Any())
            {
                return result;
            }

            navigations = navigations.Distinct();

            depth++;

            if (depth == names.Length)
            {
                depth--;

                result.AddRange(navigations.Select(n => new List<MemberInfo> { n.GetSemanticReadableMemberInfo() }));

                return result;
            }

            foreach (var navigation in navigations)
            {
                var subtype = navigation.TargetEntityType;
                var success = false;
                var resolvedSubpaths = ResolveIncludePaths(names, ref depth, ref subtype);

                foreach (var subpath in resolvedSubpaths)
                {
                    success |= (subpath.Count != 0);

                    subpath.Insert(0, navigation.GetSemanticReadableMemberInfo());

                    result.Add(subpath);
                }

                if (!success)
                {
                    entityType = subtype;
                }

                if (resolvedSubpaths.Count == 0)
                {
                    result.Add(new List<MemberInfo> { navigation.GetSemanticReadableMemberInfo() });
                }
            }

            return result;
        }

        private IEnumerable<MemberInfo> ProcessIncludeLambda(LambdaExpression lambdaExpression)
        {
            IReadOnlyList<PropertyInfo> properties;

            try
            {
                properties
                    = lambdaExpression
                        .GetComplexPropertyAccess(nameof(EntityFrameworkQueryableExtensions.Include));
            }
            catch (ArgumentException argumentException)
            {
                throw new InvalidOperationException(argumentException.Message, argumentException);
            }

            var entityType
                = model.GetEntityTypes()
                    .SingleOrDefault(t => !t.IsOwned() && t.ClrType == lambdaExpression.Parameters[0].Type);

            if (entityType == null)
            {
                // TODO: this
                throw new NotSupportedException();
                /*throw new NotSupportedException(
                    CoreStrings.IncludeNotSpecifiedDirectlyOnEntityType(
                        $"Include(\"{string.Join('.', properties.Select(p => p.Name))}\")",
                        properties.First().Name));*/
            }

            foreach (var property in properties)
            {
                var navigation = entityType.FindNavigation(property);

                if (navigation == null)
                {
                    navigation
                        = entityType
                            .FindDerivedNavigations(property.Name)
                            .SingleOrDefault(n => n.GetSemanticReadableMemberInfo() == property);
                }

                if (navigation == null)
                {
                    // TODO: this
                    throw new InvalidOperationException();
                    /*throw new InvalidOperationException(
                        CoreStrings.IncludeBadNavigation(property, entityType.DisplayName()));*/
                }

                yield return property;

                entityType = navigation.TargetEntityType;
            }
        }

        private IEnumerable<(Expression expression, IList<INavigation> path)> BuildIncludeAccessors(
            IEntityType entityType,
            Expression baseExpression,
            IEnumerable<IEnumerable<MemberInfo>> paths,
            IList<INavigation> previousPath)
        {
            foreach (var pathset in paths.Where(p => p.Any()).GroupBy(p => p.First(), p => p.Skip(1)))
            {
                var includedMember = pathset.Key;

                var navigation = entityType.GetNavigations().FirstOrDefault(n => n.GetSemanticReadableMemberInfo().Equals(includedMember));

                if (navigation == null)
                {
                    // The navigation may be null in some inheritance scenarios.

                    navigation = (from t in entityType.GetDerivedTypes()
                                  from n in t.GetNavigations()
                                  where n.GetSemanticReadableMemberInfo().Equals(includedMember)
                                  select n).FirstOrDefault();

                    if (navigation == null)
                    {
                        // TODO: maybe throw?
                        continue;
                    }
                }

                var currentBaseExpression = baseExpression;

                if (includedMember.DeclaringType.IsSubclassOf(currentBaseExpression.Type))
                {
                    currentBaseExpression = Expression.Convert(currentBaseExpression, includedMember.DeclaringType);
                }

                var includedExpression = Expression.MakeMemberAccess(currentBaseExpression, includedMember) as Expression;

                var currentPath = previousPath.ToList();

                currentPath.Add(navigation);

                if (pathset.Any(p => p.Any()))
                {
                    if (includedMember.GetMemberType().IsSequenceType())
                    {
                        var sequenceType = includedMember.GetMemberType().GetSequenceType();
                        var innerParameter = Expression.Parameter(sequenceType);

                        var innerIncludes
                            = BuildIncludeAccessors(
                                navigation.TargetEntityType,
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

                        if (includedMember.GetMemberType().IsCollectionType())
                        {
                            sequenceExpression = sequenceExpression.AsCollectionType();
                        }

                        yield return (sequenceExpression, currentPath);
                    }
                    else
                    {
                        var innerIncludes
                            = BuildIncludeAccessors(
                                navigation.TargetEntityType,
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

    internal static class ExpressionExtensionsShim
    {
        public static IReadOnlyList<PropertyInfo> GetComplexPropertyAccess(
            this LambdaExpression propertyAccessExpression,
            string methodName)
        {
            if (!TryGetComplexPropertyAccess(propertyAccessExpression, out var propertyPath))
            {
                throw new ArgumentException();
            }

            return propertyPath;
        }

        public static bool TryGetComplexPropertyAccess(
            this LambdaExpression propertyAccessExpression,
            out IReadOnlyList<PropertyInfo> propertyPath)
        {
            Debug.Assert(propertyAccessExpression.Parameters.Count == 1);

            propertyPath
                = propertyAccessExpression
                    .Parameters
                    .Single()
                    .MatchPropertyAccess(propertyAccessExpression.Body);

            return propertyPath != null;
        }

        private static IReadOnlyList<PropertyInfo> MatchPropertyAccess(
            this Expression parameterExpression, 
            Expression propertyAccessExpression)
        {
            var propertyInfos = new List<PropertyInfo>();

            MemberExpression memberExpression;

            do
            {
                memberExpression = RemoveTypeAs(RemoveConvert(propertyAccessExpression)) as MemberExpression;

                if (!(memberExpression?.Member is PropertyInfo propertyInfo))
                {
                    return null;
                }

                propertyInfos.Insert(0, propertyInfo);

                propertyAccessExpression = memberExpression.Expression;
            }
            while (RemoveTypeAs(RemoveConvert(memberExpression.Expression)) != parameterExpression);

            return propertyInfos;
        }

        public static Expression RemoveConvert(this Expression expression)
        {
            while (expression != null
                   && (expression.NodeType == ExpressionType.Convert
                       || expression.NodeType == ExpressionType.ConvertChecked))
            {
                expression = RemoveConvert(((UnaryExpression)expression).Operand);
            }

            return expression;
        }

        public static Expression RemoveTypeAs(this Expression expression)
        {
            while ((expression?.NodeType == ExpressionType.TypeAs))
            {
                expression = RemoveConvert(((UnaryExpression)expression).Operand);
            }

            return expression;
        }
    }
}
