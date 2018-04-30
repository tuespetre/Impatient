using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Rewriting;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.Extensions.ReflectionExtensions;
using static System.Linq.Enumerable;

namespace Impatient.Query.ExpressionVisitors.Composing
{
    public class QueryComposingExpressionVisitor : ExpressionVisitor
    {
        private readonly TranslatabilityAnalyzingExpressionVisitor translatabilityVisitor;
        private readonly IEnumerable<ExpressionVisitor> rewritingExpressionVisitors;
        private readonly ExpressionVisitor parameterizingExpressionVisitor;

        // TODO: Use separate visitors to be stateless/remove the topLevel condition.
        private bool topLevel = true;

        public QueryComposingExpressionVisitor(
            TranslatabilityAnalyzingExpressionVisitor translatabilityVisitor,
            IEnumerable<ExpressionVisitor> rewritingExpressionVisitors,
            ExpressionVisitor parameterizingExpressionVisitor)
        {
            this.translatabilityVisitor = translatabilityVisitor ?? throw new ArgumentNullException(nameof(translatabilityVisitor));
            this.rewritingExpressionVisitors = rewritingExpressionVisitors ?? throw new ArgumentNullException(nameof(rewritingExpressionVisitors));
            this.parameterizingExpressionVisitor = parameterizingExpressionVisitor;
        }

        private IEnumerable<ExpressionVisitor> PostExpansionVisitors
        {
            get
            {
                yield return parameterizingExpressionVisitor;

                foreach (var rewritingVisitor in rewritingExpressionVisitors)
                {
                    yield return rewritingVisitor;
                }

                yield return new SqlParameterRewritingExpressionVisitor2();

                yield return this;
            }
        }

        private IEnumerable<ExpressionVisitor> ClientPostExpansionVisitors
        {
            get
            {
                yield return parameterizingExpressionVisitor;

                yield return new SqlParameterRewritingExpressionVisitor2();

                yield return this;
            }
        }

        public override Expression Visit(Expression node)
        {
            if (topLevel)
            {
                topLevel = false;

                var visited = base.Visit(node);

                topLevel = true;

                visited
                    = new GroupExpandingExpressionVisitor(
                        translatabilityVisitor,
                        PostExpansionVisitors)
                        .Visit(visited);

                return visited;
            }

            return base.Visit(node);
        }

