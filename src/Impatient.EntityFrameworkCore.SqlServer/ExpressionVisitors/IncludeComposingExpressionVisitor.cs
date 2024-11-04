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

namespace Impatient.EntityFrameworkCore.SqlServer;

public class IncludeComposingExpressionVisitor(IModel model, DescriptorSet descriptorSet) : ExpressionVisitor
{
    private static readonly MethodInfo queryableSelectMethodInfo
        = ReflectionExtensions.GetGenericMethodDefinition((IQueryable<object> o) => o.Select(x => x));

    private static readonly MethodInfo enumerableSelectMethodInfo
        = ReflectionExtensions.GetGenericMethodDefinition((IEnumerable<object> o) => o.Select(x => x));

    private static readonly MethodInfo queryableCastMethodInfo
        = ReflectionExtensions.GetGenericMethodDefinition((IQueryable o) => o.Cast<object>());

    private static readonly MethodInfo queryableOfTypeMethodInfo
        = ReflectionExtensions.GetGenericMethodDefinition((IQueryable o) => o.OfType<object>());

    private readonly IModel model = model;
    private readonly DescriptorSet descriptorSet = descriptorSet;

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (!IsIncludeOrThenIncludeMethod(node.Method))
        {
            return base.VisitMethodCall(node);
        }

        /*
            var filteredBlogs = context.Blogs
                .Include(blog => blog.Posts.Where(post => post.BlogId == 1))
                .ThenInclude(post => post.Author)
                .Include(blog => blog.Posts.Where(post => post.BlogId == 1))
                .ThenInclude(post => post.Tags.OrderBy(postTag => postTag.TagId).Skip(3))
                .ToList();
        */

        // Where, OrderBy, OrderByDescending, ThenBy, ThenByDescending, Skip, Take

        var currentSet = new List<List<MemberInfo>> { new() };
        var paths = new List<List<MemberInfo>>();
        var inner = node.Arguments[0];
        var type = inner.Type.GetSequenceType();

        do
        {
            switch (node.Arguments[1].UnwrapLambda() ?? node.Arguments[1])
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
                    var argument = (string)((ConstantExpression)node.Arguments[1]).Value;
                    var names = argument.Split('.').Select(p => p.Trim()).ToArray();
                    var startCount = currentSet.Count;
                    var resolvedPaths = ResolveIncludePaths(type, names);

                    for (var i = 0; i < startCount; i++)
                    {
                        foreach (var resolvedPath in resolvedPaths)
                        {
                            currentSet.Add([.. resolvedPath, .. currentSet[i]]);
                        }
                    }

                    currentSet.RemoveRange(0, startCount);

                    break;
                }

