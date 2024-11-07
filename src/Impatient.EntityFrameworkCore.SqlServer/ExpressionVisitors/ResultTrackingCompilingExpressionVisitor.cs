using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Projection;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors;

public class ResultTrackingCompilingExpressionVisitor : ExpressionVisitor
{
    private readonly IModel model;

    public ResultTrackingCompilingExpressionVisitor(IModel model)
    {
        this.model = model ?? throw new ArgumentNullException(nameof(model));
    }

    public override Expression Visit(Expression node)
    {
        var queryTrackingBehavior = QueryTrackingBehavior.TrackAll;

        if (node is QueryOptionsExpression queryOptionsExpression)
        {
            queryTrackingBehavior = queryOptionsExpression.QueryTrackingBehavior;

            if (queryTrackingBehavior is QueryTrackingBehavior.NoTracking or QueryTrackingBehavior.NoTrackingWithIdentityResolution)
            {
                // remove shadow properties from materialization expressions
                // so they are not pulled from the server.

                node = new ShadowPropertyRemovingExpressionVisitor().Visit(node);

                if (queryTrackingBehavior is QueryTrackingBehavior.NoTracking)
                {
                    return node;
                }
            }
        }

        var unwrapped = node.UnwrapInnerExpression();

        if (unwrapped is RelationalQueryExpression relationalQueryExpression
            && relationalQueryExpression.SelectExpression.Projection is ServerProjectionExpression)
        {
            return node;
        }

        var visitor = new ProjectionBubblingExpressionVisitor();

        if (visitor.Visit(unwrapped) is ProjectionExpression projection)
        {
            var extraCallStack = new Stack<MethodCallExpression>();

            var call = unwrapped as MethodCallExpression;
            var returnType = unwrapped.Type;

            // TODO: Strip predicates and push down into calls to Where
            while (call != null && call.Method.IsQueryableOrEnumerableMethod())
            {
                if (!call.Method.ReturnType.IsSequenceType()
                    || call.Method.ReturnType.IsGenericType(typeof(IGrouping<,>))
                    || call.Method.Name == nameof(Queryable.Cast))
                {
                    node = call.Arguments[0];
                    extraCallStack.Push(call);
                    call = node as MethodCallExpression;
                    returnType = node.Type;
                }
                else
                {
                    call = null;
                }
            }

            // TODO: consider whether this is still needed, but maybe in a modified way
            //var result = new UntrackingExpressionVisitor().Visit(node);
            var result = node;

            var body = projection.Flatten().Body;

            var pathFinder = new PathFindingExpressionVisitor();

            pathFinder.Visit(body);

            if (pathFinder.FoundPaths.Count != 0)
            {
                // The ProjectionBubblingExpressionVisitor might give us some
                // ExpandedGroupings to reference even though the actual result
                // of the query will not be an ExpandedGrouping. 

                result
                    = Expression.Call(
                        typeof(Enumerable)
                            .GetMethod(nameof(Enumerable.Cast))
                            .MakeGenericMethod(returnType.GetSequenceType()),
                        Expression.Call(
                            EntityTrackingHelper.TrackEntitiesMethodInfo,
                            result,
                            Expression.Convert(ExecutionContextParameters.DbCommandExecutor, typeof(EFCoreDbCommandExecutor)),
                            Expression.Constant(queryTrackingBehavior == QueryTrackingBehavior.NoTrackingWithIdentityResolution),
                            Expression.Constant(GenerateAccessors(pathFinder.FoundPaths.Values.ToArray()))));
            }

            while (extraCallStack.TryPop(out call))
            {
                result = call.Update(call.Object, call.Arguments.Skip(1).Prepend(result));
            }

            return result;
        }

        return node;
    }

    private MaterializerAccessorInfo[] GenerateAccessors(MaterializerPathInfo[] pathInfos)
    {
        var accessorInfos = new List<MaterializerAccessorInfo>();

        foreach (var pathInfo in pathInfos)
        {
            var entityTypes = model.FindEntityTypes(pathInfo.Type).ToArray();
            var entityType = entityTypes.FirstOrDefault();

            if (entityTypes.Length > 1)
            {
                var targetMember = pathInfo.Path.Last();
                var resolvedEntityType = default(IEntityType);

                foreach (var candidateType in entityTypes)
                {
                    foreach (var foreignKey in candidateType.GetForeignKeys())
                    {
                        if (foreignKey.IsOwnership
                            && foreignKey.PrincipalToDependent.GetSemanticReadableMemberInfo() == targetMember)
                        {
                            resolvedEntityType = candidateType;
                            goto Resolved;
                        }
                    }
                }

                if (resolvedEntityType is null)
                {
                    throw new InvalidOperationException();
                }

            Resolved:
                entityType = resolvedEntityType;
            }

            // TODO: remove the stuff above?

            var getter = GenerateGetter(pathInfo);

            var setter = GenerateSetter(pathInfo);

            var subAccessors = default(MaterializerAccessorInfo[]);

            if (pathInfo.SubPaths != null)
            {
                subAccessors = GenerateAccessors(pathInfo.SubPaths);
            }

            accessorInfos.Add(new MaterializerAccessorInfo
            {
                EntityType = entityType,
                GetValue = getter,
                SetValue = setter,
                SubAccessors = subAccessors,
            });
        }

        return accessorInfos.ToArray();
    }