        protected override Expression VisitLambda<T>(Expression<T> node)
        {
            if (node.ReturnType.IsGenericType(typeof(IQueryable<>)))
            {
                var parameters = node.Parameters.Select(p => VisitAndConvert(p, nameof(VisitLambda))).ToArray();
                var body = Visit(node.Body);

                if (!body.Type.IsGenericType(typeof(IQueryable<>)))
                {
                    body
                        = Expression.Call(
                            GetGenericMethodDefinition((IEnumerable<object> o) => o.AsQueryable())
                                .MakeGenericMethod(body.Type.GetSequenceType()),
                            body);
                }

                return node.Update(body, parameters);
            }

            return base.VisitLambda(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.IsQueryableOrEnumerableMethod())
            {
                var visitedArguments = new Expression[node.Arguments.Count];

                var outerSource = visitedArguments[0] = ProcessQuerySource(Visit(node.Arguments[0]));

                if (outerSource is EnumerableRelationalQueryExpression outerQuery
                    && !node.Method.HasComparerArgument()
                    && !node.ContainsNonLambdaDelegates()
                    && !node.ContainsNonLambdaExpressions())
                {
                    switch (node.Method.Name)
                    {
                        // Pass-through operations

                        case nameof(Queryable.AsQueryable):
                        case nameof(Enumerable.ToArray):
                        case nameof(Enumerable.ToList):
                        {
                            // TODO: Handle ToHashSet
                            return outerQuery.WithTransformationMethod(node.Method);
                        }

                        // Materialization operations

                        case nameof(Enumerable.ToDictionary):
                        case nameof(Enumerable.ToLookup):
                        {
                            // TODO: Handle ToDictionary
                            // TODO: Handle ToLookup
                            break;
                        }

                        // Projection operations

                        case nameof(Queryable.Select):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var selectorLambda = node.Arguments[1].UnwrapLambda();

                            var referencesIndexParameter
                                = selectorLambda.Parameters.Count == 2
                                    && selectorLambda.Body.References(selectorLambda.Parameters[1]);

                            if (!HandlePushdown(
                                s => s.IsDistinct || referencesIndexParameter && (s.Limit != null || s.Offset != null),
                                selectorLambda.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            if (selectorLambda.Parameters.Count == 1)
                            {
                                var selectorBody
                                    = selectorLambda
                                        .ExpandParameters(outerProjection)
                                        .VisitWith(PostExpansionVisitors);

                                if (IsTranslatable(selectorBody))
                                {
                                    return outerQuery
                                        .UpdateSelectExpression(outerSelectExpression
                                            .UpdateProjection(new ServerProjectionExpression(
                                                selectorBody)));
                                }
                                else
                                {
                                    return outerQuery
                                        .UpdateSelectExpression(outerSelectExpression
                                            .UpdateProjection(outerSelectExpression.Projection
                                                .Merge(Expression.Lambda(
                                                    selectorLambda.Body,
                                                    selectorLambda.Parameters[0]))))
                                        .VisitWith(ClientPostExpansionVisitors);
                                }
                            }
                            else
                            {
                                var selectorBody
                                    = selectorLambda
                                        .ExpandParameters(
                                            outerProjection,
                                            CreateRowNumberExpression(outerSelectExpression))
                                        .VisitWith(PostExpansionVisitors);

                                if (IsTranslatable(selectorBody))
                                {
                                    return outerQuery
                                        .UpdateSelectExpression(outerSelectExpression
                                            .UpdateProjection(new ServerProjectionExpression(selectorBody))
                                            .AsWindowed());
                                }
                                else
                                {
                                    return FallbackToEnumerable();
                                }
                            }
                        }

                        case nameof(Queryable.SelectMany):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var collectionSelectorLambda = node.Arguments[1].UnwrapLambda();
                            var resultSelectorLambda = node.Arguments.ElementAtOrDefault(2)?.UnwrapLambda();

                            var referencesIndexParameter
                                = collectionSelectorLambda.Parameters.Count == 2
                                    && collectionSelectorLambda.Body.References(collectionSelectorLambda.Parameters[1]);

                            if (referencesIndexParameter)
                            {
                                outerProjection
                                    = RowNumberTuple.Create(
                                        outerProjection,
                                        CreateRowNumberExpression(outerSelectExpression));

                                outerSelectExpression
                                    = outerSelectExpression
                                        .UpdateProjection(new ServerProjectionExpression(outerProjection))
                                        .AsWindowed();
                            }

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForLeftSideOfJoin(),
                                resultSelectorLambda?.Parameters[0].Name ?? collectionSelectorLambda.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var expansionParameters = new[] { outerProjection };

                            if (referencesIndexParameter)
                            {
                                var liftedRowNumberTuple = outerProjection as NewExpression;

                                outerProjection = liftedRowNumberTuple.Arguments[0];

                                outerSelectExpression
                                    = outerSelectExpression
                                        .UpdateProjection(new ServerProjectionExpression(
                                            outerProjection));

                                expansionParameters = new[] { outerProjection, liftedRowNumberTuple.Arguments[1] };
                            }

                            var innerSource = collectionSelectorLambda.ExpandParameters(expansionParameters);
                            var handleAsCorrelated = innerSource != collectionSelectorLambda.Body;
                            var handleAsJoin = false;
                            var outerKeySelector = default(Expression);
                            var innerKeyLambda = default(LambdaExpression);
                            var defaultIfEmpty = false;

                            if (innerSource is MethodCallExpression innerSourceMethodCall
                                && innerSourceMethodCall.Method.IsQueryableOrEnumerableMethod()
                                && innerSourceMethodCall.Method.Name == nameof(Queryable.DefaultIfEmpty)
                                && innerSourceMethodCall.Arguments.Count == 1)
                            {
                                defaultIfEmpty = true;
                                innerSource = innerSourceMethodCall.Arguments[0];
                            }

                            if (innerSource is GroupedRelationalQueryExpression groupedRelationalQueryExpression)
                            {
                                handleAsCorrelated = false;
                                handleAsJoin = true;

                                outerKeySelector = groupedRelationalQueryExpression.OuterKeySelector;
                                innerKeyLambda = groupedRelationalQueryExpression.InnerKeyLambda;

                                innerSource
                                    = new EnumerableRelationalQueryExpression(
                                        groupedRelationalQueryExpression.SelectExpression);
                            }

                            innerSource = ProcessQuerySource(innerSource).VisitWith(PostExpansionVisitors);

                            if (!(innerSource is EnumerableRelationalQueryExpression innerQuery))
                            {
                                return FallbackToEnumerable();
                            }

                            var innerSelectExpression
                                = new TableUniquifyingExpressionVisitor()
                                    .VisitAndConvert(innerQuery.SelectExpression, nameof(VisitMethodCall));

                            var innerProjection = innerSelectExpression.Projection.Flatten().Body;

                            var innerRequiresPushdown
                                = innerSelectExpression.RequiresPushdownForRightSideOfJoin()
                                    || handleAsCorrelated
                                    || defaultIfEmpty;

                            if (defaultIfEmpty)
                            {
                                if (!(innerProjection is DefaultIfEmptyExpression))
                                {
                                    innerProjection
                                        = new DefaultIfEmptyExpression(innerProjection);
                                }

                                innerSelectExpression
                                    = innerSelectExpression
                                        .UpdateProjection(new ServerProjectionExpression(
                                            innerProjection));
                            }

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForRightSideOfJoin() || handleAsCorrelated || defaultIfEmpty,
                                resultSelectorLambda?.Parameters[1].Name,
                                ref innerSelectExpression,
                                ref innerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var joinPredicate = default(Expression);

                            if (handleAsJoin)
                            {
                                var innerKeySelector = innerKeyLambda.ExpandParameters(innerProjection);

                                joinPredicate
                                    = Expression
                                        .Equal(
                                            JoinKeyDenullifyingExpressionVisitor.Instance.Visit(outerKeySelector),
                                            JoinKeyDenullifyingExpressionVisitor.Instance.Visit(innerKeySelector))
                                        .VisitWith(PostExpansionVisitors);

                                if (!IsTranslatable(joinPredicate))
                                {
                                    return FallbackToEnumerable();
                                }
                            }

                            var selector
                                = (resultSelectorLambda == null
                                    ? innerProjection
                                    : resultSelectorLambda.ExpandParameters(
                                        outerProjection,
                                        innerProjection))
                                    .VisitWith(PostExpansionVisitors);

                            var projection
                                = resultSelectorLambda == null || IsTranslatable(selector)
                                    ? new ServerProjectionExpression(selector)
                                    : new CompositeProjectionExpression(
                                        outerSelectExpression.Projection,
                                        innerSelectExpression.Projection,
                                        resultSelectorLambda) as ProjectionExpression;

                            var outerTable = outerSelectExpression.Table;
                            var innerTable = (AliasedTableExpression)innerSelectExpression.Table;

                            // TODO: Figure out a way to handle providers that do not support correlated subqueries

                            var joinExpression
                                = handleAsJoin
                                    ? defaultIfEmpty
                                        ? new LeftJoinExpression(outerTable, innerTable, joinPredicate, selector.Type)
                                        : new InnerJoinExpression(outerTable, innerTable, joinPredicate, selector.Type)
                                            as TableExpression
                                    : handleAsCorrelated
                                        ? defaultIfEmpty
                                            ? new OuterApplyExpression(outerTable, innerTable, selector.Type)
                                            : new CrossApplyExpression(outerTable, innerTable, selector.Type)
                                                as TableExpression
                                        : new CrossJoinExpression(outerTable, innerTable, selector.Type);

                            return outerQuery
                                .UpdateSelectExpression(outerSelectExpression
                                    .UpdateProjection(projection)
                                    .UpdateTable(joinExpression));
                        }

                        // Join operations

                        case nameof(Queryable.Join):
                        {
                            var innerSource = visitedArguments[1] = ProcessQuerySource(Visit(node.Arguments[1]));

                            if (!(innerSource is EnumerableRelationalQueryExpression innerQuery))
                            {
                                return FallbackToEnumerable();
                            }

                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var outerKeyLambda = node.Arguments[2].UnwrapLambda();

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForLeftSideOfJoin(),
                                outerKeyLambda.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var outerKeySelector
                                = outerKeyLambda
                                    .ExpandParameters(outerProjection)
                                    .VisitWith(PostExpansionVisitors);

                            if (!IsTranslatable(outerKeySelector))
                            {
                                return FallbackToEnumerable();
                            }

                            var innerSelectExpression = innerQuery.SelectExpression;
                            var innerProjection = innerSelectExpression.Projection.Flatten().Body;
                            var innerKeyLambda = node.Arguments[3].UnwrapLambda();

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForRightSideOfJoin(),
                                innerKeyLambda.Parameters[0].Name,
                                ref innerSelectExpression,
                                ref innerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var innerKeySelector
                                = innerKeyLambda
                                    .ExpandParameters(innerProjection)
                                    .VisitWith(PostExpansionVisitors);

                            if (!IsTranslatable(innerKeySelector))
                            {
                                return FallbackToEnumerable();
                            }

                            var resultLambda
                                = node.Arguments[4]
                                    .UnwrapLambda();

                            var resultSelector
                                = resultLambda
                                    .ExpandParameters(outerProjection, innerProjection)
                                    .VisitWith(PostExpansionVisitors);

                            var projection
                                = IsTranslatable(resultSelector)
                                    ? new ServerProjectionExpression(resultSelector)
                                    : new CompositeProjectionExpression(
                                        outerSelectExpression.Projection,
                                        new ServerProjectionExpression(innerProjection),
                                        resultLambda) as ProjectionExpression;

                            var table
                                = new InnerJoinExpression(
                                    outerSelectExpression.Table,
                                    innerSelectExpression.Table as AliasedTableExpression,
                                    Expression.Equal(
                                        JoinKeyDenullifyingExpressionVisitor.Instance.Visit(outerKeySelector),
                                        JoinKeyDenullifyingExpressionVisitor.Instance.Visit(innerKeySelector)),
                                    resultSelector.Type);

                            return outerQuery
                                .UpdateSelectExpression(outerSelectExpression
                                    .UpdateProjection(projection)
                                    .UpdateTable(table));
                        }

                        case nameof(Queryable.GroupJoin):
                        {
                            var innerSource = visitedArguments[1] = ProcessQuerySource(Visit(node.Arguments[1]));

                            if (!(innerSource is EnumerableRelationalQueryExpression innerQuery))
                            {
                                return FallbackToEnumerable();
                            }

                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var outerKeyLambda = node.Arguments[2].UnwrapLambda();

                            var outerKeySelector
                                = outerKeyLambda
                                    .ExpandParameters(outerProjection)
                                    .VisitWith(PostExpansionVisitors);

                            if (!IsTranslatable(outerKeySelector))
                            {
                                return FallbackToEnumerable();
                            }

                            var innerSelectExpression = innerQuery.SelectExpression;
                            var innerProjection = innerSelectExpression.Projection.Flatten().Body;
                            var innerKeyLambda = node.Arguments[3].UnwrapLambda();

                            var innerKeySelector
                                = innerKeyLambda
                                    .ExpandParameters(innerProjection)
                                    .VisitWith(PostExpansionVisitors);

                            if (!IsTranslatable(innerKeySelector))
                            {
                                return FallbackToEnumerable();
                            }

                            var resultLambda
                                = node.Arguments[4]
                                    .UnwrapLambda();

                            innerProjection
                                = new GroupedRelationalQueryExpression(
                                    innerSelectExpression,
                                    outerKeySelector,
                                    innerKeySelector,
                                    innerKeyLambda,
                                    type: resultLambda.Parameters[1].Type,
                                    requiresDenullification: true);

                            var resultSelector
                                = resultLambda
                                    .ExpandParameters(
                                        outerProjection,
                                        innerProjection)
                                    .VisitWith(PostExpansionVisitors);

                            if (IsTranslatable(resultSelector))
                            {
                                return outerQuery
                                    .UpdateSelectExpression(outerSelectExpression
                                        .UpdateProjection(new ServerProjectionExpression(
                                            resultSelector)));
                            }

                            innerProjection
                                = new GroupedRelationalQueryExpression(
                                    innerSelectExpression,
                                    outerKeyLambda.Body.Replace(
                                        outerKeyLambda.Parameters[0],
                                        resultLambda.Parameters[0]),
                                    innerKeySelector,
                                    innerKeyLambda,
                                    type: resultLambda.Parameters[1].Type,
                                    requiresDenullification: true);

                            resultLambda
                                = Expression.Lambda(
                                    name: "GroupJoinClientResultSelector",
                                    body: resultLambda.Body.Replace(resultLambda.Parameters[1], innerProjection),
                                    parameters: new[] { resultLambda.Parameters[0] });

                            return outerQuery
                                .UpdateSelectExpression(outerSelectExpression
                                    .UpdateProjection(outerSelectExpression.Projection
                                        .Merge(resultLambda)))
                                .VisitWith(ClientPostExpansionVisitors);
                        }

                        // Filtering operations

                        case nameof(Queryable.Where):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var predicateLambda = node.Arguments[1].UnwrapLambda();

                            var referencesIndexParameter
                                = predicateLambda.Parameters.Count == 2
                                    && predicateLambda.Body.References(predicateLambda.Parameters[1]);

                            if (referencesIndexParameter)
                            {
                                outerProjection
                                    = RowNumberTuple.Create(
                                        outerProjection,
                                        CreateRowNumberExpression(outerSelectExpression));

                                outerSelectExpression
                                    = outerSelectExpression
                                        .UpdateProjection(new ServerProjectionExpression(outerProjection))
                                        .AsWindowed();
                            }

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForPredicate(),
                                predicateLambda.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var expansionParameters = new[] { outerProjection };

                            if (referencesIndexParameter)
                            {
                                var liftedRowNumberTuple = outerProjection as NewExpression;

                                outerProjection = liftedRowNumberTuple.Arguments[0];

                                outerSelectExpression
                                    = outerSelectExpression
                                        .UpdateProjection(new ServerProjectionExpression(outerProjection));

                                expansionParameters = new[] { outerProjection, liftedRowNumberTuple.Arguments[1] };
                            }

                            var predicateBody
                                = predicateLambda
                                    .ExpandParameters(expansionParameters)
                                    .VisitWith(PostExpansionVisitors);

                            if (IsTranslatable(predicateBody))
                            {
                                return outerQuery
                                    .UpdateSelectExpression(outerSelectExpression
                                        .AddToPredicate(predicateBody));
                            }

                            if (predicateLambda.Body is BinaryExpression unresolvedBinaryExpression)
                            {
                                var parts
                                    = (from unresolved in unresolvedBinaryExpression.SplitNodes(ExpressionType.AndAlso)
                                       let lambda = Expression.Lambda(unresolved, predicateLambda.Parameters)
                                       let resolved = lambda.ExpandParameters(expansionParameters).VisitWith(PostExpansionVisitors)
                                       let translatable = IsTranslatable(resolved)
                                       let expression = translatable ? resolved : unresolved
                                       select new { expression, translatable }).ToArray();

                                if (parts.Any(p => p.translatable))
                                {
                                    var resolvedPredicate
                                        = parts
                                            .Where(p => p.translatable)
                                            .Select(p => p.expression)
                                            .Aggregate(Expression.AndAlso);

                                    if (resolvedPredicate is BinaryExpression resolvedBinary)
                                    {
                                        resolvedPredicate = resolvedBinary.Balance();
                                    }

                                    var unresolvedPredicate
                                        = parts
                                            .Where(p => !p.translatable)
                                            .Select(p => p.expression)
                                            .Aggregate(Expression.AndAlso);

                                    if (unresolvedPredicate is BinaryExpression unresolvedBinary)
                                    {
                                        unresolvedPredicate = unresolvedBinary.Balance();
                                    }

                                    visitedArguments[0]
                                        = outerQuery
                                            .UpdateSelectExpression(outerSelectExpression
                                                .AddToPredicate(resolvedPredicate));

                                    visitedArguments[1]
                                        = Expression.Lambda(
                                            unresolvedPredicate,
                                            predicateLambda.Parameters);
                                }
                            }

                            return FallbackToEnumerable();
                        }

                        case nameof(Queryable.OfType):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;

                            var inType = outerProjection.Type;
                            var outType = node.Method.GetGenericArguments()[0];

                            if (inType == outType)
                            {
                                return outerQuery;
                            }

                            // TODO: Test polymorphism in a nested complex subquery
                            if (outerProjection is PolymorphicExpression originalPolymorphicExpression)
                            {
                                if (!HandlePushdown(
                                    s => s.RequiresPushdownForPredicate(),
                                    null,
                                    ref outerSelectExpression,
                                    ref outerProjection))
                                {
                                    return FallbackToEnumerable();
                                }

                                var polymorphicExpression
                                    = (outerProjection.UnwrapInnerExpression() as PolymorphicExpression)
                                        .Filter(outType);

                                var predicate
                                    = polymorphicExpression
                                        .Descriptors
                                        .Select(d => d.Test.ExpandParameters(polymorphicExpression.Row))
                                        .Aggregate(Expression.OrElse);

                                var newProjectionExpression
                                    = outerProjection.Replace(originalPolymorphicExpression, polymorphicExpression);

                                return outerQuery
                                    .UpdateSelectExpression(outerSelectExpression
                                        .UpdateProjection(new ServerProjectionExpression(polymorphicExpression))
                                        .AddToPredicate(predicate));
                            }

                            return FallbackToEnumerable();
                        }

                        // Grouping operations

                        case nameof(Queryable.GroupBy):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var keySelectorLambda = node.Arguments[1].UnwrapLambda();

                            if (!keySelectorLambda.Body.References(keySelectorLambda.Parameters[0]))
                            {
                                // SQL Server Says:
                                // Msg 164, Level 15, State 1, Line 3
                                // Each GROUP BY expression must contain at least one column that is not an outer reference.
                                return FallbackToEnumerable();
                            }

                            if (!HandlePushdown(
                                s => s.IsWindowed || s.IsDistinct || s.Limit != null || s.Offset != null || s.Grouping != null,
                                keySelectorLambda.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            outerSelectExpression = outerSelectExpression.UpdateOrderBy(null);

                            // Key Selector

                            var keySelector
                                = keySelectorLambda
                                    .ExpandParameters(outerProjection)
                                    .VisitWith(PostExpansionVisitors);

                            if (!IsTranslatable(keySelector) || ContainsAggregateOrSubquery(keySelector))
                            {
                                return FallbackToEnumerable();
                            }

                            // Element Selector

                            var elementSelector = outerProjection;

                            if (node.Method.GetParameters().Any(p => p.Name == "elementSelector"))
                            {
                                elementSelector
                                    = node.Arguments[2]
                                        .UnwrapLambda()
                                        .ExpandParameters(elementSelector)
                                        .VisitWith(PostExpansionVisitors);
                            }

                            if (!IsTranslatable(elementSelector))
                            {
                                return FallbackToEnumerable();
                            }

                            // Result Selector

                            if (node.Method.GetParameters().Any(p => p.Name == "resultSelector"))
                            {
                                var resultLambda = node.Arguments[node.Arguments.Count - 1].UnwrapLambda();

                                var resultSelector
                                    = resultLambda
                                        .ExpandParameters(
                                            keySelector,
                                            new GroupByResultExpression(
                                                outerSelectExpression,
                                                keySelector,
                                                keySelector,
                                                keySelectorLambda,
                                                elementSelector,
                                                false))
                                        .VisitWith(PostExpansionVisitors);

                                if (IsTranslatable(resultSelector))
                                {
                                    return outerQuery
                                        .UpdateSelectExpression(outerSelectExpression
                                            .UpdateProjection(new ServerProjectionExpression(resultSelector))
                                            .UpdateGrouping(keySelector));
                                }

                                return outerQuery
                                    .UpdateSelectExpression(outerSelectExpression
                                        .UpdateProjection(
                                            new ClientProjectionExpression(
                                                new ServerProjectionExpression(keySelector),
                                                Expression.Lambda(
                                                    resultLambda.Body.Replace(
                                                        resultLambda.Parameters[1],
                                                        new GroupByResultExpression(
                                                            outerSelectExpression,
                                                            resultLambda.Parameters[0],
                                                            keySelector,
                                                            keySelectorLambda,
                                                            elementSelector,
                                                            false)),
                                                    resultLambda.Parameters[0])))
                                        .UpdateGrouping(keySelector))
                                    .VisitWith(ClientPostExpansionVisitors);
                            }
                            else
                            {
                                var resultSelector
                                    = (Expression)new GroupByResultExpression(
                                        outerSelectExpression,
                                        keySelector,
                                        keySelector,
                                        keySelectorLambda,
                                        elementSelector,
                                        false);

                                if (IsTranslatable(resultSelector))
                                {
                                    return outerQuery
                                        .UpdateSelectExpression(outerSelectExpression
                                            .UpdateProjection(new ServerProjectionExpression(resultSelector))
                                            .UpdateGrouping(keySelector));
                                }

                                var keyPlaceholderGrouping = KeyPlaceholderGrouping.Create(outerSelectExpression, keySelector);

                                var groupingParameter = Expression.Parameter(keyPlaceholderGrouping.Type, "g");

                                return outerQuery
                                    .UpdateSelectExpression(outerSelectExpression
                                        .UpdateProjection(
                                            new ClientProjectionExpression(
                                                new ServerProjectionExpression(keyPlaceholderGrouping),
                                                Expression.Lambda(
                                                    new GroupByResultExpression(
                                                        outerSelectExpression,
                                                        Expression.MakeMemberAccess(
                                                            groupingParameter,
                                                            keyPlaceholderGrouping.Type.GetRuntimeProperty("Key")),
                                                        keySelector,
                                                        keySelectorLambda,
                                                        elementSelector,
                                                        false),
                                                    groupingParameter)))
                                        .UpdateGrouping(keySelector))
                                    .VisitWith(ClientPostExpansionVisitors);
                            }
                        }

                        // Generation operations

                        case nameof(Enumerable.Empty):
                        case nameof(Enumerable.Range):
                        case nameof(Enumerable.Repeat):
                        {
                            // TODO: Handle Empty
                            // TODO: Handle Range
                            // TODO: Handle Repeat
                            break;
                        }

                        case nameof(Queryable.DefaultIfEmpty):
                        {
                            if (node.Arguments.Count > 1)
                            {
                                // TODO: Investigate default value argument support
                                // - Scalar type should be simple enough
                                // - Complex type would need some extra checks
                                //   - The shape of the expression would have to match:
                                //     - NewExpression, same ctors
                                //     - MemberInitExpression, same bindings (unordered)
                                //   - 'Zip' the NewExpression/MemberInitExpressions together
                                //     - At each leaf node, coalesce from the original to the default
                                return FallbackToEnumerable();
                            }

                            var outerProjection = outerQuery.SelectExpression.Projection.Flatten().Body;

                            if (!IsTranslatable(outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var emptySubquery
                                = new SubqueryTableExpression(
                                    alias: "t",
                                    subquery: new SelectExpression(
                                        new ServerProjectionExpression(
                                            EmptyRecord.NewExpression)));

                            var innerSubquery
                                = new SubqueryTableExpression(
                                    alias: "t",
                                    subquery: outerQuery.SelectExpression.UpdateProjection(
                                        new ServerProjectionExpression(
                                            new DefaultIfEmptyExpression(
                                                outerProjection))));

                            var joinExpression
                                = new LeftJoinExpression(
                                    emptySubquery,
                                    innerSubquery,
                                    Expression.Equal(Expression.Constant(1), Expression.Constant(1)),
                                    innerSubquery.Type);

                            var projectionBody
                                = new ProjectionReferenceRewritingExpressionVisitor(innerSubquery)
                                    .Visit(outerProjection);

                            var selectExpression
                                = new SelectExpression(
                                    new ServerProjectionExpression(
                                        new DefaultIfEmptyExpression(
                                            projectionBody,
                                            new SqlColumnExpression(innerSubquery, "$empty", typeof(int?), true))),
                                    joinExpression);

                            return new EnumerableRelationalQueryExpression(selectExpression);
                        }

                        // Element operations

                        case nameof(Queryable.First):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var predicateLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

                            if (predicateLambda != null)
                            {
                                if (!HandlePushdown(
                                    s => s.RequiresPushdownForPredicate(),
                                    predicateLambda.Parameters[0].Name,
                                    ref outerSelectExpression,
                                    ref outerProjection))
                                {
                                    return FallbackToEnumerable();
                                }

                                var predicateBody
                                    = predicateLambda
                                        .ExpandParameters(outerProjection)
                                        .VisitWith(PostExpansionVisitors);

                                if (!IsTranslatable(predicateBody))
                                {
                                    // TODO: fallback to Where(predicate).First()
                                    return FallbackToEnumerable();
                                }

                                outerSelectExpression = outerSelectExpression.AddToPredicate(predicateBody);
                            }

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForLimit(),
                                predicateLambda?.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            return Expression.Call(
                                GetGenericMethodDefinition((IEnumerable<object> e) => e.First())
                                    .MakeGenericMethod(outerSelectExpression.Type),
                                outerQuery
                                    .UpdateSelectExpression(outerSelectExpression
                                        .UpdateLimit(Expression.Constant(1))));
                        }

                        case nameof(Queryable.FirstOrDefault):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var predicateLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

                            if (predicateLambda != null)
                            {
                                if (!HandlePushdown(
                                    s => s.RequiresPushdownForPredicate(),
                                    predicateLambda.Parameters[0].Name,
                                    ref outerSelectExpression,
                                    ref outerProjection))
                                {
                                    return FallbackToEnumerable();
                                }

                                var predicateBody
                                    = predicateLambda
                                        .ExpandParameters(outerProjection)
                                        .VisitWith(PostExpansionVisitors);

                                if (!IsTranslatable(predicateBody))
                                {
                                    // TODO: fallback to Where(predicate).FirstOrDefault()
                                    return FallbackToEnumerable();
                                }

                                outerSelectExpression = outerSelectExpression.AddToPredicate(predicateBody);
                            }

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForLimit(),
                                predicateLambda?.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            outerSelectExpression
                                = outerSelectExpression
                                    .UpdateLimit(Expression.Constant(1));

                            if (outerProjection.Type.IsScalarType())
                            {
                                return new SingleValueRelationalQueryExpression(
                                    outerSelectExpression);
                            }

                            return Expression.Call(
                                GetGenericMethodDefinition((IEnumerable<object> e) => e.FirstOrDefault())
                                    .MakeGenericMethod(outerSelectExpression.Type),
                                outerQuery
                                    .UpdateSelectExpression(outerSelectExpression));
                        }

                        case nameof(Queryable.Last):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var predicateLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

                            if (predicateLambda != null)
                            {
                                if (!HandlePushdown(
                                    s => s.RequiresPushdownForPredicate(),
                                    predicateLambda.Parameters[0].Name,
                                    ref outerSelectExpression,
                                    ref outerProjection))
                                {
                                    return FallbackToEnumerable();
                                }

                                var predicateBody
                                    = predicateLambda
                                        .ExpandParameters(outerProjection)
                                        .VisitWith(PostExpansionVisitors);

                                if (!IsTranslatable(predicateBody))
                                {
                                    // TODO: fallback to Where(predicate).Last()
                                    return FallbackToEnumerable();
                                }

                                outerSelectExpression = outerSelectExpression.AddToPredicate(predicateBody);
                            }

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForLimit(),
                                predicateLambda?.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var orderByExpression = outerSelectExpression.OrderBy;

                            if (orderByExpression == null)
                            {
                                // Don't use CreateRowNumberExpression because we don't need
                                // the Convert or the Subtract.

                                orderByExpression
                                    = new OrderByExpression(
                                        new SqlWindowFunctionExpression(
                                            new SqlFunctionExpression("ROW_NUMBER", typeof(long)),
                                            ComputeDefaultWindowOrderBy(outerSelectExpression)),
                                        false);
                            }

                            return Expression.Call(
                                GetGenericMethodDefinition((IEnumerable<object> e) => e.Last())
                                    .MakeGenericMethod(outerSelectExpression.Type),
                                outerQuery
                                    .UpdateSelectExpression(outerSelectExpression
                                        .UpdateOrderBy(orderByExpression.Reverse())
                                        .UpdateLimit(Expression.Constant(1))));
                        }

                        case nameof(Queryable.LastOrDefault):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var predicateLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

                            if (predicateLambda != null)
                            {
                                if (!HandlePushdown(
                                    s => s.RequiresPushdownForPredicate(),
                                    predicateLambda.Parameters[0].Name,
                                    ref outerSelectExpression,
                                    ref outerProjection))
                                {
                                    return FallbackToEnumerable();
                                }

                                var predicateBody
                                    = predicateLambda
                                        .ExpandParameters(outerProjection)
                                        .VisitWith(PostExpansionVisitors);

                                if (!IsTranslatable(predicateBody))
                                {
                                    // TODO: fallback to Where(predicate).LastOrDefault()
                                    return FallbackToEnumerable();
                                }

                                outerSelectExpression = outerSelectExpression.AddToPredicate(predicateBody);
                            }

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForLimit(),
                                predicateLambda?.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var orderByExpression = outerSelectExpression.OrderBy;

                            if (orderByExpression == null)
                            {
                                // Don't use CreateRowNumberExpression because we don't need
                                // the Convert or the Subtract.

                                orderByExpression
                                    = new OrderByExpression(
                                        new SqlWindowFunctionExpression(
                                            new SqlFunctionExpression("ROW_NUMBER", typeof(long)),
                                            ComputeDefaultWindowOrderBy(outerSelectExpression)),
                                        false);
                            }

                            // TODO: Return SingleValueRelationalQuery instead

                            return Expression.Call(
                                GetGenericMethodDefinition((IEnumerable<object> e) => e.LastOrDefault())
                                    .MakeGenericMethod(outerSelectExpression.Type),
                                outerQuery
                                    .UpdateSelectExpression(outerSelectExpression
                                        .UpdateOrderBy(orderByExpression.Reverse())
                                        .UpdateLimit(Expression.Constant(1))));
                        }

                        case nameof(Queryable.Single):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var predicateLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

                            if (predicateLambda != null)
                            {
                                if (!HandlePushdown(
                                    s => s.RequiresPushdownForPredicate(),
                                    predicateLambda.Parameters[0].Name,
                                    ref outerSelectExpression,
                                    ref outerProjection))
                                {
                                    return FallbackToEnumerable();
                                }

                                var predicateBody
                                    = predicateLambda
                                        .ExpandParameters(outerProjection)
                                        .VisitWith(PostExpansionVisitors);

                                if (!IsTranslatable(predicateBody))
                                {
                                    // TODO: fallback to Where(predicate).Single()
                                    return FallbackToEnumerable();
                                }

                                outerSelectExpression = outerSelectExpression.AddToPredicate(predicateBody);
                            }

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForLimit(),
                                predicateLambda?.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            return Expression.Call(
                                GetGenericMethodDefinition((IEnumerable<object> e) => e.Single())
                                    .MakeGenericMethod(outerSelectExpression.Type),
                                outerQuery
                                    .UpdateSelectExpression(outerSelectExpression
                                        .UpdateLimit(Expression.Constant(2))));
                        }

