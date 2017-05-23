using Impatient.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors
{
    public class QueryCompilingExpressionVisitor : ExpressionVisitor
    {
        // TODO: Make this configurable
        private static bool complexNestedQueriesSupported = true;

        private readonly ImpatientQueryProvider queryProvider;

        private IEnumerable<ExpressionVisitor> RewritingExpressionVisitors 
            => queryProvider.ExpressionVisitorProvider.RewritingExpressionVisitors;

        public QueryCompilingExpressionVisitor(ImpatientQueryProvider queryProvider)
        {
            this.queryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable)
                || node.Method.DeclaringType == typeof(Enumerable))
            {
                var visitedArguments = new Expression[node.Arguments.Count];

                var outerSource = visitedArguments[0] = Visit(node.Arguments[0]);

                if (outerSource is EnumerableRelationalQueryExpression outerQuery)
                {
                    var isQueryable = node.Method.DeclaringType == typeof(Queryable);

                    switch (node.Method.Name)
                    {
                        // Pass-through operations

                        case nameof(Queryable.AsQueryable):
                        {
                            return outerQuery;
                        }

                        // Projection operations

                        case nameof(Queryable.Select):
                        {
                            var selector = node.Arguments[1].UnwrapLambda();

                            if (selector.Parameters.Count != 1)
                            {
                                // TODO: Add support for the index parameter
                                goto ReturnEnumerableCall;
                            }

                            var outerProjection = outerQuery.SelectExpression.Projection;

                            var selectorBody
                                = selector
                                    .ExpandParameters(outerProjection.Flatten().Body)
                                    .ApplyVisitors(this)
                                    .ApplyVisitors(queryProvider.ExpressionVisitorProvider.RewritingExpressionVisitors);

                            if (selectorBody.IsTranslatable())
                            {
                                return outerQuery
                                    .UpdateSelectExpression(outerQuery.SelectExpression
                                        .UpdateProjection(new ServerProjectionExpression(
                                            Expression.Lambda(selectorBody))));
                            }

                            return outerQuery
                                .UpdateSelectExpression(outerQuery.SelectExpression
                                    .UpdateProjection(outerProjection.Merge(selector)));
                        }

                        case nameof(Queryable.SelectMany):
                        {
                            if (node.Arguments.Count != 3)
                            {
                                // TODO: Support overloads that only use selector
                                goto ReturnEnumerableCall;
                            }

                            var collectionSelector = node.Arguments[1].UnwrapLambda();

                            if (collectionSelector.Parameters.Count != 1)
                            {
                                // TODO: Support overloads that use the index parameter
                                goto ReturnEnumerableCall;
                            }

                            var outerProjection = outerQuery.SelectExpression.Projection.Flatten().Body;

                            // TODO: Inspect whether the outer query needs to be pushed down

                            var projection = collectionSelector.ExpandParameters(outerProjection);
                            var handleAsCorrelated = projection != collectionSelector.Body;
                            var defaultIfEmpty = false;
                            var handleAsJoin = false;
                            var joinPredicate = default(JoinPredicateExpression);
                            var joinExpression = default(Expression);

                            projection 
                                = projection
                                    .ApplyVisitors(RewritingExpressionVisitors)
                                    .ApplyVisitors(this);

                            if (complexNestedQueriesSupported)
                            {
                                if (projection is EnumerableRelationalQueryExpression query)
                                {
                                    var lastMethodCall = collectionSelector.Body as MethodCallExpression;

                                    if (lastMethodCall != null
                                        && (lastMethodCall.Method.DeclaringType == typeof(Enumerable)
                                            || lastMethodCall.Method.DeclaringType == typeof(Queryable))
                                        && lastMethodCall.Method.Name == nameof(Queryable.DefaultIfEmpty)
                                        && query.SelectExpression.Projection.ResultLambda.Body is MetaAliasExpression metaAliasExpression
                                        && metaAliasExpression.AliasExpression.Alias == "$empty")
                                    {
                                        defaultIfEmpty = true;

                                        var subquery = ((SubqueryTableExpression)((LeftJoinExpression)query.SelectExpression.Table).InnerTable).Subquery;
                                        var subqueryProjection = ((MetaAliasExpression)subquery.Projection.ResultLambda.Body).Expression;

                                        query = new EnumerableRelationalQueryExpression(subquery.UpdateProjection(new ServerProjectionExpression(subqueryProjection)));
                                        lastMethodCall = lastMethodCall.Arguments[0] as MethodCallExpression;
                                    }

                                    if (lastMethodCall == null
                                        && query.SelectExpression.Predicate is JoinPredicateExpression)
                                    {
                                        joinPredicate = (JoinPredicateExpression)query.SelectExpression.Predicate;
                                        joinExpression = joinPredicate.Expression;
                                        handleAsJoin = true;
                                        handleAsCorrelated = false;
                                    }

                                    projection = query;
                                }
                            }

                            if (!(projection is EnumerableRelationalQueryExpression innerQuery))
                            {
                                // TODO: Test coverage
                                goto ReturnEnumerableCall;
                            }

                            var resultSelector = node.Arguments[2].UnwrapLambda();
                            var innerSelectExpression = new TableUniquifyingExpressionVisitor().VisitAndConvert(innerQuery.SelectExpression, nameof(VisitMethodCall));
                            var innerTable = innerSelectExpression.Table as AliasedTableExpression;
                            var innerProjection = innerSelectExpression.Projection.Flatten().Body;

                            if (handleAsJoin)
                            {
                                joinPredicate = (JoinPredicateExpression)innerSelectExpression.Predicate;
                                joinExpression = joinPredicate.Expression;
                                innerSelectExpression = innerSelectExpression.RemovePredicate();
                            }

                            var innerRequiresPushdown = innerSelectExpression.RequiresPushdownForRightSideOfJoin() || handleAsCorrelated || defaultIfEmpty;

                            if (innerRequiresPushdown)
                            {
                                if (defaultIfEmpty)
                                {
                                    innerSelectExpression
                                        = innerSelectExpression.UpdateProjection(
                                            new ServerProjectionExpression(Expression.Lambda(
                                                new MetaAliasExpression(
                                                    innerProjection,
                                                    new SqlAliasExpression(Expression.Constant(false), "$empty")))));
                                }

                                innerTable
                                    = new SubqueryTableExpression(
                                        innerSelectExpression,
                                        resultSelector.Parameters[1].Name);

                                innerProjection
                                    = new ProjectionReferenceRewritingExpressionVisitor(innerTable)
                                        .Visit(innerProjection);

                                if (defaultIfEmpty)
                                {
                                    innerProjection
                                        = new DefaultIfEmptyExpression(
                                            innerProjection,
                                            new SqlAliasExpression(
                                                new SqlCastExpression(
                                                    Expression.Coalesce(
                                                        new SqlColumnExpression(innerTable, "$empty", typeof(bool?)),
                                                        Expression.Constant(true)),
                                                    "BIT",
                                                    typeof(bool)),
                                                "$empty"));
                                }
                            }

                            if (handleAsJoin && innerRequiresPushdown)
                            {
                                var tables = innerSelectExpression.Table.Flatten();

                                var binaryPredicate = (BinaryExpression)joinExpression;

                                var newRight = Transform(binaryPredicate.Right, e =>
                                {
                                    if (e is SqlColumnExpression sce && tables.Contains(sce.Table))
                                    {
                                        return new SqlColumnExpression(innerTable, sce.ColumnName, sce.Type, sce.IsNullable);
                                    }

                                    return e;
                                });

                                joinExpression = binaryPredicate.Update(binaryPredicate.Left, binaryPredicate.Conversion, newRight);
                            }

                            var selector 
                                = resultSelector
                                    .ExpandParameters(outerProjection, innerProjection)
                                    .ApplyVisitors(RewritingExpressionVisitors)
                                    .ApplyVisitors(this);

                            var outerTable = outerQuery.SelectExpression.Table;

                            return outerQuery
                                .UpdateSelectExpression(outerQuery.SelectExpression
                                    .UpdateTable(
                                        handleAsJoin
                                            ? defaultIfEmpty
                                                ? new LeftJoinExpression(outerTable, innerTable, joinExpression, selector.Type)
                                                : new InnerJoinExpression(outerTable, innerTable, joinExpression, selector.Type)
                                                    as TableExpression
                                            : handleAsCorrelated
                                                ? defaultIfEmpty
                                                    ? new OuterApplyExpression(outerTable, innerTable, selector.Type)
                                                    : new CrossApplyExpression(outerTable, innerTable, selector.Type)
                                                        as TableExpression
                                                : new CrossJoinExpression(outerTable, innerTable, selector.Type))
                                    .UpdateProjection(
                                        selector.IsTranslatable()
                                            ? new ServerProjectionExpression(Expression.Lambda(selector))
                                            : new CompositeProjectionExpression(
                                                outerQuery.SelectExpression.Projection,
                                                innerSelectExpression.Projection,
                                                resultSelector) as ProjectionExpression));
                        }

                        // Join operations

                        case nameof(Queryable.Join):
                        {
                            if (node.Arguments.Count != 5)
                            {
                                // TODO: Investigate possibility of supporting IEqualityComparer overloads
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            var innerQueryable = visitedArguments[1] = Visit(node.Arguments[1]);

                            if (!(innerQueryable is EnumerableRelationalQueryExpression innerQuery))
                            {
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            // TODO: Inspect whether the outer query needs to be pushed down
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var outerKeyLambda = node.Arguments[2].UnwrapLambda();

                            if (outerSelectExpression.RequiresPushdownForLeftSideOfJoin())
                            {
                                // TODO: What if the outer query's projection is invalid within a subquery?
                                // Are there cases where a Client/Composite projection would be ok?
                                if (!(outerSelectExpression.Projection is ServerProjectionExpression))
                                {
                                    goto ReturnEnumerableCall;
                                }

                                var outerTable
                                    = new SubqueryTableExpression(
                                        outerSelectExpression,
                                        outerKeyLambda.Parameters[0].Name);

                                outerProjection
                                    = new ProjectionReferenceRewritingExpressionVisitor(outerTable)
                                        .Visit(outerProjection);

                                // TODO: What about ordering?
                                outerSelectExpression
                                    = new SelectExpression(
                                        new ServerProjectionExpression(outerProjection),
                                        outerTable);
                            }

                            var outerKeySelector
                                = outerKeyLambda
                                    .ExpandParameters(outerProjection)
                                    .ApplyVisitors(RewritingExpressionVisitors)
                                    .ApplyVisitors(this);

                            if (!outerKeySelector.IsTranslatable())
                            {
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            var innerTable = innerQuery.SelectExpression.Table as AliasedTableExpression;
                            var innerProjection = innerQuery.SelectExpression.Projection.Flatten().Body;
                            var innerKeyLambda = node.Arguments[3].UnwrapLambda();

                            if (innerQuery.SelectExpression.RequiresPushdownForRightSideOfJoin())
                            {
                                innerTable
                                    = new SubqueryTableExpression(
                                        innerQuery.SelectExpression,
                                        innerKeyLambda.Parameters[0].Name);

                                innerProjection
                                    = new ProjectionReferenceRewritingExpressionVisitor((SubqueryTableExpression)innerTable)
                                        .Visit(innerProjection);
                            }

                            var innerKeySelector
                                = innerKeyLambda
                                    .ExpandParameters(innerProjection)
                                    .ApplyVisitors(RewritingExpressionVisitors)
                                    .ApplyVisitors(this);

                            if (!innerKeySelector.IsTranslatable())
                            {
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            var resultLambda 
                                = node.Arguments[4]
                                    .UnwrapLambda();

                            var resultSelector
                                = resultLambda
                                    .ExpandParameters(outerProjection, innerProjection)
                                    .ApplyVisitors(RewritingExpressionVisitors)
                                    .ApplyVisitors(this);

                            return outerQuery
                                .UpdateSelectExpression(outerSelectExpression
                                    .UpdateTable(
                                        new InnerJoinExpression(
                                            outerSelectExpression.Table,
                                            innerTable,
                                            Expression.Equal(outerKeySelector, innerKeySelector),
                                            resultSelector.Type))
                                    .UpdateProjection(
                                        resultSelector.IsTranslatable()
                                            ? new ServerProjectionExpression(Expression.Lambda(resultSelector))
                                            : outerSelectExpression.Projection.Merge(
                                                Expression.Lambda(resultLambda.ApplyVisitors(this)))));
                        }

                        case nameof(Queryable.GroupJoin):
                        {
                            // Not supported as-is -- GroupJoins are optimized away
                            // by the GroupJoinRemovingExpressionVisitor.

                            // TODO: test coverage
                            goto ReturnEnumerableCall;
                        }

                        // Filtering operations

                        case nameof(Queryable.Where):
                        {
                            var predicateLambda = node.Arguments[1].UnwrapLambda();

                            if (predicateLambda.Parameters.Count != 1)
                            {
                                // TODO: add support for index parameter overload
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            var outerProjection = outerQuery.SelectExpression.Projection.Flatten().Body;

                            var predicateBody
                                = predicateLambda
                                    .ExpandParameters(outerProjection)
                                    .ApplyVisitors(RewritingExpressionVisitors)
                                    .ApplyVisitors(this);

                            if (predicateBody.IsTranslatable())
                            {
                                return outerQuery
                                    .UpdateSelectExpression(outerQuery.SelectExpression
                                        .AddToPredicate(predicateBody));
                            }

                            if (predicateBody is BinaryExpression resolvedBinaryPredicate
                                && resolvedBinaryPredicate.NodeType == ExpressionType.AndAlso
                                && predicateLambda.Body is BinaryExpression unresolvedBinaryPredicate)
                            {
                                if (resolvedBinaryPredicate.Left.IsTranslatable())
                                {
                                    visitedArguments[0]
                                        = outerQuery
                                            .UpdateSelectExpression(outerQuery.SelectExpression
                                                .AddToPredicate(resolvedBinaryPredicate.Left));

                                    visitedArguments[1]
                                        = Expression.Lambda(
                                            unresolvedBinaryPredicate.Right,
                                            predicateLambda.Parameters);

                                    goto ReturnEnumerableCall;
                                }

                                if (resolvedBinaryPredicate.Right.IsTranslatable())
                                {
                                    visitedArguments[0]
                                        = outerQuery
                                            .UpdateSelectExpression(outerQuery.SelectExpression
                                                .AddToPredicate(resolvedBinaryPredicate.Right));

                                    visitedArguments[1]
                                        = Expression.Lambda(
                                            unresolvedBinaryPredicate.Left,
                                            predicateLambda.Parameters);

                                    goto ReturnEnumerableCall;
                                }
                            }

                            // TODO: test coverage
                            goto ReturnEnumerableCall;
                        }

                        case nameof(Queryable.OfType):
                        {
                            var inType = outerQuery.SelectExpression.Projection.Type;
                            var outType = node.Method.GetGenericArguments()[0];

                            if (inType == outType)
                            {
                                return outerQuery;
                            }

                            // TODO: Handle downcasting within type hierarchies?
                            // TODO: Inspect whether the outer query needs to be pushed down
                            // TODO: test coverage
                            goto ReturnEnumerableCall;
                        }

                        // Grouping operations

                        case nameof(Queryable.GroupBy):
                        {
                            if (typeof(IEqualityComparer<>) == node.Method.GetParameters().Last().ParameterType.GetGenericTypeDefinition())
                            {
                                // TODO: Investigate possibility of supporting IEqualityComparer overloads
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            // TODO: Inspect whether the outer query needs to be pushed down

                            var outerProjection = outerQuery.SelectExpression.Projection.Flatten().Body;
                            var nodeMethodGenericMethodDefinition = node.Method.GetGenericMethodDefinition();

                            // Key Selector

                            var keySelector
                                = node.Arguments[1]
                                    .UnwrapLambda()
                                    .ExpandParameters(outerProjection)
                                    .ApplyVisitors(RewritingExpressionVisitors)
                                    .ApplyVisitors(this);

                            if (!keySelector.IsTranslatable())
                            {
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            // Element Selector

                            var elementSelector = outerProjection;

                            if (nodeMethodGenericMethodDefinition == groupByKeyElement
                                || nodeMethodGenericMethodDefinition == groupByKeyElementResult)
                            {
                                elementSelector
                                    = node.Arguments[2]
                                        .UnwrapLambda()
                                        .ExpandParameters(elementSelector)
                                        .ApplyVisitors(RewritingExpressionVisitors)
                                        .ApplyVisitors(this);
                            }

                            if (!elementSelector.IsTranslatable())
                            {
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            // Result Selector

                            var resultSelectorLambda
                                = node.Arguments[node.Arguments.Count - 1]
                                    .UnwrapLambda();

                            var resultSelector 
                                = (Expression)new RelationalGroupingExpression(
                                    outerQuery,
                                    keySelector, 
                                    elementSelector);

                            if (nodeMethodGenericMethodDefinition == groupByKeyResult
                                || nodeMethodGenericMethodDefinition == groupByKeyElementResult)
                            {
                                resultSelector
                                    = resultSelectorLambda
                                        .ExpandParameters(keySelector, resultSelector)
                                        .ApplyVisitors(RewritingExpressionVisitors)
                                        .ApplyVisitors(/*new RelationalGroupingExpansionRewritingExpressionVisitor(),*/ this);
                            }

                            if (resultSelector.IsTranslatable())
                            {
                                return outerQuery
                                    .UpdateSelectExpression(outerQuery.SelectExpression
                                        .UpdateProjection(new ServerProjectionExpression(resultSelector))
                                        .UpdateGrouping(keySelector));
                            }

                            goto ReturnEnumerableCall;
                        }

                        // Generation operations

                        case nameof(Queryable.DefaultIfEmpty):
                        {
                            if (node.Arguments.Count > 1)
                            {
                                // TODO: See if we can support the default value argument
                                goto ReturnEnumerableCall;
                            }

                            if (!(outerQuery.SelectExpression.Projection is ServerProjectionExpression))
                            {
                                // TODO: See if we can work around this condition
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            var emptySubquery
                                = new SubqueryTableExpression(
                                    alias: "t",
                                    subquery: new SelectExpression(
                                        new ServerProjectionExpression(
                                            Expression.Lambda(EmptyRecord.NewExpression))));

                            var innerProjectionBody = outerQuery.SelectExpression.Projection.Flatten().Body;

                            var innerSubquery
                                = new SubqueryTableExpression(
                                    alias: "t",
                                    subquery: outerQuery.SelectExpression.UpdateProjection(
                                        new ServerProjectionExpression(Expression.Lambda(
                                            new MetaAliasExpression(
                                                innerProjectionBody,
                                                new SqlAliasExpression(Expression.Constant(false), "$empty"))))));

                            var joinExpression
                                = new LeftJoinExpression(
                                    emptySubquery,
                                    innerSubquery,
                                    Expression.Equal(Expression.Constant(1), Expression.Constant(1)),
                                    innerSubquery.Type);

                            var projectionRewriter = new ProjectionReferenceRewritingExpressionVisitor(innerSubquery);
                            var projectionBody = projectionRewriter.Visit(innerProjectionBody);

                            var selectExpression
                                = new SelectExpression(
                                    new ServerProjectionExpression(Expression.Lambda(
                                        new DefaultIfEmptyExpression(
                                            projectionBody,
                                            new SqlAliasExpression(
                                                new SqlCastExpression(
                                                    Expression.Coalesce(
                                                        new SqlColumnExpression(innerSubquery, "$empty", typeof(bool?)),
                                                        Expression.Constant(true)),
                                                    "BIT",
                                                    typeof(bool)),
                                                "$empty")))),
                                    joinExpression);

                            return new EnumerableRelationalQueryExpression(selectExpression);
                        }

                        // Element operations

                        case nameof(Queryable.First):
                        {
                            // TODO: Inspect whether the outer query needs to be pushed down

                            var selectExpression = outerQuery.SelectExpression;

                            if (node.Arguments.Count == 2)
                            {
                                var outerProjection = selectExpression.Projection.Flatten().Body;

                                var predicate
                                    = node.Arguments[1]
                                        .UnwrapLambda()
                                        .ExpandParameters(outerProjection)
                                        .ApplyVisitors(RewritingExpressionVisitors)
                                        .ApplyVisitors(this);

                                if (!predicate.IsTranslatable())
                                {
                                    // TODO: test coverage
                                    goto ReturnEnumerableCall;
                                }

                                selectExpression = selectExpression.AddToPredicate(predicate);
                            }

                            return Expression.Call(
                                ImpatientExtensions
                                    .GetGenericMethodDefinition((IEnumerable<object> e) => e.First())
                                    .MakeGenericMethod(selectExpression.Type),
                                outerQuery
                                    .UpdateSelectExpression(selectExpression
                                        .UpdateLimit(Expression.Constant(1))));
                        }

                        case nameof(Queryable.FirstOrDefault):
                        {
                            // TODO: Inspect whether the outer query needs to be pushed down
                            // TODO: Let FirstOrDefault run at the server in subqueries

                            var selectExpression = outerQuery.SelectExpression;

                            if (node.Arguments.Count == 2)
                            {
                                var outerProjection = selectExpression.Projection.Flatten().Body;

                                var predicate
                                    = node.Arguments[1]
                                        .UnwrapLambda()
                                        .ExpandParameters(outerProjection)
                                        .ApplyVisitors(RewritingExpressionVisitors)
                                        .ApplyVisitors(this);

                                if (!predicate.IsTranslatable())
                                {
                                    // TODO: test coverage
                                    goto ReturnEnumerableCall;
                                }

                                selectExpression = selectExpression.AddToPredicate(predicate);
                            }

                            return Expression.Call(
                                ImpatientExtensions
                                    .GetGenericMethodDefinition((IEnumerable<object> e) => e.FirstOrDefault())
                                    .MakeGenericMethod(selectExpression.Type),
                                outerQuery
                                    .UpdateSelectExpression(selectExpression
                                        .UpdateLimit(Expression.Constant(1))));
                        }

                        case nameof(Queryable.Last):
                        {
                            if (outerQuery.SelectExpression.OrderBy == null)
                            {
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            // TODO: reverse the OrderBy then apply the same operations as First/FirstOrDefault
                            // TODO: Inspect whether the outer query needs to be pushed down
                            // TODO: test coverage
                            goto ReturnEnumerableCall;
                        }

                        case nameof(Queryable.LastOrDefault):
                        {
                            if (outerQuery.SelectExpression.OrderBy == null)
                            {
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            // TODO: reverse the OrderBy then apply the same operations as First/FirstOrDefault
                            // TODO: Inspect whether the outer query needs to be pushed down
                            // TODO: test coverage
                            goto ReturnEnumerableCall;
                        }

                        case nameof(Queryable.Single):
                        {
                            // TODO: Inspect whether the outer query needs to be pushed down

                            var selectExpression = outerQuery.SelectExpression;

                            if (node.Arguments.Count == 2)
                            {
                                var outerProjection = selectExpression.Projection.Flatten().Body;

                                var predicate
                                    = node.Arguments[1]
                                        .UnwrapLambda()
                                        .ExpandParameters(outerProjection)
                                        .ApplyVisitors(RewritingExpressionVisitors)
                                        .ApplyVisitors(this);

                                if (!predicate.IsTranslatable())
                                {
                                    // TODO: test coverage
                                    goto ReturnEnumerableCall;
                                }

                                selectExpression = selectExpression.AddToPredicate(predicate);
                            }

                            return Expression.Call(
                                ImpatientExtensions
                                    .GetGenericMethodDefinition((IEnumerable<object> e) => e.Single())
                                    .MakeGenericMethod(selectExpression.Type),
                                outerQuery
                                    .UpdateSelectExpression(selectExpression
                                        .UpdateLimit(Expression.Constant(2))));
                        }

                        case nameof(Queryable.SingleOrDefault):
                        {
                            // TODO: Inspect whether the outer query needs to be pushed down
                            var selectExpression = outerQuery.SelectExpression;

                            if (node.Arguments.Count == 2)
                            {
                                var outerProjection = selectExpression.Projection.Flatten().Body;

                                var predicate
                                    = node.Arguments[1]
                                        .UnwrapLambda()
                                        .ExpandParameters(outerProjection)
                                        .ApplyVisitors(RewritingExpressionVisitors)
                                        .ApplyVisitors(this);

                                if (!predicate.IsTranslatable())
                                {
                                    // TODO: test coverage
                                    goto ReturnEnumerableCall;
                                }

                                selectExpression = selectExpression.AddToPredicate(predicate);
                            }

                            return Expression.Call(
                                ImpatientExtensions
                                    .GetGenericMethodDefinition((IEnumerable<object> e) => e.SingleOrDefault())
                                    .MakeGenericMethod(selectExpression.Type),
                                outerQuery
                                    .UpdateSelectExpression(selectExpression
                                        .UpdateLimit(Expression.Constant(2))));
                        }

                        case nameof(Queryable.ElementAt):
                        {
                            // May not ever be supported

                            // TODO: test coverage
                            goto ReturnEnumerableCall;
                        }

                        case nameof(Queryable.ElementAtOrDefault):
                        {
                            // May not ever be supported

                            // TODO: test coverage
                            goto ReturnEnumerableCall;
                        }

                        // Sorting operations

                        case nameof(Queryable.OrderBy):
                        case nameof(Queryable.OrderByDescending):
                        case nameof(Queryable.ThenBy):
                        case nameof(Queryable.ThenByDescending):
                        {
                            if (node.Arguments.Count == 3)
                            {
                                // TODO: Investigate possibility of supporting IComparer overloads
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            // TODO: Inspect whether the outer query needs to be pushed down

                            var outerProjection = outerQuery.SelectExpression.Projection.Flatten().Body;

                            var ordering
                                = node.Arguments[1]
                                    .UnwrapLambda()
                                    .ExpandParameters(outerProjection)
                                    .ApplyVisitors(RewritingExpressionVisitors)
                                    .ApplyVisitors(this);

                            if (!ordering.IsTranslatable() || !ordering.Type.IsScalarType())
                            {
                                // TODO: Convert the existing OrderByExpression, if any,
                                // into a client method call tree, and replace
                                // arguments[1] with it.

                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            var descending
                                = node.Method.Name == nameof(Queryable.OrderByDescending)
                                    || node.Method.Name == nameof(Queryable.ThenByDescending);

                            return outerQuery
                                .UpdateSelectExpression(outerQuery.SelectExpression
                                    .AddToOrderBy(ordering, descending))
                                .AsOrdered();
                        }

                        case nameof(Queryable.Reverse):
                        {
                            if (outerQuery.SelectExpression.OrderBy == null)
                            {
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            // TODO: Inspect whether the outer query needs to be pushed down

                            return outerQuery
                                .UpdateSelectExpression(outerQuery.SelectExpression
                                    .UpdateOrderBy(outerQuery.SelectExpression.OrderBy.Reverse()));
                        }

                        // Partitioning operations

                        case nameof(Queryable.Take):
                        {
                            if (outerQuery.SelectExpression.IsDistinct
                                || outerQuery.SelectExpression.Limit != null)
                            {
                                if (!(outerQuery.SelectExpression.Projection is ServerProjectionExpression))
                                {
                                    goto ReturnEnumerableCall;
                                }

                                var subquery = new SubqueryTableExpression(outerQuery.SelectExpression, "take");

                                var projection
                                    = new ProjectionReferenceRewritingExpressionVisitor(subquery)
                                        .Visit(outerQuery.SelectExpression.Projection.ResultLambda.Body);

                                outerQuery 
                                    = outerQuery.UpdateSelectExpression(
                                        new SelectExpression(
                                            new ServerProjectionExpression(projection), 
                                            subquery));
                            }

                            var count = Visit(node.Arguments[1]);

                            if (count is ConstantExpression)
                            {
                                return outerQuery
                                    .UpdateSelectExpression(outerQuery.SelectExpression
                                        .UpdateLimit(count));
                            }

                            // TODO: test coverage
                            goto ReturnEnumerableCall;
                        }

                        case nameof(Queryable.Skip):
                        {
                            if (outerQuery.SelectExpression.IsDistinct
                                || outerQuery.SelectExpression.Offset != null
                                || outerQuery.SelectExpression.Limit != null)
                            {
                                if (!(outerQuery.SelectExpression.Projection is ServerProjectionExpression))
                                {
                                    goto ReturnEnumerableCall;
                                }

                                var subquery = new SubqueryTableExpression(outerQuery.SelectExpression, "skip");

                                var projection
                                    = new ProjectionReferenceRewritingExpressionVisitor(subquery)
                                        .Visit(outerQuery.SelectExpression.Projection.ResultLambda.Body);

                                outerQuery
                                    = outerQuery.UpdateSelectExpression(
                                        new SelectExpression(
                                            new ServerProjectionExpression(projection),
                                            subquery));
                            }

                            var count = Visit(node.Arguments[1]);

                            if (count is ConstantExpression)
                            {
                                return outerQuery
                                    .UpdateSelectExpression(outerQuery.SelectExpression
                                        .UpdateOffset(count));
                            }

                            // TODO: test coverage
                            goto ReturnEnumerableCall;
                        }

                        case nameof(Queryable.TakeWhile):
                        {
                            // May not ever be supported

                            // TODO: test coverage
                            goto ReturnEnumerableCall;
                        }

                        case nameof(Queryable.SkipWhile):
                        {
                            // May not ever be supported

                            // TODO: test coverage
                            goto ReturnEnumerableCall;
                        }

                        // Conversion operations

                        case nameof(Queryable.Cast):
                        {
                            var inType = outerQuery.SelectExpression.Projection.Type;
                            var outType = node.Method.GetGenericArguments()[0];

                            if (inType == outType)
                            {
                                return outerQuery;
                            }

                            // TODO: Any other casting scenarios?

                            // TODO: test coverage
                            goto ReturnEnumerableCall;
                        }

                        // Set operations

                        case nameof(Queryable.Distinct):
                        {
                            if (node.Arguments.Count != 1)
                            {
                                // TODO: Investigate possibility of supporting IEqualityComparer overloads
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            // TODO: Inspect whether the outer query needs to be pushed down

                            return outerQuery
                                .UpdateSelectExpression(outerQuery.SelectExpression
                                    .AsDistinct());
                        }

                        case nameof(Queryable.Concat):
                        case nameof(Queryable.Except):
                        case nameof(Queryable.Intersect):
                        case nameof(Queryable.Union):
                        {
                            var innerQueryable = Visit(node.Arguments[1]);

                            if (!(innerQueryable is EnumerableRelationalQueryExpression innerQuery))
                            {
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            if (!(outerQuery.SelectExpression.Projection is ServerProjectionExpression
                                && innerQuery.SelectExpression.Projection is ServerProjectionExpression))
                            {
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            // TODO: Constraints on ordering

                            var setOperatorExpression
                                = node.Method.Name == nameof(Queryable.Concat)
                                    ? new UnionAllExpression(outerQuery.SelectExpression, innerQuery.SelectExpression)
                                    : node.Method.Name == nameof(Queryable.Except)
                                        ? new ExceptExpression(outerQuery.SelectExpression, innerQuery.SelectExpression)
                                        : node.Method.Name == nameof(Queryable.Intersect)
                                            ? new IntersectExpression(outerQuery.SelectExpression, innerQuery.SelectExpression)
                                            : node.Method.Name == nameof(Queryable.Union)
                                                ? new UnionExpression(outerQuery.SelectExpression, innerQuery.SelectExpression)
                                                : default(SetOperatorExpression);

                            var projectionRewriter = new ProjectionReferenceRewritingExpressionVisitor(setOperatorExpression);
                            var projectionBody = projectionRewriter.Visit(outerQuery.SelectExpression.Projection.Flatten().Body);

                            var selectExpression
                                = new SelectExpression(
                                    new ServerProjectionExpression(Expression.Lambda(projectionBody)),
                                    setOperatorExpression);

                            return new EnumerableRelationalQueryExpression(selectExpression);
                        }

                        case nameof(Queryable.Zip):
                        {
                            // May not ever be supported

                            // TODO: test coverage
                            goto ReturnEnumerableCall;
                        }

                        // Equality operations

                        case nameof(Queryable.SequenceEqual):
                        {
                            // May not ever be supported

                            // TODO: test coverage
                            goto ReturnEnumerableCall;
                        }

                        // Quantifier operations

                        case nameof(Queryable.All):
                        {
                            var selectExpression = outerQuery.SelectExpression;

                            // TODO: Inspect whether the outer query needs to be pushed down

                            var outerProjection = selectExpression.Projection.Flatten().Body;

                            var predicate
                                = node.Arguments[1]
                                    .UnwrapLambda()
                                    .ExpandParameters(outerProjection)
                                    .ApplyVisitors(RewritingExpressionVisitors)
                                    .ApplyVisitors(this);

                            if (!predicate.IsTranslatable())
                            {
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            // TODO: Consider another approach here (CASE WHEN EXISTS (... EXCEPT ... WHERE) or something)

                            var conditional
                                = new SqlCastExpression(
                                    Expression.Condition(
                                        Expression.Equal(
                                            new SqlFragmentExpression("COUNT_BIG(*)", typeof(long)),
                                            new SqlAggregateExpression(
                                                "SUM",
                                                Expression.Condition(
                                                    predicate,
                                                    Expression.Constant(1),
                                                    Expression.Constant(0)),
                                                typeof(long))),
                                        Expression.Constant(true),
                                        Expression.Constant(false)),
                                    "BIT",
                                    typeof(bool));

                            return new SingleValueRelationalQueryExpression(
                                selectExpression.UpdateProjection(
                                    new ServerProjectionExpression(
                                        Expression.Lambda(conditional))));
                        }

                        case nameof(Queryable.Any):
                        {
                            // TODO: Inspect whether the outer query needs to be pushed down

                            var selectExpression = outerQuery.SelectExpression;

                            if (node.Arguments.Count == 2)
                            {
                                var outerProjection = selectExpression.Projection.Flatten().Body;

                                var predicateBody
                                    = node.Arguments[1]
                                        .UnwrapLambda()
                                        .ExpandParameters(outerProjection)
                                        .ApplyVisitors(RewritingExpressionVisitors)
                                        .ApplyVisitors(this);

                                if (!predicateBody.IsTranslatable())
                                {
                                    // TODO: test coverage
                                    goto ReturnEnumerableCall;
                                }

                                selectExpression = selectExpression.AddToPredicate(predicateBody);
                            }

                            if (!(selectExpression.Projection is ServerProjectionExpression))
                            {
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            var subquery
                                = selectExpression.UpdateProjection(
                                    new ServerProjectionExpression(
                                        Expression.Lambda(Expression.Constant(1))));

                            var existsQueryBody
                                = new SqlCastExpression(
                                    Expression.Condition(
                                        new SqlExistsExpression(subquery),
                                        Expression.Constant(true),
                                        Expression.Constant(false)),
                                    "BIT",
                                    typeof(bool));

                            return new SingleValueRelationalQueryExpression(
                                new SelectExpression(
                                    new ServerProjectionExpression(
                                        Expression.Lambda(existsQueryBody))));
                        }

                        case nameof(Queryable.Contains):
                        {
                            var selectExpression = outerQuery.SelectExpression;

                            if (!(selectExpression.Projection is ServerProjectionExpression))
                            {
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            var valuesExpression = Visit(node.Arguments[1]);

                            switch (valuesExpression)
                            {
                                case SingleValueRelationalQueryExpression singularRelationalQuery:
                                {
                                    throw new NotImplementedException();
                                }

                                case ConstantExpression constant:
                                {
                                    return new SingleValueRelationalQueryExpression(
                                        new SelectExpression(
                                            new ServerProjectionExpression(
                                                Expression.Lambda(
                                                    new SqlCastExpression(
                                                        Expression.Condition(
                                                            new SqlInExpression(constant, selectExpression),
                                                            Expression.Constant(true),
                                                            Expression.Constant(false)),
                                                        "BIT",
                                                        typeof(bool))))));
                                }
                            }

                            // TODO: test coverage
                            goto ReturnEnumerableCall;
                        }

                        // Aggregation operations

                        case nameof(Queryable.Average):
                        case nameof(Queryable.Max):
                        case nameof(Queryable.Min):
                        case nameof(Queryable.Sum):
                        {
                            // TODO: Inspect whether the outer query needs to be pushed down

                            var selectExpression = outerQuery.SelectExpression;

                            if (node.Arguments.Count == 2)
                            {
                                var selector = node.Arguments[1].UnwrapLambda();
                                var outerProjection = selectExpression.Projection.Flatten().Body;

                                var selectorBody
                                    = selector
                                        .ExpandParameters(outerProjection)
                                        .ApplyVisitors(RewritingExpressionVisitors)
                                        .ApplyVisitors(this);

                                if (selectorBody.IsTranslatable())
                                {
                                    selectExpression
                                        = outerQuery.SelectExpression
                                            .UpdateProjection(new ServerProjectionExpression(
                                                Expression.Lambda(selectorBody)));
                                }
                                else
                                {
                                    // TODO: test coverage
                                    goto ReturnEnumerableCall;
                                }
                            }

                            if (!(selectExpression.Projection is ServerProjectionExpression))
                            {
                                // TODO: test coverage
                                goto ReturnEnumerableCall;
                            }

                            var sqlFunctionExpression
                                = node.Method.Name == nameof(Queryable.Average)
                                    ? new SqlFunctionExpression(
                                        "AVG",
                                        node.Method.ReturnType,
                                        new SqlCastExpression(
                                            selectExpression.Projection.Flatten().Body,
                                            selectExpression.Projection.Type == typeof(decimal)
                                                || selectExpression.Projection.Type == typeof(decimal?)
                                                ? "decimal"
                                                : "float",
                                            selectExpression.Projection.Type))
                                    : new SqlFunctionExpression(
                                        node.Method.Name.ToUpperInvariant(),
                                        selectExpression.Projection.Type,
                                        selectExpression.Projection.Flatten().Body);

                            return new SingleValueRelationalQueryExpression(
                                selectExpression.UpdateProjection(
                                    new ServerProjectionExpression(
                                        Expression.Lambda(sqlFunctionExpression))));
                        }

                        case nameof(Queryable.Count):
                        case nameof(Queryable.LongCount):
                        {
                            // TODO: Inspect whether the outer query needs to be pushed down

                            var selectExpression = outerQuery.SelectExpression;

                            if (node.Arguments.Count == 2)
                            {
                                var outerProjection = selectExpression.Projection.Flatten().Body;

                                var predicate
                                    = node.Arguments[1]
                                        .UnwrapLambda()
                                        .ExpandParameters(outerProjection)
                                        .ApplyVisitors(RewritingExpressionVisitors)
                                        .ApplyVisitors(this);

                                if (!predicate.IsTranslatable())
                                {
                                    // TODO: test coverage
                                    goto ReturnEnumerableCall;
                                }

                                selectExpression = selectExpression.AddToPredicate(predicate);
                            }

                            return new SingleValueRelationalQueryExpression(
                                selectExpression.UpdateProjection(
                                    new ServerProjectionExpression(
                                        Expression.Lambda(
                                            node.Method.Name == nameof(Queryable.Count)
                                                ? new SqlFragmentExpression("COUNT(*)", typeof(int))
                                                : new SqlFragmentExpression("COUNT_BIG(*)", typeof(long))))));
                        }

                        case nameof(Queryable.Aggregate):
                        {
                            // May not ever be supported

                            // TODO: test coverage
                            goto ReturnEnumerableCall;
                        }
                    }
                }

                ReturnEnumerableCall:

                return Expression.Call(
                    MatchQueryableMethod(node.Method),
                    node.Arguments
                        .Zip(visitedArguments, (original, visited) => visited ?? Visit(original))
                        .Select(a => a.NodeType == ExpressionType.Quote ? a.UnwrapLambda() : a));
            }

            return base.VisitMethodCall(node);
        }

        private static MethodInfo MatchQueryableMethod(MethodInfo method)
        {
            if (method.DeclaringType == typeof(Enumerable))
            {
                return method;
            }

            var genericMethodDefinition = method.GetGenericMethodDefinition();

            var genericArguments = genericMethodDefinition.GetGenericArguments();

            var parameterTypes
                = genericMethodDefinition.GetParameters()
                    .Select(p =>
                    {
                        if (p.ParameterType.IsConstructedGenericType)
                        {
                            var genericTypeDefinition = p.ParameterType.GetGenericTypeDefinition();

                            if (genericTypeDefinition == typeof(Expression<>))
                            {
                                return p.ParameterType.GenericTypeArguments[0];
                            }
                            else if (genericTypeDefinition == typeof(IQueryable<>))
                            {
                                return typeof(IEnumerable<>).MakeGenericType(p.ParameterType.GenericTypeArguments[0]);
                            }
                        }

                        return p.ParameterType;
                    })
                    .ToArray();

            bool TypesMatch(Type type1, Type type2)
            {
                if (type1 == type2)
                {
                    return true;
                }
                else if (type1.IsConstructedGenericType && type2.IsConstructedGenericType)
                {
                    var genericType1 = type1.GetGenericTypeDefinition();
                    var genericType2 = type2.GetGenericTypeDefinition();

                    return genericType1 == genericType2
                        && type1.GenericTypeArguments.Zip(type2.GenericTypeArguments, TypesMatch).All(b => b);
                }
                else if (type1.IsGenericParameter && type2.IsGenericParameter)
                {
                    return type1.Name == type2.Name
                        && type1.GenericParameterPosition == type2.GenericParameterPosition;
                }
                else
                {
                    return false;
                }
            }

            var matching = (from m in typeof(Enumerable).GetTypeInfo().DeclaredMethods

                            where m.Name == method.Name

                            let parameters = m.GetParameters()
                            where parameters.Length == parameterTypes.Length
                            where m.GetParameters().Select(p => p.ParameterType).Zip(parameterTypes, TypesMatch).All(b => b)

                            let arguments = m.GetGenericArguments()
                            where arguments.Length == genericArguments.Length
                            where arguments.Zip(genericArguments, TypesMatch).All(b => b)

                            select m).ToList();

            return matching.Single().MakeGenericMethod(method.GetGenericArguments());
        }

        private static MethodInfo GetMethod<TResult>(Expression<Func<IQueryable<object>, TResult>> expression)
        {
            return ((MethodCallExpression)expression.Body).Method.GetGenericMethodDefinition();
        }

        private static readonly MethodInfo groupByKey = GetMethod(q => q.GroupBy(x => x));
        private static readonly MethodInfo groupByKeyElement = GetMethod(q => q.GroupBy(x => x, x => x));
        private static readonly MethodInfo groupByKeyResult = GetMethod(q => q.GroupBy(x => x, (x, y) => x));
        private static readonly MethodInfo groupByKeyElementResult = GetMethod(q => q.GroupBy(x => x, x => x, (x, y) => x));

        private class EmptyRecord
        {
            private EmptyRecord(string empty)
            {
                Empty = empty;
            }

            public string Empty { get; }

            public static readonly NewExpression NewExpression
                = Expression.New(
                    typeof(EmptyRecord).GetTypeInfo().DeclaredConstructors.Single(c => !c.IsStatic),
                    new[] { Expression.Constant(null, typeof(string)) },
                    new[] { typeof(EmptyRecord).GetRuntimeProperty(nameof(Empty)) });
        }

        private static Expression Transform(Expression expression, Func<Expression, Expression> transformation)
        {
            return new DelegateExpressionVisitor(transformation).Visit(expression);
        }

        private class DelegateExpressionVisitor : ExpressionVisitor
        {
            private readonly Func<Expression, Expression> transformation;

            public DelegateExpressionVisitor(Func<Expression, Expression> transformation)
            {
                this.transformation = transformation;
            }

            public override Expression Visit(Expression node)
            {
                return transformation(base.Visit(node));
            }
        }
    }
}