    private Func<object, object> GenerateGetter(MaterializerPathInfo pathInfo)
    {
        var blockVariables = new List<ParameterExpression>();
        var blockExpressions = new List<Expression>();
        var parameter = Expression.Parameter(typeof(object));
        var currentExpression = (Expression)parameter;
        var returnLabel = Expression.Label(typeof(object), "Return");
        var nullConstantExpression = Expression.Constant(null, typeof(object));

        for (var i = 0; i < pathInfo.Path.Length; i++)
        {
            var member = pathInfo.Path[i];

            var memberType = member.GetMemberType();

            var memberVariable = Expression.Variable(memberType, member.Name);

            blockVariables.Add(memberVariable);

            if (!member.DeclaringType.IsAssignableFrom(currentExpression.Type))
            {
                currentExpression = Expression.TypeAs(currentExpression, member.DeclaringType);
            }

            blockExpressions.Add(
                Expression.IfThen(
                    Expression.Equal(nullConstantExpression, currentExpression),
                    Expression.Return(returnLabel, Expression.Default(memberType))));

            Expression readExpression = Expression.MakeMemberAccess(currentExpression, GetMemberForRead(member));

            if (!memberType.IsAssignableFrom(readExpression.Type))
            {
                readExpression = Expression.TypeAs(readExpression, memberType);
            }

            blockExpressions.Add(Expression.Assign(memberVariable, readExpression));

            currentExpression = memberVariable;
        }

        blockExpressions.Add(Expression.Label(returnLabel, currentExpression));

        return Expression
            .Lambda<Func<object, object>>(
                Expression.Block(blockVariables, blockExpressions),
                parameter)
            .Compile();
    }

    private Action<object, object> GenerateSetter(MaterializerPathInfo pathInfo)
    {
        var targetParameter = Expression.Parameter(typeof(object));
        var valueParameter = Expression.Parameter(typeof(object));
        var currentExpression = (Expression)targetParameter;
        var lastMember = default(MemberInfo);

        for (var i = 0; i < pathInfo.Path.Length; i++)
        {
            var member = pathInfo.Path[i];

            if (!member.DeclaringType.IsAssignableFrom(currentExpression.Type))
            {
                currentExpression = Expression.Convert(currentExpression, member.DeclaringType);
            }

            if (i + 1 == pathInfo.Path.Length)
            {
                currentExpression = Expression.MakeMemberAccess(currentExpression, member = GetMemberForWrite(member));
            }
            else
            {
                currentExpression = Expression.MakeMemberAccess(currentExpression, member = GetMemberForRead(member));
            }

            lastMember = member;
        }

        Expression body = default;

        if (lastMember is FieldInfo field && field.IsInitOnly)
        {
            body
                = Expression.Call(
                    Expression.Constant(field),
                    typeof(FieldInfo).GetRuntimeMethod(nameof(FieldInfo.SetValue), [typeof(object), typeof(object)]),
                    (currentExpression as MemberExpression).Expression,
                    Expression.Convert(valueParameter, currentExpression.Type));
        }
        else
        {
            body
                = Expression.Assign(
                    currentExpression,
                    Expression.Convert(valueParameter, currentExpression.Type));
        }

        return Expression
            .Lambda<Action<object, object>>(
                body,
                [targetParameter, valueParameter])
            .Compile();
    }

    private MemberInfo GetMemberForRead(MemberInfo memberInfo)
    {
        if (memberInfo.MemberType != MemberTypes.Property)
        {
            return memberInfo;
        }

        var propertyInfo = (PropertyInfo)memberInfo;

        var configuredField = GetNavigationForMember(memberInfo)?.FieldInfo;

        return configuredField ?? propertyInfo.FindBackingField() ?? memberInfo;
    }

    private MemberInfo GetMemberForWrite(MemberInfo memberInfo)
    {
        if (memberInfo.MemberType != MemberTypes.Property)
        {
            return memberInfo;
        }

        var propertyInfo = (PropertyInfo)memberInfo;

        if (!propertyInfo.CanWrite)
        {
            var configuredField = GetNavigationForMember(memberInfo)?.FieldInfo;

            return configuredField ?? propertyInfo.FindBackingField() ?? memberInfo;
        }

        return memberInfo;
    }