                        case nameof(Queryable.SingleOrDefault):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var predicateLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

                            if (predicateLambda != null)
                            {
                                if (!HandlePushdown(
                                    s => s.RequiresPushdownForPredicate(),
                                    predicateLambda.Parameters[0].Name,
                                    ref outerSelectExpression,
                                    ref outerProjection))
                                {
                                    return FallbackToEnumerable();
                                }

                                var predicateBody
                                    = predicateLambda
                                        .ExpandParameters(outerProjection)
                                        .VisitWith(PostExpansionVisitors);

                                if (!IsTranslatable(predicateBody))
                                {
                                    // TODO: fallback to Where(predicate).SingleOrDefault()
                                    return FallbackToEnumerable();
                                }

                                outerSelectExpression = outerSelectExpression.AddToPredicate(predicateBody);
                            }

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForLimit(),
                                predicateLambda?.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            return Expression.Call(
                                GetGenericMethodDefinition((IEnumerable<object> e) => e.SingleOrDefault())
                                    .MakeGenericMethod(outerSelectExpression.Type),
                                outerQuery
                                    .UpdateSelectExpression(outerSelectExpression
                                        .UpdateLimit(Expression.Constant(2))));
                        }

                        case nameof(Queryable.ElementAt):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForLimit(),
                                null,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var index = Visit(node.Arguments[1]);