                default:
                {
                    throw new NotSupportedException($"Include argument expression of type {node.Arguments[1].NodeType} not supported");
                }
            }

            if (IsIncludeOrThenIncludeMethod(node.Method) && !IsThenIncludeMethod(node.Method))
            {
                // Paths are inserted at the beginning to preserve the 
                // semantic order of includes defined by the query.
                paths.InsertRange(0, currentSet);
                currentSet = [[]];
            }

            inner = node.Arguments[0];
            type = inner.Type.GetSequenceType();
            node = inner as MethodCallExpression;
        }
        while (IsIncludeOrThenIncludeMethod(node?.Method));

        if (currentSet.Any(p => p.Count != 0))
        {
            paths.InsertRange(0, currentSet);
        }

        var innerSequenceType = inner.Type.GetSequenceType();

        var entityType = GetEntityTypeForInclude(innerSequenceType);

        var parameter
            = Expression.Parameter(
                innerSequenceType,
                entityType.GetTableName()[..1].ToLower());

        var includeAccessors
            = BuildIncludeAccessors(
                entityType,
                parameter,
                paths.AsEnumerable(),
                Array.Empty<INavigation>()).ToArray();

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

    private List<List<MemberInfo>> ResolveIncludePaths(Type type, string[] names)
    {
        var entityType = GetEntityTypeForInclude(type);

        int depth = 0;

        var paths = ResolveIncludePaths(names, ref depth, ref entityType).Select(p => p.ToList()).ToList();

        if (!paths.Any(p => p.Count == names.Length))
        {
            throw new InvalidOperationException("Include error");
        }

        return paths;
    }

    private static List<List<MemberInfo>> ResolveIncludePaths(string[] names, ref int depth, ref IEntityType entityType)
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

            result.AddRange(navigations.Select(n => new List<MemberInfo> { n.GetIdentifyingMemberInfo() }));

            return result;
        }

        foreach (var navigation in navigations)
        {
            var subtype = navigation.TargetEntityType;
            var success = false;
            var resolvedSubpaths = ResolveIncludePaths(names, ref depth, ref subtype);

            foreach (var subpath in resolvedSubpaths)
            {
                success |= subpath.Count != 0;

                subpath.Insert(0, navigation.GetIdentifyingMemberInfo());

                result.Add(subpath);
            }

            if (!success)
            {
                entityType = subtype;
            }

            if (resolvedSubpaths.Count == 0)
            {
                result.Add([navigation.GetIdentifyingMemberInfo()]);
            }
        }

        return result;
    }

    private IEnumerable<MemberInfo> ProcessIncludeLambda(LambdaExpression lambdaExpression)
    {
        if (!lambdaExpression.TryGetComplexMemberAccess(out var members))
        {
            throw new InvalidOperationException("The specified include expression is not supported.");
        }

        var clrType
            = lambdaExpression.Parameters[0].Type.IsSequenceType()
                ? lambdaExpression.Parameters[0].Type.GetSequenceType()
                : lambdaExpression.Parameters[0].Type;

        var entityType = GetEntityTypeForInclude(clrType);

        foreach (var member in members)
        {
            INavigationBase navigation = entityType.FindNavigation(member);

            if (navigation is null)
            {
                navigation = entityType.FindSkipNavigation(member);
            }

            if (navigation is null)
            {
                navigation
                    = entityType
                        .FindDerivedNavigations(member.Name)
                        .SingleOrDefault(n => n.PropertyInfo == member || n.FieldInfo == member);
            }

            if (navigation is null)
            {
                throw new InvalidOperationException("The specified include expression does not reference a defined navigation.");
            }

            yield return member;

            entityType = navigation.TargetEntityType;
        }
    }

    private static IEnumerable<(Expression expression, IList<INavigation> path)> BuildIncludeAccessors(
        IEntityType entityType,
        Expression baseExpression,
        IEnumerable<IEnumerable<MemberInfo>> paths,
        IList<INavigation> previousPath)
    {
        foreach (var pathset in paths.Where(p => p.Any()).GroupBy(p => p.First(), p => p.Skip(1)))
        {
            var includedMember = pathset.Key;

            var navigation = entityType.GetNavigations().FirstOrDefault(n => n.GetSemanticReadableMemberInfo().Equals(includedMember));

            if (navigation is null)
            {
                // The navigation may be null in some inheritance scenarios.

                navigation = (from t in entityType.GetDerivedTypes()
                              from n in t.GetNavigations()
                              where n.GetSemanticReadableMemberInfo().Equals(includedMember)
                              select n).FirstOrDefault();

                if (navigation is null)
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
                            Array.Empty<INavigation>()).ToArray();

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

    private IEntityType GetEntityTypeForInclude(Type type)
    {
        var entityType = model.GetEntityTypes().SingleOrDefault(t => t.ClrType == type);

        if (entityType is null)
        {
            throw new InvalidOperationException($"Unable to include entity of unmapped type: {type.FullName}");
        }

        return entityType;
    }
}

internal static class ExpressionExtensionsShim
{
    public static bool TryGetComplexMemberAccess(
        this LambdaExpression memberAccessLambda,
        out IReadOnlyList<MemberInfo> memberPath)
    {
        Debug.Assert(memberAccessLambda.Parameters.Count == 1);

        memberPath
            = memberAccessLambda
                .Parameters
                .Single()
                .MatchMemberAccess(memberAccessLambda.Body);

        return memberPath is not null;
    }

    private static List<MemberInfo> MatchMemberAccess(
        this Expression parameterExpression, 
        Expression memberAccessExpression)
    {
        var members = new List<MemberInfo>();

        MemberExpression memberExpression;

        do
        {
            memberExpression = RemoveTypeAs(RemoveConvert(memberAccessExpression)) as MemberExpression;

            var member = memberExpression?.Member;

            if (member is not (PropertyInfo or FieldInfo))
            {
                return null;
            }

            members.Insert(0, member);

            memberAccessExpression = memberExpression.Expression;
        }
        while (RemoveTypeAs(RemoveConvert(memberExpression.Expression)) != parameterExpression);

        return members;
    }

    public static Expression RemoveConvert(this Expression expression)
    {
        while (expression is not null
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