    private INavigationBase GetNavigationForMember(MemberInfo targetMember)
    {
        foreach (var candidateType in model.FindEntityTypes(targetMember.GetMemberType()))
        {
            foreach (var foreignKey in candidateType.GetForeignKeys())
            {
                if (foreignKey.PrincipalToDependent?.GetSemanticReadableMemberInfo() == targetMember)
                {
                    return foreignKey.PrincipalToDependent;
                }
                else if (foreignKey.DependentToPrincipal?.GetSemanticReadableMemberInfo() == targetMember)
                {
                    return foreignKey.DependentToPrincipal;
                }
            }
        }

        return null;
    }

    private class UntrackingExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case EntityMaterializationExpression entityMaterializationExpression:
                {
                    return base.Visit(
                        entityMaterializationExpression
                            .UpdateQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution));
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }
    }

    private class PathFindingExpressionVisitor : ProjectionExpressionVisitor
    {
        public Dictionary<string, MaterializerPathInfo> FoundPaths = [];

        private void AddPath(Type type, MaterializerPathInfo[] subpaths = null)
        {
            var name = string.Join('.', GetNameParts());

            if (FoundPaths.TryGetValue(name, out _))
            {
                Debug.Assert(FoundPaths[name].Type.IsAssignableFrom(type));

                return;
            }

            FoundPaths[name] = new MaterializerPathInfo
            {
                Type = type,
                Path = CurrentPath.ToArray(),
                SubPaths = subpaths,
            };
        }

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case EntityMaterializationExpression entityMaterializationExpression:
                {
                    AddPath(node.Type);

                    return base.Visit(node);
                }

                case PolymorphicExpression polymorphicExpression:
                {
                    AddPath(node.Type);

                    foreach (var descriptor in polymorphicExpression.Descriptors)
                    {
                        Visit(descriptor.Materializer.ExpandParameters(polymorphicExpression.Row));
                    }

                    return node;
                }

                case EnumerableRelationalQueryExpression queryExpression:
                {
                    var projection = queryExpression.SelectExpression.Projection;

                    var selectorVisitor = new ProjectionBubblingExpressionVisitor();

                    var result = selectorVisitor.VisitAndConvert(projection, nameof(Visit));

                    var pathFinder = new PathFindingExpressionVisitor();

                    pathFinder.Visit(result.Flatten().Body);

                    AddPath(node.Type, pathFinder.FoundPaths.Values.ToArray());

                    return node;
                }

                case SqlColumnExpression sqlColumnExpression:
                {
                    if (sqlColumnExpression.Table is SubqueryTableExpression subqueryTableExpression)
                    {
                        var projection = subqueryTableExpression.Subquery.Projection.Flatten().Body;

                        if (projection.TryResolvePath(sqlColumnExpression.ColumnName, out var resolved))
                        {
                            Visit(resolved);
                        }
                    }

                    return node;
                }

                case MethodCallExpression methodCallExpression
                when methodCallExpression.Method.IsQueryableOrEnumerableMethod():
                {
                    var selectorVisitor = new ProjectionBubblingExpressionVisitor();

                    if (selectorVisitor.Visit(methodCallExpression) is ProjectionExpression projection)
                    {
                        var pathFinder = new PathFindingExpressionVisitor();

                        pathFinder.Visit(projection.Flatten().Body);

                        if (methodCallExpression.Type.IsSequenceType())
                        {
                            AddPath(node.Type, pathFinder.FoundPaths.Values.ToArray());
                        }
                        else
                        {
                            foreach (var (key, value) in pathFinder.FoundPaths)
                            {
                                FoundPaths.Add(string.Join('.', GetNameParts().Append(key)), value);
                            }
                        }
                    }

                    return node;
                }

                case IncludeExpression includeExpression:
                {
                    Visit(includeExpression.Expression);

                    for (var i = 0; i < includeExpression.Paths.Length; i++)
                    {
                        var include = includeExpression.Includes[i];
                        var path = includeExpression.Paths[i];

                        FoundPaths.Add(
                            string.Join('.', GetNameParts().Concat(path.Select(p => p.Name))),
                            new MaterializerPathInfo
                            {
                                Type = include.Type,
                                Path = CurrentPath.Concat(path.Select(p => p.GetSemanticReadableMemberInfo())).ToArray()
                            });
                    }

                    return includeExpression;
                }
            }

            return base.Visit(node);
        }
    }

    private struct MaterializerPathInfo
    {
        public Type Type;
        public MemberInfo[] Path;
        public MaterializerPathInfo[] SubPaths;
    }

    private class ShadowPropertyRemovingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case SelectExpression select:
                {
                    return select.UpdateProjection(VisitAndConvert(select.Projection, nameof(VisitExtension)));
                }

                case EntityMaterializationExpression entity:
                {
                    return new EntityMaterializationExpression(
                        entity.EntityType,
                        entity.QueryTrackingBehavior,
                        Visit(entity.KeyExpression),
                        Enumerable.Empty<IProperty>(),
                        Enumerable.Empty<Expression>(),
                        Visit(entity.Expression),
                        entity.IncludedNavigations);
                }
                default:
                {
                    return base.VisitExtension(node);
                }
            }
        }
    }
}