                            if (!IsTranslatable(index))
                            {
                                return FallbackToEnumerable();
                            }

                            var orderByExpression = outerSelectExpression.OrderBy;

                            if (orderByExpression == null)
                            {
                                // Don't use CreateRowNumberExpression because we don't need
                                // the Convert or the Subtract.

                                orderByExpression
                                    = new OrderByExpression(
                                        new SqlWindowFunctionExpression(
                                            new SqlFunctionExpression("ROW_NUMBER", typeof(long)),
                                            ComputeDefaultWindowOrderBy(outerSelectExpression)),
                                        false);
                            }

                            return Expression.Call(
                                GetGenericMethodDefinition((IEnumerable<object> e) => e.ElementAt(0))
                                    .MakeGenericMethod(outerSelectExpression.Type),
                                outerQuery
                                    .UpdateSelectExpression(outerSelectExpression
                                        .UpdateOrderBy(orderByExpression)
                                        .UpdateOffset(index)
                                        .UpdateLimit(Expression.Constant(1))),
                                Expression.Constant(0));
                        }

                        case nameof(Queryable.ElementAtOrDefault):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForLimit(),
                                null,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var index = Visit(node.Arguments[1]);

                            if (!IsTranslatable(index))
                            {
                                return FallbackToEnumerable();
                            }

                            var orderByExpression = outerSelectExpression.OrderBy;

                            if (orderByExpression == null)
                            {
                                // Don't use CreateRowNumberExpression because we don't need
                                // the Convert or the Subtract.

                                orderByExpression
                                    = new OrderByExpression(
                                        new SqlWindowFunctionExpression(
                                            new SqlFunctionExpression("ROW_NUMBER", typeof(long)),
                                            ComputeDefaultWindowOrderBy(outerSelectExpression)),
                                        false);
                            }

                            // TODO: Return SingleValueRelationalQuery instead

                            return Expression.Call(
                                GetGenericMethodDefinition((IEnumerable<object> e) => e.ElementAtOrDefault(0))
                                    .MakeGenericMethod(outerSelectExpression.Type),
                                outerQuery
                                    .UpdateSelectExpression(outerSelectExpression
                                        .UpdateOrderBy(orderByExpression)
                                        .UpdateOffset(index)
                                        .UpdateLimit(Expression.Constant(1))),
                                Expression.Constant(0));
                        }

                        // Sorting operations

                        case nameof(Queryable.OrderBy):
                        case nameof(Queryable.OrderByDescending):
                        case nameof(Queryable.ThenBy):
                        case nameof(Queryable.ThenByDescending):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var selectorLambda = node.Arguments[1].UnwrapLambda();

                            if (!HandlePushdown(
                                s => s.IsDistinct || s.Limit != null || s.Offset != null,
                                selectorLambda.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                goto ReturnOrderedEnumerable;
                            }

                            var selectorBody
                                = selectorLambda
                                    .ExpandParameters(outerProjection)
                                    .VisitWith(PostExpansionVisitors);

                            if (IsTranslatable(selectorBody) && selectorBody.Type.IsScalarType())
                            {
                                switch (node.Method.Name)
                                {
                                    case nameof(Queryable.OrderBy):
                                    {
                                        return outerQuery
                                            .UpdateSelectExpression(outerSelectExpression
                                                .UpdateOrderBy(null)
                                                .AddToOrderBy(selectorBody, false))
                                            .AsOrdered();
                                    }

                                    case nameof(Queryable.OrderByDescending):
                                    {
                                        return outerQuery
                                            .UpdateSelectExpression(outerSelectExpression
                                                .UpdateOrderBy(null)
                                                .AddToOrderBy(selectorBody, true))
                                            .AsOrdered();
                                    }

                                    case nameof(Queryable.ThenBy):
                                    {
                                        return outerQuery
                                            .UpdateSelectExpression(outerSelectExpression
                                                .AddToOrderBy(selectorBody, false))
                                            .AsOrdered();
                                    }

                                    case nameof(Queryable.ThenByDescending):
                                    {
                                        return outerQuery
                                            .UpdateSelectExpression(outerSelectExpression
                                                .AddToOrderBy(selectorBody, true))
                                            .AsOrdered();
                                    }
                                }
                            }

                            ReturnOrderedEnumerable:

                            if (node.Method.Name == nameof(Queryable.ThenBy)
                                || node.Method.Name == nameof(Queryable.ThenByDescending))
                            {
                                var resultNode
                                    = outerQuery
                                        .UpdateSelectExpression(outerSelectExpression
                                            .UpdateOrderBy(null)) as Expression;

                                var currentNode = node.Arguments[0];

                                while (currentNode is MethodCallExpression methodCall
                                    && methodCall.Method.IsOrderingMethod())
                                {
                                    resultNode = methodCall.Update(null, methodCall.Arguments.Skip(1).Prepend(resultNode));

                                    resultNode
                                        = Expression.Call(
                                            MatchQueryableMethod(methodCall.Method),
                                            methodCall.Arguments
                                                .Skip(1)
                                                .Prepend(resultNode)
                                                .Select(a => a.NodeType == ExpressionType.Quote ? a.UnwrapLambda() : a));

                                    currentNode = methodCall.Arguments[0];
                                }

                                visitedArguments[0] = resultNode;
                            }

                            return FallbackToEnumerable();
                        }

                        case nameof(Queryable.Reverse):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;

                            if (!HandlePushdown(
                                s => s.IsDistinct || s.Limit != null || s.Offset != null,
                                null,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var orderByExpression = outerSelectExpression.OrderBy;

                            if (orderByExpression == null)
                            {
                                // Don't use CreateRowNumberExpression because we don't need
                                // the Convert or the Subtract.

                                orderByExpression
                                    = new OrderByExpression(
                                        new SqlWindowFunctionExpression(
                                            new SqlFunctionExpression("ROW_NUMBER", typeof(long)),
                                            ComputeDefaultWindowOrderBy(outerSelectExpression)),
                                        false);
                            }

                            return outerQuery
                                .UpdateSelectExpression(outerSelectExpression
                                    .UpdateOrderBy(orderByExpression.Reverse()));
                        }

                        // Partitioning operations

                        case nameof(Queryable.Take):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForLimit(),
                                null,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var count = Visit(node.Arguments[1]);

                            if (!IsTranslatable(count))
                            {
                                return FallbackToEnumerable();
                            }

                            return outerQuery
                                .UpdateSelectExpression(outerSelectExpression
                                    .UpdateLimit(count));
                        }

                        case nameof(Queryable.Skip):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForOffset(),
                                null,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            if (outerSelectExpression.OrderBy == null)
                            {
                                outerSelectExpression
                                    = outerSelectExpression
                                        .UpdateOrderBy(
                                            new OrderByExpression(
                                                new SqlWindowFunctionExpression(
                                                    new SqlFunctionExpression("ROW_NUMBER", typeof(long)),
                                                    ComputeDefaultWindowOrderBy(outerSelectExpression)),
                                                false));
                            }

                            var count = Visit(node.Arguments[1]);

                            if (!IsTranslatable(count))
                            {
                                return FallbackToEnumerable();
                            }

                            return outerQuery
                                .UpdateSelectExpression(outerSelectExpression
                                    .UpdateOffset(count));
                        }

                        case nameof(Queryable.TakeWhile):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var predicateLambda = node.Arguments[1].UnwrapLambda();

                            outerProjection
                                = RowNumberTuple.Create(
                                    outerProjection,
                                    new SqlWindowFunctionExpression(
                                        new SqlFunctionExpression("ROW_NUMBER", typeof(int)),
                                        ComputeDefaultWindowOrderBy(outerSelectExpression)));

                            outerSelectExpression
                                = outerSelectExpression
                                    .UpdateProjection(new ServerProjectionExpression(outerProjection))
                                    .AsWindowed();

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForPredicate(),
                                predicateLambda.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var outerRowNumberExpression = (outerProjection as NewExpression).Arguments[1];

                            outerProjection = (outerProjection as NewExpression).Arguments[0];

                            outerSelectExpression
                                = outerSelectExpression
                                    .UpdateProjection(new ServerProjectionExpression(outerProjection));

                            var subquerySelectExpression
                                = new TableUniquifyingExpressionVisitor()
                                    .VisitAndConvert(
                                        outerQuery.SelectExpression,
                                        nameof(VisitMethodCall));

                            var subqueryProjection
                                = RowNumberTuple.Create(
                                    subquerySelectExpression.Projection.Flatten().Body,
                                    new SqlWindowFunctionExpression(
                                        new SqlFunctionExpression("ROW_NUMBER", typeof(int)),
                                        ComputeDefaultWindowOrderBy(subquerySelectExpression)));

                            subquerySelectExpression
                                = subquerySelectExpression
                                    .UpdateProjection(new ServerProjectionExpression(subqueryProjection))
                                    .AsWindowed();

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForPredicate(),
                                predicateLambda.Parameters[0].Name,
                                ref subquerySelectExpression,
                                ref subqueryProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var subqueryPredicateBody
                                = predicateLambda
                                    .ExpandParameters(
                                        predicateLambda.Parameters
                                            .Zip(
                                                new[]
                                                {
                                                    (subqueryProjection as NewExpression).Arguments[0],
                                                    Expression.Subtract(
                                                        (subqueryProjection as NewExpression).Arguments[1],
                                                        Expression.Constant(1)),
                                                },
                                                (a, b) => b)
                                            .ToArray())
                                    .VisitWith(PostExpansionVisitors);

                            if (!IsTranslatable(subqueryPredicateBody))
                            {
                                return FallbackToEnumerable();
                            }

                            subquerySelectExpression
                                = subquerySelectExpression
                                    .AddToPredicate(
                                        BinaryInvertingExpressionVisitor.Instance.Visit(subqueryPredicateBody))
                                    .UpdateProjection(new ServerProjectionExpression(
                                        Expression.Coalesce(
                                            new SqlAggregateExpression(
                                                "MIN",
                                                (subqueryProjection as NewExpression).Arguments[1],
                                                typeof(int?)),
                                            Expression.Add(
                                                outerRowNumberExpression,
                                                Expression.Constant(1)))));

                            Func<Expression, Expression, Expression> factory = Expression.LessThan;

                            return outerQuery
                                .UpdateSelectExpression(outerSelectExpression
                                    .AddToPredicate(
                                        Expression.LessThan(
                                            outerRowNumberExpression,
                                            new SingleValueRelationalQueryExpression(
                                                subquerySelectExpression))));
                        }

                        case nameof(Queryable.SkipWhile):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var predicateLambda = node.Arguments[1].UnwrapLambda();

                            outerProjection
                                = RowNumberTuple.Create(
                                    outerProjection,
                                    new SqlWindowFunctionExpression(
                                        new SqlFunctionExpression("ROW_NUMBER", typeof(int)),
                                        ComputeDefaultWindowOrderBy(outerSelectExpression)));

                            outerSelectExpression
                                = outerSelectExpression
                                    .UpdateProjection(new ServerProjectionExpression(outerProjection))
                                    .AsWindowed();

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForPredicate(),
                                predicateLambda.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var outerRowNumberExpression = (outerProjection as NewExpression).Arguments[1];

                            outerProjection = (outerProjection as NewExpression).Arguments[0];

                            outerSelectExpression
                                = outerSelectExpression
                                    .UpdateProjection(new ServerProjectionExpression(outerProjection));

                            var subquerySelectExpression
                                = new TableUniquifyingExpressionVisitor()
                                    .VisitAndConvert(
                                        outerQuery.SelectExpression,
                                        nameof(VisitMethodCall));

                            var subqueryProjection
                                = RowNumberTuple.Create(
                                    subquerySelectExpression.Projection.Flatten().Body,
                                    new SqlWindowFunctionExpression(
                                        new SqlFunctionExpression("ROW_NUMBER", typeof(int)),
                                        ComputeDefaultWindowOrderBy(subquerySelectExpression)));

                            subquerySelectExpression
                                = subquerySelectExpression
                                    .UpdateProjection(new ServerProjectionExpression(subqueryProjection))
                                    .AsWindowed();

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForPredicate(),
                                predicateLambda.Parameters[0].Name,
                                ref subquerySelectExpression,
                                ref subqueryProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var subqueryPredicateBody
                                = predicateLambda
                                    .ExpandParameters(
                                        predicateLambda.Parameters
                                            .Zip(
                                                new[]
                                                {
                                                    (subqueryProjection as NewExpression).Arguments[0],
                                                    Expression.Subtract(
                                                        (subqueryProjection as NewExpression).Arguments[1],
                                                        Expression.Constant(1)),
                                                },
                                                (a, b) => b)
                                            .ToArray())
                                    .VisitWith(PostExpansionVisitors);

                            if (!IsTranslatable(subqueryPredicateBody))
                            {
                                return FallbackToEnumerable();
                            }

                            subquerySelectExpression
                                = subquerySelectExpression
                                    .AddToPredicate(
                                        BinaryInvertingExpressionVisitor.Instance.Visit(subqueryPredicateBody))
                                    .UpdateProjection(new ServerProjectionExpression(
                                        Expression.Coalesce(
                                            new SqlAggregateExpression(
                                                "MIN",
                                                (subqueryProjection as NewExpression).Arguments[1],
                                                typeof(int?)),
                                            Expression.Constant(0))));

                            return outerQuery
                                .UpdateSelectExpression(outerSelectExpression
                                    .AddToPredicate(
                                        Expression.GreaterThanOrEqual(
                                            outerRowNumberExpression,
                                            new SingleValueRelationalQueryExpression(
                                                subquerySelectExpression))));
                        }

                        // Conversion operations

                        case nameof(Queryable.Cast):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;

                            var inType = outerProjection.Type;
                            var outType = node.Method.GetGenericArguments()[0];

                            if (inType == outType)
                            {
                                return outerQuery;
                            }

                            if (outType.IsAssignableFrom(inType)
                                && outerProjection is PolymorphicExpression polymorphicExpression)
                            {
                                return outerQuery
                                    .UpdateSelectExpression(outerSelectExpression
                                        .UpdateProjection(new ServerProjectionExpression(
                                            polymorphicExpression.Upcast(outType))));
                            }

                            return FallbackToEnumerable();
                        }

                        // Set operations

                        case nameof(Queryable.Distinct):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;

                            if (!HandlePushdown(
                                s => s.IsDistinct || s.Limit != null || s.Offset != null,
                                null,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            if (!IsTranslatable(outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            // Per docs: 
                            // The expected behavior is that it returns an unordered sequence 
                            // of the unique items in source.
                            // https://msdn.microsoft.com/en-us/library/bb348456(v=vs.110).aspx

                            return outerQuery
                                .UpdateSelectExpression(outerSelectExpression
                                    .UpdateProjection(
                                        new ServerProjectionExpression(outerProjection))
                                    .UpdateOrderBy(null)
                                    .AsDistinct());
                        }

                        case nameof(Queryable.Concat):
                        case nameof(Queryable.Except):
                        case nameof(Queryable.Intersect):
                        case nameof(Queryable.Union):
                        {
                            var innerSource = visitedArguments[1] = ProcessQuerySource(Visit(node.Arguments[1]));

                            if (!(innerSource is EnumerableRelationalQueryExpression innerQuery))
                            {
                                return FallbackToEnumerable();
                            }

                            var outerSelectExpression = outerQuery.SelectExpression.UpdateOrderBy(null);
                            var innerSelectExpression = innerQuery.SelectExpression.UpdateOrderBy(null);

                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var innerProjection = innerSelectExpression.Projection.Flatten().Body;

                            // TODO: Check to make sure the projections have the same 'shape'

                            if (!IsTranslatable(outerProjection) || !IsTranslatable(innerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var typesMatch = outerProjection.Type == innerProjection.Type;

                            if (!typesMatch)
                            {
                                if (outerProjection is PolymorphicExpression outerPolymorphicExpression)
                                {
                                    outerProjection = outerPolymorphicExpression.Upcast(node.Type.GetSequenceType());
                                }
                                else
                                {
                                    //outerProjection = Expression.Convert(outerProjection, node.Type.GetSequenceType());
                                }

                                outerSelectExpression
                                    = outerSelectExpression.UpdateProjection(
                                        new ServerProjectionExpression(
                                            outerProjection));

                                if (innerProjection is PolymorphicExpression innerPolymorphicExpression)
                                {
                                    innerProjection = innerPolymorphicExpression.Upcast(node.Type.GetSequenceType());
                                }
                                else
                                {
                                    //innerProjection = Expression.Convert(innerProjection, node.Type.GetSequenceType());
                                }

                                innerSelectExpression
                                    = innerSelectExpression.UpdateProjection(
                                        new ServerProjectionExpression(
                                            innerProjection));
                            }

                            /*if (!typesMatch)
                            {
                                return FallbackToEnumerable();
                            }*/

                            var setOperatorExpression = default(SetOperatorExpression);

                            switch (node.Method.Name)
                            {
                                case nameof(Queryable.Concat):
                                {
                                    setOperatorExpression = new UnionAllExpression(outerSelectExpression, innerSelectExpression);
                                    break;
                                }

                                case nameof(Queryable.Except):
                                {
                                    setOperatorExpression = new ExceptExpression(outerSelectExpression, innerSelectExpression);
                                    break;
                                }

                                case nameof(Queryable.Intersect):
                                {
                                    setOperatorExpression = new IntersectExpression(outerSelectExpression, innerSelectExpression);
                                    break;
                                }

                                case nameof(Queryable.Union):
                                {
                                    setOperatorExpression = new UnionExpression(outerSelectExpression, innerSelectExpression);
                                    break;
                                }
                            }

                            return new EnumerableRelationalQueryExpression(
                                new SelectExpression(
                                    new ServerProjectionExpression(
                                        new ProjectionReferenceRewritingExpressionVisitor(setOperatorExpression)
                                            .Visit(outerProjection)
                                            .VisitWith(PostExpansionVisitors)),
                                    setOperatorExpression));
                        }

                        case nameof(Queryable.Zip):
                        {
                            var innerSource = visitedArguments[1] = ProcessQuerySource(Visit(node.Arguments[1]));

                            if (!(innerSource is EnumerableRelationalQueryExpression innerQuery))
                            {
                                return FallbackToEnumerable();
                            }

                            var resultSelectorLambda = node.Arguments[2].UnwrapLambda();

                            var outerSelectExpression = outerQuery.SelectExpression;

                            var outerProjection
                                = RowNumberTuple.Create(
                                    outerSelectExpression.Projection.Flatten().Body,
                                    new SqlWindowFunctionExpression(
                                        new SqlFunctionExpression("ROW_NUMBER", typeof(int)),
                                        ComputeDefaultWindowOrderBy(outerSelectExpression)));

                            outerSelectExpression
                                = outerSelectExpression
                                    .UpdateProjection(new ServerProjectionExpression(outerProjection))
                                    .AsWindowed();

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForLeftSideOfJoin(),
                                resultSelectorLambda.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var innerSelectExpression = innerQuery.SelectExpression;

                            var innerProjection
                                = RowNumberTuple.Create(
                                    innerSelectExpression.Projection.Flatten().Body,
                                    new SqlWindowFunctionExpression(
                                        new SqlFunctionExpression("ROW_NUMBER", typeof(int)),
                                        ComputeDefaultWindowOrderBy(innerSelectExpression)));

                            innerSelectExpression
                                = innerSelectExpression
                                    .UpdateProjection(new ServerProjectionExpression(innerProjection))
                                    .AsWindowed();

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForRightSideOfJoin(),
                                resultSelectorLambda.Parameters[1].Name,
                                ref innerSelectExpression,
                                ref innerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var predicate
                                = Expression.Equal(
                                    (outerProjection as NewExpression).Arguments[1],
                                    (innerProjection as NewExpression).Arguments[1]);

                            var resultSelectorBody
                                = resultSelectorLambda
                                    .ExpandParameters(
                                        (outerProjection as NewExpression).Arguments[0],
                                        (innerProjection as NewExpression).Arguments[0])
                                    .VisitWith(PostExpansionVisitors);

                            var projection
                                = IsTranslatable(resultSelectorBody)
                                    ? new ServerProjectionExpression(resultSelectorBody)
                                    : new CompositeProjectionExpression(
                                        outerSelectExpression.Projection,
                                        innerSelectExpression.Projection,
                                        resultSelectorLambda) as ProjectionExpression;

                            var table
                                = new InnerJoinExpression(
                                    outerSelectExpression.Table,
                                    innerSelectExpression.Table as AliasedTableExpression,
                                    predicate,
                                    resultSelectorBody.Type);

                            return outerQuery
                                .UpdateSelectExpression(outerSelectExpression
                                    .UpdateProjection(projection)
                                    .UpdateTable(table));
                        }

                        // Equality operations

                        case nameof(Queryable.SequenceEqual):
                        {
                            var innerSource = visitedArguments[1] = ProcessQuerySource(Visit(node.Arguments[1]));

                            if (!(innerSource is EnumerableRelationalQueryExpression innerQuery))
                            {
                                return FallbackToEnumerable();
                            }

                            var outerSelectExpression = outerQuery.SelectExpression;

                            var outerProjection
                                = RowNumberTuple.Create(
                                    outerSelectExpression.Projection.Flatten().Body,
                                    new SqlWindowFunctionExpression(
                                        new SqlFunctionExpression("ROW_NUMBER", typeof(int)),
                                        ComputeDefaultWindowOrderBy(outerSelectExpression)));

                            outerSelectExpression
                                = outerSelectExpression
                                    .UpdateProjection(new ServerProjectionExpression(outerProjection))
                                    .AsWindowed();

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForLeftSideOfJoin(),
                                null,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var innerSelectExpression = innerQuery.SelectExpression;

                            var innerProjection
                                = RowNumberTuple.Create(
                                    innerSelectExpression.Projection.Flatten().Body,
                                    new SqlWindowFunctionExpression(
                                        new SqlFunctionExpression("ROW_NUMBER", typeof(int)),
                                        ComputeDefaultWindowOrderBy(innerSelectExpression)));

                            innerSelectExpression
                                = innerSelectExpression
                                    .UpdateProjection(new ServerProjectionExpression(innerProjection))
                                    .AsWindowed();

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForRightSideOfJoin(),
                                null,
                                ref innerSelectExpression,
                                ref innerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var outerRowNumber = (outerProjection as NewExpression).Arguments[1];
                            var innerRowNumber = (innerProjection as NewExpression).Arguments[1];

                            var resultSelectExpression
                                = new SelectExpression(
                                    new ServerProjectionExpression(
                                        Expression.Constant(1)),
                                    new FullJoinExpression(
                                        outerSelectExpression.Table,
                                        innerSelectExpression.Table as AliasedTableExpression,
                                        Expression.Equal(outerRowNumber, innerRowNumber),
                                        typeof(object)));

                            var conditions = new[]
                            {
                                Expression.Equal(
                                    Expression.Convert(outerRowNumber, typeof(int?)),
                                    Expression.Constant(null, typeof(int?))),
                                Expression.Equal(
                                    Expression.Convert(innerRowNumber, typeof(int?)),
                                    Expression.Constant(null, typeof(int?))),
                                Expression.NotEqual(
                                    (outerProjection as NewExpression).Arguments[0],
                                    (innerProjection as NewExpression).Arguments[0]),
                            };

                            return new SingleValueRelationalQueryExpression(
                                new SelectExpression(
                                    new ServerProjectionExpression(
                                        Expression.Not(
                                            new SqlExistsExpression(
                                                resultSelectExpression
                                                    .AddToPredicate(conditions.Aggregate(Expression.OrElse)))))));
                        }

                        // Quantifier operations

                        case nameof(Queryable.All):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var predicateLambda = node.Arguments[1].UnwrapLambda();

                            if (!HandlePushdown(
                                s => s.RequiresPushdownForPredicate(),
                                predicateLambda.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var predicateBody
                                = predicateLambda
                                    .ExpandParameters(outerProjection)
                                    .VisitWith(PostExpansionVisitors);

                            if (!IsTranslatable(predicateBody))
                            {
                                return FallbackToEnumerable();
                            }

                            return new SingleValueRelationalQueryExpression(
                                new SelectExpression(
                                    new ServerProjectionExpression(
                                        Expression.Not(
                                            new SqlExistsExpression(
                                                outerSelectExpression
                                                    .UpdateProjection(new ServerProjectionExpression(Expression.Constant(1)))
                                                    .AddToPredicate(BinaryInvertingExpressionVisitor.Instance.Visit(predicateBody)))))));
                        }

                        case nameof(Queryable.Any):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var predicateLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

                            if (predicateLambda != null)
                            {
                                if (!HandlePushdown(
                                    s => s.RequiresPushdownForPredicate(),
                                    predicateLambda.Parameters[0].Name,
                                    ref outerSelectExpression,
                                    ref outerProjection))
                                {
                                    return FallbackToEnumerable();
                                }

                                var predicateBody
                                    = predicateLambda
                                        .ExpandParameters(outerProjection)
                                        .VisitWith(PostExpansionVisitors);

                                if (!IsTranslatable(predicateBody))
                                {
                                    return FallbackToEnumerable();
                                }

                                outerSelectExpression = outerSelectExpression.AddToPredicate(predicateBody);
                            }

                            if (!IsTranslatable(outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            outerSelectExpression
                                = outerSelectExpression
                                    .UpdateProjection(new ServerProjectionExpression(Expression.Constant(1)));

                            if (outerSelectExpression.Offset == null
                                && outerSelectExpression.Limit == null)
                            {
                                outerSelectExpression
                                    = outerSelectExpression.UpdateOrderBy(null);
                            }

                            return new SingleValueRelationalQueryExpression(
                                new SelectExpression(
                                    new ServerProjectionExpression(
                                        new SqlExistsExpression(
                                            outerSelectExpression))));
                        }

                        case nameof(Queryable.Contains):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;

                            if (!IsTranslatable(outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var valuesExpression = Visit(node.Arguments[1]);

                            switch (valuesExpression)
                            {
                                case SingleValueRelationalQueryExpression singleValueRelationalQueryExpression:
                                {
                                    throw new NotImplementedException();
                                }

                                case ConstantExpression constantExpression:
                                {
                                    // TODO: We should be able to get rid of the Cast/Condition wrappers here
                                    return new SingleValueRelationalQueryExpression(
                                        new SelectExpression(
                                            new ServerProjectionExpression(
                                                new SqlCastExpression(
                                                    Expression.Condition(
                                                        new SqlInExpression(constantExpression, outerSelectExpression),
                                                        Expression.Constant(true),
                                                        Expression.Constant(false)),
                                                    typeof(bool)))));
                                }
                            }

                            return FallbackToEnumerable();
                        }

                        // Aggregation operations

                        case nameof(Queryable.Average):
                        case nameof(Queryable.Max):
                        case nameof(Queryable.Min):
                        case nameof(Queryable.Sum):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var selectorLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

                            if (selectorLambda != null)
                            {
                                if (!HandlePushdown(
                                    s => s.IsDistinct,
                                    selectorLambda.Parameters[0].Name,
                                    ref outerSelectExpression,
                                    ref outerProjection))
                                {
                                    return FallbackToEnumerable();
                                }

                                outerProjection
                                    = selectorLambda
                                        .ExpandParameters(outerProjection)
                                        .VisitWith(PostExpansionVisitors);

                                outerSelectExpression
                                    = outerSelectExpression
                                        .UpdateProjection(new ServerProjectionExpression(outerProjection));
                            }

                            if (!HandlePushdown(
                                s => s.IsWindowed || s.IsDistinct || s.Limit != null || s.Offset != null || s.Grouping != null,
                                selectorLambda?.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            if (!IsTranslatable(outerProjection))
                            {
                                return FallbackToEnumerable();
                            }
                            
                            if (ContainsAggregateOrSubquery(outerProjection))
                            {
                                var enumerableMethod
                                    = (from m in typeof(Enumerable).GetRuntimeMethods()
                                       where m.Name == node.Method.Name
                                       let p = m.GetParameters()
                                       where p.Length == 1
                                       where m.ReturnType == node.Method.ReturnType || m.ReturnType.IsGenericParameter
                                       orderby m.ContainsGenericParameters
                                       select m).First();

                                if (enumerableMethod.IsGenericMethodDefinition)
                                {
                                    enumerableMethod = enumerableMethod.MakeGenericMethod(outerProjection.Type);
                                }

                                return Expression.Call(
                                    enumerableMethod,
                                    outerQuery
                                        .UpdateSelectExpression(outerSelectExpression
                                            .UpdateProjection(new ServerProjectionExpression(
                                                outerProjection))));
                            }

                            var sqlFunctionExpression
                                = node.Method.Name == nameof(Queryable.Average)
                                    ? new SqlAggregateExpression(
                                        "AVG",
                                        new SqlCastExpression(
                                            outerProjection,
                                            node.Method.ReturnType),
                                        node.Method.ReturnType)
                                    : new SqlAggregateExpression(
                                        node.Method.Name.ToUpperInvariant(),
                                        outerProjection,
                                        node.Method.ReturnType);

                            return new SingleValueRelationalQueryExpression(
                                outerSelectExpression
                                    .UpdateOrderBy(null)
                                    .UpdateProjection(new ServerProjectionExpression(
                                        sqlFunctionExpression.VisitWith(PostExpansionVisitors))));
                        }

                        case nameof(Queryable.Count):
                        case nameof(Queryable.LongCount):
                        {
                            var outerSelectExpression = outerQuery.SelectExpression;
                            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
                            var predicateLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

                            if (predicateLambda != null)
                            {
                                if (!HandlePushdown(
                                    s => s.RequiresPushdownForPredicate(),
                                    predicateLambda.Parameters[0].Name,
                                    ref outerSelectExpression,
                                    ref outerProjection))
                                {
                                    return FallbackToEnumerable();
                                }

                                var predicateBody
                                    = predicateLambda
                                        .ExpandParameters(outerProjection)
                                        .VisitWith(PostExpansionVisitors);

                                if (!IsTranslatable(predicateBody))
                                {
                                    return FallbackToEnumerable();
                                }

                                outerSelectExpression = outerSelectExpression.AddToPredicate(predicateBody);
                            }

                            if (!HandlePushdown(
                                s => s.IsDistinct || s.Limit != null || s.Offset != null || s.Grouping != null,
                                predicateLambda?.Parameters[0].Name,
                                ref outerSelectExpression,
                                ref outerProjection))
                            {
                                return FallbackToEnumerable();
                            }

                            var starFragment = new SqlFragmentExpression("*", typeof(object));

                            return new SingleValueRelationalQueryExpression(
                                outerSelectExpression
                                    .UpdateOrderBy(null)
                                    .UpdateProjection(new ServerProjectionExpression(
                                        new SqlAggregateExpression("COUNT", starFragment, node.Method.ReturnType)
                                            .VisitWith(PostExpansionVisitors))));
                        }

                        case nameof(Queryable.Aggregate):
                        {
                            // Does anyone know what remote data source
                            // this could possibly be translated to?

                            return FallbackToEnumerable();
                        }
                    }
                }

                return FallbackToEnumerable();

                MethodCallExpression FallbackToEnumerable()
                {
                    return Expression.Call(
                        node.ContainsNonLambdaExpressions()
                            ? node.Method
                            : MatchQueryableMethod(node.Method),
                        node.Arguments
                            .Zip(visitedArguments, (original, visited) => visited ?? Visit(original))
                            .Select(a => node.ContainsNonLambdaExpressions() ? a : a.UnwrapLambda() ?? a));
                }
            }

            return base.VisitMethodCall(node);
        }

        private bool IsTranslatable(Expression node) => translatabilityVisitor.Visit(node) is TranslatableExpression;

        private bool HandlePushdown(
            Func<SelectExpression, bool> condition,
            string alias,
            ref SelectExpression selectExpression,
            ref Expression projection)
        {
            if (!condition(selectExpression))
            {
                return true;
            }

            if (!IsTranslatable(projection))
            {
                return false;
            }

            Pushdown(alias, ref selectExpression);

            projection = selectExpression.Projection.Flatten().Body;

            return true;
        }

        private void Pushdown(
            string alias,
            ref SelectExpression selectExpression)
        {
            if (selectExpression.Limit == null && selectExpression.Offset == null)
            {
                selectExpression = selectExpression.UpdateOrderBy(null);
            }

            var projection
                = new SubqueryAliasDecoratingExpressionVisitor()
                    .Visit(selectExpression.Projection.Flatten().Body);

            selectExpression
                = selectExpression
                    .UpdateProjection(new ServerProjectionExpression(projection));

            var table
                = new SubqueryTableExpression(
                    new KeyPlaceholderGroupingInjectingExpressionVisitor().VisitAndConvert(
                        selectExpression,
                        nameof(VisitMethodCall)),
                    alias == null || alias.StartsWith("<>") ? "t" : alias);

            projection
                = new ProjectionReferenceRewritingExpressionVisitor(table)
                    .Visit(projection)
                    .VisitWith(PostExpansionVisitors);

            selectExpression
                = new SelectExpression(
                    new ServerProjectionExpression(projection),
                    table);
        }

        private Expression ProcessQuerySource(Expression node)
        {
            switch (node)
            {
                case GroupedRelationalQueryExpression groupedRelationalQueryExpression:
                {
                    var selectExpression = groupedRelationalQueryExpression.SelectExpression;
                    var outerKeySelector = groupedRelationalQueryExpression.OuterKeySelector;
                    var innerKeySelector = groupedRelationalQueryExpression.InnerKeySelector;

                    if (selectExpression.RequiresPushdownForPredicate())
                    {
                        Pushdown("t", ref selectExpression);

                        innerKeySelector
                            = groupedRelationalQueryExpression.InnerKeyLambda.ExpandParameters(
                                selectExpression.Projection.Flatten().Body);
                    }

                    var predicate
                        = Expression
                            .Equal(outerKeySelector, innerKeySelector)
                            .VisitWith(PostExpansionVisitors);

                    if (IsTranslatable(predicate))
                    {
                        if (groupedRelationalQueryExpression.RequiresDenullification)
                        {
                            predicate = JoinKeyDenullifyingExpressionVisitor.Instance.Visit(predicate);
                        }

                        return new EnumerableRelationalQueryExpression(
                            selectExpression.AddToPredicate(predicate));
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }

                case GroupByResultExpression groupByResultExpression:
                {
                    var uniquifier = new TableUniquifyingExpressionVisitor();

                    var oldSelectExpression = groupByResultExpression.SelectExpression;
                    var newSelectExpression = uniquifier.VisitAndConvert(oldSelectExpression, nameof(Visit));

                    var oldTables = oldSelectExpression.Table.Flatten().ToArray();
                    var newTables = newSelectExpression.Table.Flatten().ToArray();

                    var updater = new TableUpdatingExpressionVisitor(oldTables, newTables);

                    var outerKeySelector
                        = groupByResultExpression.OuterKeySelector;

                    var innerKeySelector
                        = updater
                            .Visit(groupByResultExpression.InnerKeySelector);

                    var predicate
                        = Expression
                            .Equal(outerKeySelector, innerKeySelector)
                            .VisitWith(PostExpansionVisitors);

                    var elementSelector
                        = updater
                            .Visit(groupByResultExpression.ElementSelector)
                            .VisitWith(PostExpansionVisitors);

                    if (IsTranslatable(predicate) && IsTranslatable(elementSelector))
                    {
                        return new EnumerableRelationalQueryExpression(
                            newSelectExpression
                                .UpdateProjection(new ServerProjectionExpression(elementSelector))
                                .AddToPredicate(predicate));
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }

                default:
                {
                    return node;
                }
            }
        }

        protected virtual Expression CreateRowNumberExpression(SelectExpression selectExpression)
        {
            return new SqlCastExpression(
                Expression.Subtract(
                    new SqlWindowFunctionExpression(
                        new SqlFunctionExpression("ROW_NUMBER", typeof(long)),
                        selectExpression.OrderBy ?? ComputeDefaultWindowOrderBy(selectExpression)),
                    Expression.Constant((long)1)),
                typeof(int));
        }

        protected virtual OrderByExpression ComputeDefaultWindowOrderBy(SelectExpression selectExpression)
        {
            // Not all DB providers require an ORDER BY in the ROW_NUMBER function;
            // those providers could override this method to control that.
            // This could also be an opportunity to generate an ORDER BY
            // using a configured default or clustered primary key or similar.

            return new OrderByExpression(SingleValueRelationalQueryExpression.SelectOne, false);
        }

        // TODO: Make this an extension method
        private static bool ContainsAggregateOrSubquery(Expression expression)
        {
            var visitor = new AggregateOrSubqueryFindingExpressionVisitor();

            visitor.Visit(expression);

            return visitor.FoundAggregateOrSubquery;
        }

        private class AggregateOrSubqueryFindingExpressionVisitor : ExpressionVisitor
        {
            public bool FoundAggregateOrSubquery { get; private set; }

            public override Expression Visit(Expression node)
            {
                if (FoundAggregateOrSubquery || node is SqlColumnExpression)
                {
                    return node;
                }
                else if (node is SqlAggregateExpression || node is SelectExpression)
                {
                    FoundAggregateOrSubquery = true;

                    return node;
                }
                else
                {
                    return base.Visit(node);
                }
            }
        }
    }
}
