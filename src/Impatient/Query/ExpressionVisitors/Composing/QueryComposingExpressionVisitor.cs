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

        private IEnumerable<ExpressionVisitor> ServerPostExpansionVisitors
            => rewritingExpressionVisitors.Concat(ClientPostExpansionVisitors);

        private IEnumerable<ExpressionVisitor> ClientPostExpansionVisitors
        {
            get
            {
                yield return parameterizingExpressionVisitor;
                yield return new StaticMemberSqlParameterRewritingExpressionVisitor();
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
                        ServerPostExpansionVisitors)
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

                if (node.ReturnType.IsGenericType(typeof(IOrderedQueryable<>))
                    && !body.Type.IsGenericType(typeof(IOrderedQueryable<>)))
                {
                    body
                        = Expression.New(
                            typeof(StubOrderedQueryableEnumerable<>)
                                .MakeGenericType(body.Type.GetSequenceType())
                                .GetTypeInfo()
                                .DeclaredConstructors
                                .Single(),
                            body);
                }

                return node.Update(body, parameters);
            }

            return base.VisitLambda(node);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (!node.Method.IsQueryableOrEnumerableMethod())
            {
                return base.VisitMethodCall(node);
            }

            var visitedArguments = new Expression[node.Arguments.Count];

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

            if (node.Method.HasComparerArgument()
                || node.ContainsNonLambdaDelegates()
                || node.ContainsNonLambdaExpressions())
            {
                return FallbackToEnumerable();
            }

            var outerSource = visitedArguments[0] = ProcessQuerySource(Visit(node.Arguments[0]));

            if (!(outerSource is EnumerableRelationalQueryExpression outerQuery))
            {
                return FallbackToEnumerable();
            }

            switch (node.Method.Name)
            {
                // Pass-through operations

                case nameof(Queryable.AsQueryable):
                case nameof(Enumerable.ToArray):
                case nameof(Enumerable.ToList):
                // case nameof(Enumerable.ToHashSet):
                case "ToHashSet":
                {
                    return outerQuery.WithTransformationMethod(node.Method);
                }

                // Materialization operations

                case nameof(Enumerable.ToDictionary):
                {
                    return HandleToDictionary(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Enumerable.ToLookup):
                {
                    return HandleToLookup(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                // Projection operations

                case nameof(Queryable.Select):
                {
                    return HandleSelect(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.SelectMany):
                {
                    return HandleSelectMany(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                // Join operations

                case nameof(Queryable.Join):
                {
                    return HandleJoin(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.GroupJoin):
                {
                    return HandleGroupJoin(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                // Filtering operations

                case nameof(Queryable.Where):
                {
                    return HandleWhere(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.OfType):
                {
                    return HandleOfType(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                // Grouping operations

                case nameof(Queryable.GroupBy):
                {
                    return HandleGroupBy(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                // Generation operations

                case nameof(Enumerable.Empty):
                {
                    return HandleEmpty(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Enumerable.Range):
                {
                    return HandleRange(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Enumerable.Repeat):
                {
                    return HandleRepeat(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.DefaultIfEmpty):
                {
                    return HandleDefaultIfEmpty(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                // Element operations

                case nameof(Queryable.First):
                {
                    return HandleFirst(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.FirstOrDefault):
                {
                    return HandleFirstOrDefault(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.Last):
                {
                    return HandleLast(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.LastOrDefault):
                {
                    return HandleLastOrDefault(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.Single):
                {
                    return HandleSingle(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.SingleOrDefault):
                {
                    return HandleSingleOrDefault(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.ElementAt):
                {
                    return HandleElementAt(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.ElementAtOrDefault):
                {
                    return HandleElementAtOrDefault(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                // Sorting operations

                case nameof(Queryable.OrderBy):
                case nameof(Queryable.OrderByDescending):
                case nameof(Queryable.ThenBy):
                case nameof(Queryable.ThenByDescending):
                {
                    return HandleOrderBy(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.Reverse):
                {
                    return HandleReverse(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                // Partitioning operations

                case nameof(Queryable.Take):
                {
                    return HandleTake(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.Skip):
                {
                    return HandleSkip(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.TakeWhile):
                {
                    return HandleTakeWhile(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.SkipWhile):
                {
                    return HandleSkipWhile(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                // case nameof(Queryable.TakeLast):
                case "TakeLast":
                {
                    return HandleTakeLast(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                // case nameof(Queryable.SkipLast):
                case "SkipLast":
                {
                    return HandleSkipLast(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                // Conversion operations

                case nameof(Queryable.Cast):
                {
                    return HandleCast(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                // Set operations

                case nameof(Queryable.Distinct):
                {
                    return HandleDistinct(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.Concat):
                case nameof(Queryable.Except):
                case nameof(Queryable.Intersect):
                case nameof(Queryable.Union):
                {
                    return HandleSetOperator(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                //case nameof(Queryable.Append):
                case "Append":
                {
                    return HandleAppend(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                //case nameof(Queryable.Prepend):
                case "Prepend":
                {
                    return HandlePrepend(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.Zip):
                {
                    return HandleZip(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                // Equality operations

                case nameof(Queryable.SequenceEqual):
                {
                    return HandleSequenceEqual(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                // Quantifier operations

                case nameof(Queryable.All):
                {
                    return HandleAll(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.Any):
                {
                    return HandleAny(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.Contains):
                {
                    return HandleContains(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                // Aggregation operations

                case nameof(Queryable.Average):
                case nameof(Queryable.Max):
                case nameof(Queryable.Min):
                case nameof(Queryable.Sum):
                {
                    return HandlePredefinedAggregate(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.Count):
                case nameof(Queryable.LongCount):
                {
                    return HandleCount(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                case nameof(Queryable.Aggregate):
                {
                    return HandleAggregate(outerQuery, node, visitedArguments, FallbackToEnumerable);
                }

                default:
                {
                    return FallbackToEnumerable();
                }
            }
        }

        #region query operators

        protected Expression HandleToDictionary(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            // TODO: Handle ToDictionary
            return fallbackToEnumerable();
        }

        protected Expression HandleToLookup(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            // TODO: Handle ToLookup
            return fallbackToEnumerable();
        }

        protected Expression HandleSelect(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
            var selectorLambda = node.Arguments[1].UnwrapLambda();

            var referencesIndexParameter
                = selectorLambda.Parameters.Count == 2
                    && selectorLambda.Body.References(selectorLambda.Parameters[1]);

            if (outerSelectExpression.IsDistinct || (referencesIndexParameter && (outerSelectExpression.HasOffsetOrLimit)))
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(selectorLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
            }

            if (selectorLambda.Parameters.Count == 1)
            {
                var selectorBody
                    = selectorLambda
                        .ExpandParameters(outerProjection)
                        .VisitWith(ServerPostExpansionVisitors);

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
                        .VisitWith(ServerPostExpansionVisitors);

                if (IsTranslatable(selectorBody))
                {
                    return outerQuery
                        .UpdateSelectExpression(outerSelectExpression
                            .UpdateProjection(new ServerProjectionExpression(selectorBody))
                            .AsWindowed());
                }
                else
                {
                    return fallbackToEnumerable();
                }
            }
        }

        protected Expression HandleSelectMany(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
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

            if (outerSelectExpression.RequiresPushdownForLeftSideOfJoin())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                var alias = (resultSelectorLambda ?? collectionSelectorLambda).Parameters[0].Name;

                Pushdown(alias, ref outerSelectExpression, ref outerProjection);
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

            innerSource = ProcessQuerySource(innerSource).VisitWith(ServerPostExpansionVisitors);

            if (!(innerSource is EnumerableRelationalQueryExpression innerQuery))
            {
                return fallbackToEnumerable();
            }

            var innerSelectExpression
                = new TableUniquifyingExpressionVisitor()
                    .VisitAndConvert(innerQuery.SelectExpression, nameof(VisitMethodCall));

            var innerProjection = innerSelectExpression.Projection.Flatten().Body;

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

            if (innerSelectExpression.RequiresPushdownForRightSideOfJoin() || handleAsCorrelated || defaultIfEmpty)
            {
                if (!IsTranslatable(innerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(resultSelectorLambda?.Parameters[1].Name, ref innerSelectExpression, ref innerProjection);
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
                        .VisitWith(ServerPostExpansionVisitors);

                if (!IsTranslatable(joinPredicate))
                {
                    return fallbackToEnumerable();
                }
            }

            var selector
                = (resultSelectorLambda == null
                    ? innerProjection
                    : resultSelectorLambda.ExpandParameters(
                        outerProjection,
                        innerProjection))
                    .VisitWith(ServerPostExpansionVisitors);

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
                        ? new LeftJoinTableExpression(outerTable, innerTable, joinPredicate, selector.Type)
                        : new InnerJoinTableExpression(outerTable, innerTable, joinPredicate, selector.Type)
                            as TableExpression
                    : handleAsCorrelated
                        ? defaultIfEmpty
                            ? new OuterApplyTableExpression(outerTable, innerTable, selector.Type)
                            : new CrossApplyTableExpression(outerTable, innerTable, selector.Type)
                                as TableExpression
                        : new CrossJoinTableExpression(outerTable, innerTable, selector.Type);

            return outerQuery
                .UpdateSelectExpression(outerSelectExpression
                    .UpdateProjection(projection)
                    .UpdateTable(joinExpression));
        }

        protected Expression HandleJoin(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var innerSource = visitedArguments[1] = ProcessQuerySource(Visit(node.Arguments[1]));

            if (!(innerSource is EnumerableRelationalQueryExpression innerQuery))
            {
                return fallbackToEnumerable();
            }

            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
            var outerKeyLambda = node.Arguments[2].UnwrapLambda();

            if (outerSelectExpression.RequiresPushdownForLeftSideOfJoin())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(outerKeyLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
            }

            var outerKeySelector
                = outerKeyLambda
                    .ExpandParameters(outerProjection)
                    .VisitWith(ServerPostExpansionVisitors);

            if (!IsTranslatable(outerKeySelector))
            {
                return fallbackToEnumerable();
            }

            var innerSelectExpression = innerQuery.SelectExpression;
            var innerProjection = innerSelectExpression.Projection.Flatten().Body;
            var innerKeyLambda = node.Arguments[3].UnwrapLambda();

            if (innerSelectExpression.RequiresPushdownForRightSideOfJoin())
            {
                if (!IsTranslatable(innerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(innerKeyLambda.Parameters[0].Name, ref innerSelectExpression, ref innerProjection);
            }

            var innerKeySelector
                = innerKeyLambda
                    .ExpandParameters(innerProjection)
                    .VisitWith(ServerPostExpansionVisitors);

            if (!IsTranslatable(innerKeySelector))
            {
                return fallbackToEnumerable();
            }

            var resultLambda
                = node.Arguments[4]
                    .UnwrapLambda();

            var resultSelector
                = resultLambda
                    .ExpandParameters(outerProjection, innerProjection)
                    .VisitWith(ServerPostExpansionVisitors);

            var projection
                = IsTranslatable(resultSelector)
                    ? new ServerProjectionExpression(resultSelector)
                    : new CompositeProjectionExpression(
                        outerSelectExpression.Projection,
                        new ServerProjectionExpression(innerProjection),
                        resultLambda) as ProjectionExpression;

            var table
                = new InnerJoinTableExpression(
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

        protected Expression HandleGroupJoin(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var innerSource = visitedArguments[1] = ProcessQuerySource(Visit(node.Arguments[1]));

            if (!(innerSource is EnumerableRelationalQueryExpression innerQuery))
            {
                return fallbackToEnumerable();
            }

            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
            var outerKeyLambda = node.Arguments[2].UnwrapLambda();

            var outerKeySelector
                = outerKeyLambda
                    .ExpandParameters(outerProjection)
                    .VisitWith(ServerPostExpansionVisitors);

            if (!IsTranslatable(outerKeySelector))
            {
                return fallbackToEnumerable();
            }

            var innerSelectExpression = innerQuery.SelectExpression;
            var innerProjection = innerSelectExpression.Projection.Flatten().Body;
            var innerKeyLambda = node.Arguments[3].UnwrapLambda();

            var innerKeySelector
                = innerKeyLambda
                    .ExpandParameters(innerProjection)
                    .VisitWith(ServerPostExpansionVisitors);

            if (!IsTranslatable(innerKeySelector))
            {
                return fallbackToEnumerable();
            }

            var resultLambda = node.Arguments[4].UnwrapLambda();

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
                    .VisitWith(ServerPostExpansionVisitors);

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

        protected Expression HandleWhere(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
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

            if (outerSelectExpression.RequiresPushdownForPredicate())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(predicateLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
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
                    .VisitWith(ServerPostExpansionVisitors);

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
                       let resolved = lambda.ExpandParameters(expansionParameters).VisitWith(ServerPostExpansionVisitors)
                       let translatable = IsTranslatable(resolved)
                       let expression = translatable ? resolved : unresolved.VisitWith(ClientPostExpansionVisitors)
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

            return fallbackToEnumerable();
        }

        protected Expression HandleOfType(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
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
                if (outerSelectExpression.RequiresPushdownForPredicate())
                {
                    if (!IsTranslatable(outerProjection))
                    {
                        return fallbackToEnumerable();
                    }

                    Pushdown(null, ref outerSelectExpression, ref outerProjection);
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

            return fallbackToEnumerable();
        }

        protected Expression HandleGroupBy(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
            var keySelectorLambda = node.Arguments[1].UnwrapLambda();

            var leafGatherer = new ProjectionLeafGatheringExpressionVisitor();

            leafGatherer.Visit(keySelectorLambda.Body);

            if (!leafGatherer.GatheredExpressions.Values.All(e => e.References(keySelectorLambda.Parameters[0])))
            {
                // SQL Server Says:
                // Msg 164, Level 15, State 1, Line 3
                // Each GROUP BY expression must contain at least one column that is not an outer reference.
                return fallbackToEnumerable();
            }

            if (outerSelectExpression.RequiresPushdownForGrouping())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(keySelectorLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
            }

            outerSelectExpression = outerSelectExpression.UpdateOrderBy(null);

            // Key Selector

            var keySelector
                = keySelectorLambda
                    .ExpandParameters(outerProjection)
                    .VisitWith(ServerPostExpansionVisitors);

            if (!IsTranslatable(keySelector) || keySelector.ContainsAggregateOrSubquery())
            {
                return fallbackToEnumerable();
            }

            // Element Selector

            var elementSelector = outerProjection;

            if (node.Method.GetParameters().Any(p => p.Name == "elementSelector"))
            {
                elementSelector
                    = node.Arguments[2]
                        .UnwrapLambda()
                        .ExpandParameters(elementSelector)
                        .VisitWith(ServerPostExpansionVisitors);
            }

            if (!IsTranslatable(elementSelector))
            {
                return fallbackToEnumerable();
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
                        .VisitWith(ServerPostExpansionVisitors);

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

        protected Expression HandleEmpty(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            // TODO: Handle Empty
            return fallbackToEnumerable();
        }

        protected Expression HandleRange(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            // TODO: Handle Range
            return fallbackToEnumerable();
        }

        protected Expression HandleRepeat(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            // TODO: Handle Repeat
            return fallbackToEnumerable();
        }

        protected Expression HandleDefaultIfEmpty(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
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
                return fallbackToEnumerable();
            }

            var outerProjection = outerQuery.SelectExpression.Projection.Flatten().Body;

            if (!IsTranslatable(outerProjection))
            {
                return fallbackToEnumerable();
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
                = new LeftJoinTableExpression(
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
                            new SqlColumnExpression(innerSubquery, "$empty", typeof(int?), true, null))),
                    joinExpression);

            return new EnumerableRelationalQueryExpression(selectExpression);
        }

        protected Expression HandleFirst(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
            var predicateLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

            if (predicateLambda != null)
            {
                if (outerSelectExpression.RequiresPushdownForPredicate())
                {
                    if (!IsTranslatable(outerProjection))
                    {
                        return fallbackToEnumerable();
                    }

                    Pushdown(predicateLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
                }

                var predicateBody
                    = predicateLambda
                        .ExpandParameters(outerProjection)
                        .VisitWith(ServerPostExpansionVisitors);

                if (!IsTranslatable(predicateBody))
                {
                    // TODO: fallback to Where(predicate).First()
                    return fallbackToEnumerable();
                }

                outerSelectExpression = outerSelectExpression.AddToPredicate(predicateBody);
            }

            if (outerSelectExpression.RequiresPushdownForLimit())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(predicateLambda?.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
            }

            return Expression.Call(
                GetGenericMethodDefinition((IEnumerable<object> e) => e.First())
                    .MakeGenericMethod(outerSelectExpression.Type),
                outerQuery
                    .UpdateSelectExpression(outerSelectExpression
                        .UpdateLimit(Expression.Constant(1))));
        }

        protected Expression HandleFirstOrDefault(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
            var predicateLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

            if (predicateLambda != null)
            {
                if (outerSelectExpression.RequiresPushdownForPredicate())
                {
                    if (!IsTranslatable(outerProjection))
                    {
                        return fallbackToEnumerable();
                    }

                    Pushdown(predicateLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
                }

                var predicateBody
                    = predicateLambda
                        .ExpandParameters(outerProjection)
                        .VisitWith(ServerPostExpansionVisitors);

                if (!IsTranslatable(predicateBody))
                {
                    // TODO: fallback to Where(predicate).FirstOrDefault()
                    return fallbackToEnumerable();
                }

                outerSelectExpression = outerSelectExpression.AddToPredicate(predicateBody);
            }

            if (outerSelectExpression.RequiresPushdownForLimit())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(predicateLambda?.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
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

        protected Expression HandleLast(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
            var predicateLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

            if (predicateLambda != null)
            {
                if (outerSelectExpression.RequiresPushdownForPredicate())
                {
                    if (!IsTranslatable(outerProjection))
                    {
                        return fallbackToEnumerable();
                    }

                    Pushdown(predicateLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
                }

                var predicateBody
                    = predicateLambda
                        .ExpandParameters(outerProjection)
                        .VisitWith(ServerPostExpansionVisitors);

                if (!IsTranslatable(predicateBody))
                {
                    // TODO: fallback to Where(predicate).Last()
                    return fallbackToEnumerable();
                }

                outerSelectExpression = outerSelectExpression.AddToPredicate(predicateBody);
            }

            if (outerSelectExpression.RequiresPushdownForLimit() || outerSelectExpression.HasOffsetOrLimit)
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(predicateLambda?.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
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

        protected Expression HandleLastOrDefault(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
            var predicateLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

            if (predicateLambda != null)
            {
                if (outerSelectExpression.RequiresPushdownForPredicate())
                {
                    if (!IsTranslatable(outerProjection))
                    {
                        return fallbackToEnumerable();
                    }

                    Pushdown(predicateLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
                }

                var predicateBody
                    = predicateLambda
                        .ExpandParameters(outerProjection)
                        .VisitWith(ServerPostExpansionVisitors);

                if (!IsTranslatable(predicateBody))
                {
                    // TODO: fallback to Where(predicate).LastOrDefault()
                    return fallbackToEnumerable();
                }

                outerSelectExpression = outerSelectExpression.AddToPredicate(predicateBody);
            }

            if (outerSelectExpression.RequiresPushdownForLimit() || outerSelectExpression.HasOffsetOrLimit)
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(predicateLambda?.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
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

        protected Expression HandleSingle(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
            var predicateLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

            if (predicateLambda != null)
            {
                if (outerSelectExpression.RequiresPushdownForPredicate())
                {
                    if (!IsTranslatable(outerProjection))
                    {
                        return fallbackToEnumerable();
                    }

                    Pushdown(predicateLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
                }

                var predicateBody
                    = predicateLambda
                        .ExpandParameters(outerProjection)
                        .VisitWith(ServerPostExpansionVisitors);

                if (!IsTranslatable(predicateBody))
                {
                    // TODO: fallback to Where(predicate).Single()
                    return fallbackToEnumerable();
                }

                outerSelectExpression = outerSelectExpression.AddToPredicate(predicateBody);
            }

            if (outerSelectExpression.RequiresPushdownForLimit())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(predicateLambda?.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
            }

            return Expression.Call(
                GetGenericMethodDefinition((IEnumerable<object> e) => e.Single())
                    .MakeGenericMethod(outerSelectExpression.Type),
                outerQuery
                    .UpdateSelectExpression(outerSelectExpression
                        .UpdateLimit(Expression.Constant(2))));
        }

        protected Expression HandleSingleOrDefault(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
            var predicateLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

            if (predicateLambda != null)
            {
                if (outerSelectExpression.RequiresPushdownForPredicate())
                {
                    if (!IsTranslatable(outerProjection))
                    {
                        return fallbackToEnumerable();
                    }

                    Pushdown(predicateLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
                }

                var predicateBody
                    = predicateLambda
                        .ExpandParameters(outerProjection)
                        .VisitWith(ServerPostExpansionVisitors);

                if (!IsTranslatable(predicateBody))
                {
                    // TODO: fallback to Where(predicate).SingleOrDefault()
                    return fallbackToEnumerable();
                }

                outerSelectExpression = outerSelectExpression.AddToPredicate(predicateBody);
            }

            if (outerSelectExpression.RequiresPushdownForLimit())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(predicateLambda?.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
            }

            return Expression.Call(
                GetGenericMethodDefinition((IEnumerable<object> e) => e.SingleOrDefault())
                    .MakeGenericMethod(outerSelectExpression.Type),
                outerQuery
                    .UpdateSelectExpression(outerSelectExpression
                        .UpdateLimit(Expression.Constant(2))));
        }

        protected Expression HandleElementAt(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;

            if (outerSelectExpression.RequiresPushdownForLimit())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(null, ref outerSelectExpression, ref outerProjection);
            }

            var index = Visit(node.Arguments[1]);

            if (!IsTranslatable(index))
            {
                return fallbackToEnumerable();
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

        protected Expression HandleElementAtOrDefault(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;

            if (outerSelectExpression.RequiresPushdownForLimit())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(null, ref outerSelectExpression, ref outerProjection);
            }

            var index = Visit(node.Arguments[1]);

            if (!IsTranslatable(index))
            {
                return fallbackToEnumerable();
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

        protected Expression HandleOrderBy(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
            var selectorLambda = node.Arguments[1].UnwrapLambda();
            
            if (!selectorLambda.Body.References(selectorLambda.Parameters[0]))
            {
                // SQL Server Says:
                // The ORDER BY position number 42 is out of range of the number of items in the select list.
                return fallbackToEnumerable();

                // TODO: Push down into a subquery instead of falling back to enumerable.
            }

            if (outerSelectExpression.HasOffsetOrLimit || outerSelectExpression.IsDistinct)
            {
                if (!IsTranslatable(outerProjection))
                {
                    goto ReturnOrderedEnumerable;
                }

                Pushdown(selectorLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
            }

            var selectorBody
                 = selectorLambda
                     .ExpandParameters(outerProjection)
                     .VisitWith(ServerPostExpansionVisitors);

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

            return fallbackToEnumerable();
        }

        protected Expression HandleReverse(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;

            if (outerSelectExpression.HasOffsetOrLimit || outerSelectExpression.IsDistinct)
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(null, ref outerSelectExpression, ref outerProjection);
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

        protected Expression HandleTake(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;

            if (outerSelectExpression.RequiresPushdownForLimit())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(null, ref outerSelectExpression, ref outerProjection);
            }

            var count = Visit(node.Arguments[1]);

            if (!IsTranslatable(count))
            {
                return fallbackToEnumerable();
            }

            return outerQuery
                .UpdateSelectExpression(outerSelectExpression
                    .UpdateLimit(count));
        }

        protected Expression HandleSkip(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;

            if (outerSelectExpression.RequiresPushdownForOffset())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(null, ref outerSelectExpression, ref outerProjection);
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
                return fallbackToEnumerable();
            }

            return outerQuery
                .UpdateSelectExpression(outerSelectExpression
                    .UpdateOffset(count));
        }

        protected Expression HandleTakeWhile(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
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

            if (outerSelectExpression.RequiresPushdownForPredicate())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(predicateLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
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

            if (subquerySelectExpression.RequiresPushdownForPredicate())
            {
                if (!IsTranslatable(subqueryProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(predicateLambda.Parameters[0].Name, ref subquerySelectExpression, ref subqueryProjection);
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
                    .VisitWith(ServerPostExpansionVisitors);

            if (!IsTranslatable(subqueryPredicateBody))
            {
                return fallbackToEnumerable();
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

        protected Expression HandleSkipWhile(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
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

            if (outerSelectExpression.RequiresPushdownForPredicate())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(predicateLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
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

            if (subquerySelectExpression.RequiresPushdownForPredicate())
            {
                if (!IsTranslatable(subqueryProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(predicateLambda.Parameters[0].Name, ref subquerySelectExpression, ref subqueryProjection);
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
                    .VisitWith(ServerPostExpansionVisitors);

            if (!IsTranslatable(subqueryPredicateBody))
            {
                return fallbackToEnumerable();
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

        protected Expression HandleTakeLast(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            // TODO: Handle TakeLast
            return fallbackToEnumerable();
        }

        protected Expression HandleSkipLast(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            // TODO: Handle SkipLast
            return fallbackToEnumerable();
        }

        protected Expression HandleCast(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
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

            return fallbackToEnumerable();
        }

        protected Expression HandleDistinct(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;

            if (outerSelectExpression.HasOffsetOrLimit || outerSelectExpression.IsDistinct)
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(null, ref outerSelectExpression, ref outerProjection);
            }

            if (!IsTranslatable(outerProjection))
            {
                return fallbackToEnumerable();
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

        protected Expression HandleSetOperator(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var innerSource = visitedArguments[1] = ProcessQuerySource(Visit(node.Arguments[1]));

            if (!(innerSource is EnumerableRelationalQueryExpression innerQuery))
            {
                return fallbackToEnumerable();
            }

            var outerSelectExpression = outerQuery.SelectExpression.UpdateOrderBy(null);
            var innerSelectExpression = innerQuery.SelectExpression.UpdateOrderBy(null);

            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
            var innerProjection = innerSelectExpression.Projection.Flatten().Body;

            if (!IsTranslatable(outerProjection) || !IsTranslatable(innerProjection))
            {
                return fallbackToEnumerable();
            }

            var outerGatherer = new ProjectionLeafGatheringExpressionVisitor();
            var innerGatherer = new ProjectionLeafGatheringExpressionVisitor();
            outerGatherer.Visit(outerProjection);
            innerGatherer.Visit(innerProjection);

            var shapesMatch = false;

            if (outerGatherer.GatheredExpressions.Count == innerGatherer.GatheredExpressions.Count)
            {
                shapesMatch = true;

                var zippedPairs
                    = outerGatherer.GatheredExpressions
                        .Zip(innerGatherer.GatheredExpressions, ValueTuple.Create);

                foreach (var (pair1, pair2) in zippedPairs)
                {
                    if (pair1.Key != pair2.Key || pair1.Value.Type != pair2.Value.Type)
                    {
                        shapesMatch = false;
                        break;
                    }
                }
            }

            if (!shapesMatch)
            {
                return fallbackToEnumerable();
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
                return fallbackToEnumerable();
            }*/

            var setOperatorExpression = default(SetOperatorTableExpression);

            switch (node.Method.Name)
            {
                case nameof(Queryable.Concat):
                {
                    setOperatorExpression = new UnionAllTableExpression(outerSelectExpression, innerSelectExpression);
                    break;
                }

                case nameof(Queryable.Except):
                {
                    setOperatorExpression = new ExceptTableExpression(outerSelectExpression, innerSelectExpression);
                    break;
                }

                case nameof(Queryable.Intersect):
                {
                    setOperatorExpression = new IntersectTableExpression(outerSelectExpression, innerSelectExpression);
                    break;
                }

                case nameof(Queryable.Union):
                {
                    setOperatorExpression = new UnionTableExpression(outerSelectExpression, innerSelectExpression);
                    break;
                }
            }

            return new EnumerableRelationalQueryExpression(
                new SelectExpression(
                    new ServerProjectionExpression(
                        new ProjectionReferenceRewritingExpressionVisitor(setOperatorExpression)
                            .Visit(outerProjection)
                            .VisitWith(ServerPostExpansionVisitors)),
                    setOperatorExpression));
        }

        protected Expression HandleAppend(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            // TODO: Handle Append
            return fallbackToEnumerable();
        }

        protected Expression HandlePrepend(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            // TODO: Handle Prepend
            return fallbackToEnumerable();
        }

        protected Expression HandleZip(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var innerSource = visitedArguments[1] = ProcessQuerySource(Visit(node.Arguments[1]));

            if (!(innerSource is EnumerableRelationalQueryExpression innerQuery))
            {
                return fallbackToEnumerable();
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

            if (outerSelectExpression.RequiresPushdownForLeftSideOfJoin())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(resultSelectorLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
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

            if (innerSelectExpression.RequiresPushdownForLeftSideOfJoin())
            {
                if (!IsTranslatable(innerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(resultSelectorLambda.Parameters[1].Name, ref innerSelectExpression, ref innerProjection);
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
                    .VisitWith(ServerPostExpansionVisitors);

            var projection
                = IsTranslatable(resultSelectorBody)
                    ? new ServerProjectionExpression(resultSelectorBody)
                    : new CompositeProjectionExpression(
                        outerSelectExpression.Projection,
                        innerSelectExpression.Projection,
                        resultSelectorLambda) as ProjectionExpression;

            var table
                = new InnerJoinTableExpression(
                    outerSelectExpression.Table,
                    innerSelectExpression.Table as AliasedTableExpression,
                    predicate,
                    resultSelectorBody.Type);

            return outerQuery
                .UpdateSelectExpression(outerSelectExpression
                    .UpdateProjection(projection)
                    .UpdateTable(table));
        }

        protected Expression HandleSequenceEqual(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var innerSource = visitedArguments[1] = ProcessQuerySource(Visit(node.Arguments[1]));

            if (!(innerSource is EnumerableRelationalQueryExpression innerQuery))
            {
                return fallbackToEnumerable();
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

            if (outerSelectExpression.RequiresPushdownForLeftSideOfJoin())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(null, ref outerSelectExpression, ref outerProjection);
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

            if (innerSelectExpression.RequiresPushdownForRightSideOfJoin())
            {
                if (!IsTranslatable(innerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(null, ref innerSelectExpression, ref innerProjection);
            }

            var outerRowNumber = (outerProjection as NewExpression).Arguments[1];
            var innerRowNumber = (innerProjection as NewExpression).Arguments[1];

            var resultSelectExpression
                = new SelectExpression(
                    new ServerProjectionExpression(
                        Expression.Constant(1)),
                    new FullJoinTableExpression(
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

        protected Expression HandleAll(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
            var predicateLambda = node.Arguments[1].UnwrapLambda();

            if (outerSelectExpression.RequiresPushdownForPredicate())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(predicateLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
            }

            var predicateBody
                = predicateLambda
                    .ExpandParameters(outerProjection)
                    .VisitWith(ServerPostExpansionVisitors);

            if (!IsTranslatable(predicateBody))
            {
                return fallbackToEnumerable();
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

        protected Expression HandleAny(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
            var predicateLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

            if (predicateLambda != null)
            {
                if (outerSelectExpression.RequiresPushdownForPredicate())
                {
                    if (!IsTranslatable(outerProjection))
                    {
                        return fallbackToEnumerable();
                    }

                    Pushdown(predicateLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
                }

                var predicateBody
                    = predicateLambda
                        .ExpandParameters(outerProjection)
                        .VisitWith(ServerPostExpansionVisitors);

                if (!IsTranslatable(predicateBody))
                {
                    return fallbackToEnumerable();
                }

                outerSelectExpression = outerSelectExpression.AddToPredicate(predicateBody);
            }

            if (!IsTranslatable(outerProjection))
            {
                return fallbackToEnumerable();
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

        protected Expression HandleContains(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;

            if (!IsTranslatable(outerProjection))
            {
                return fallbackToEnumerable();
            }

            var valueExpression 
                = node.Arguments[1]
                    .VisitWith(ServerPostExpansionVisitors);

            if (!IsTranslatable(valueExpression))
            {
                return fallbackToEnumerable();
            }

            // TODO: Test with a scalar subquery as the value e.g. (SELECT 1) IN (SELECT 1)
            return new SingleValueRelationalQueryExpression(
                new SelectExpression(
                    new ServerProjectionExpression(
                        new SqlCastExpression(
                            Expression.Condition(
                                new SqlInExpression(valueExpression, outerSelectExpression),
                                Expression.Constant(true),
                                Expression.Constant(false)),
                            typeof(bool)))));
        }

        protected Expression HandlePredefinedAggregate(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
            var selectorLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

            if (selectorLambda != null)
            {
                if (outerSelectExpression.IsDistinct)
                {
                    if (!IsTranslatable(outerProjection))
                    {
                        return fallbackToEnumerable();
                    }

                    Pushdown(selectorLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
                }

                outerProjection
                    = selectorLambda
                        .ExpandParameters(outerProjection)
                        .VisitWith(ServerPostExpansionVisitors);

                outerSelectExpression
                    = outerSelectExpression
                        .UpdateProjection(new ServerProjectionExpression(outerProjection));
            }

            if (outerSelectExpression.RequiresPushdownForAggregate())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(selectorLambda?.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
            }

            if (!IsTranslatable(outerProjection))
            {
                return fallbackToEnumerable();
            }

            if (outerProjection.ContainsAggregateOrSubquery())
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

            Expression aggregateExpression
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

            if (node.Method.Name == nameof(Queryable.Sum)
                && node.Method.ReturnType.IsNullableType())
            {
                aggregateExpression
                    = Expression.Coalesce(
                        aggregateExpression,
                        Expression.Constant(
                            Activator.CreateInstance(node.Method.ReturnType.UnwrapNullableType())));
            }

            return new SingleValueRelationalQueryExpression(
                outerSelectExpression
                    .UpdateOrderBy(null)
                    .UpdateProjection(new ServerProjectionExpression(
                        aggregateExpression.VisitWith(ServerPostExpansionVisitors))));
        }

        protected Expression HandleCount(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            var outerSelectExpression = outerQuery.SelectExpression;
            var outerProjection = outerSelectExpression.Projection.Flatten().Body;
            var predicateLambda = node.Arguments.ElementAtOrDefault(1)?.UnwrapLambda();

            if (predicateLambda != null)
            {
                if (outerSelectExpression.RequiresPushdownForPredicate())
                {
                    if (!IsTranslatable(outerProjection))
                    {
                        return fallbackToEnumerable();
                    }

                    Pushdown(predicateLambda.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
                }

                var predicateBody
                    = predicateLambda
                        .ExpandParameters(outerProjection)
                        .VisitWith(ServerPostExpansionVisitors);

                if (!IsTranslatable(predicateBody))
                {
                    return fallbackToEnumerable();
                }

                outerSelectExpression = outerSelectExpression.AddToPredicate(predicateBody);
            }

            if (outerSelectExpression.RequiresPushdownForAggregate())
            {
                if (!IsTranslatable(outerProjection))
                {
                    return fallbackToEnumerable();
                }

                Pushdown(predicateLambda?.Parameters[0].Name, ref outerSelectExpression, ref outerProjection);
            }

            var starFragment = new SqlFragmentExpression("*", typeof(object));

            return new SingleValueRelationalQueryExpression(
                outerSelectExpression
                    .UpdateOrderBy(null)
                    .UpdateProjection(new ServerProjectionExpression(
                        new SqlAggregateExpression("COUNT", starFragment, node.Method.ReturnType)
                            .VisitWith(ServerPostExpansionVisitors))));
        }

        protected Expression HandleAggregate(
            EnumerableRelationalQueryExpression outerQuery,
            MethodCallExpression node,
            Expression[] visitedArguments,
            Func<Expression> fallbackToEnumerable)
        {
            return fallbackToEnumerable();
        }

        #endregion

        private bool IsTranslatable(Expression node) => translatabilityVisitor.Visit(node) is TranslatableExpression;

        private void Pushdown(string alias, ref SelectExpression selectExpression, ref Expression projection)
        {
            if (selectExpression.Limit == null && selectExpression.Offset == null)
            {
                selectExpression = selectExpression.UpdateOrderBy(null);
            }

            projection = new SubqueryAliasDecoratingExpressionVisitor().Visit(projection);

            selectExpression
                = selectExpression.UpdateProjection(
                    new ServerProjectionExpression(projection));

            selectExpression
                = new KeyPlaceholderGroupingInjectingExpressionVisitor()
                    .VisitAndConvert(selectExpression, nameof(VisitMethodCall));

            alias = (alias == null || alias.StartsWith("<>")) ? "t" : alias;

            var table = new SubqueryTableExpression(selectExpression, alias);

            projection
                = new ProjectionReferenceRewritingExpressionVisitor(table)
                    .Visit(projection)
                    .VisitWith(ServerPostExpansionVisitors);

            selectExpression = new SelectExpression(new ServerProjectionExpression(projection), table);
        }

        protected virtual Expression ProcessQuerySource(Expression node)
        {
            if (node is null || !node.Type.IsSequenceType())
            {
                return node;
            }

            switch (node)
            {
                case GroupedRelationalQueryExpression query:
                {
                    var selectExpression = query.SelectExpression;
                    var outerKeySelector = query.OuterKeySelector;
                    var innerKeySelector = query.InnerKeySelector;

                    if (selectExpression.RequiresPushdownForPredicate())
                    {
                        var projection = selectExpression.Projection.Flatten().Body;

                        Pushdown("t", ref selectExpression, ref projection);

                        innerKeySelector = query.InnerKeyLambda.ExpandParameters(projection);
                    }

                    var predicate
                        = Expression
                            .Equal(outerKeySelector, innerKeySelector)
                            .VisitWith(ServerPostExpansionVisitors);

                    if (IsTranslatable(predicate))
                    {
                        if (query.RequiresDenullification)
                        {
                            predicate = JoinKeyDenullifyingExpressionVisitor.Instance.Visit(predicate);
                        }

                        return new EnumerableRelationalQueryExpression(
                            selectExpression.AddToPredicate(predicate));
                    }
                    else
                    {
                        throw new NotImplementedException();
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
                            .VisitWith(ServerPostExpansionVisitors);

                    var elementSelector
                        = updater
                            .Visit(groupByResultExpression.ElementSelector)
                            .VisitWith(ServerPostExpansionVisitors);

                    if (IsTranslatable(predicate) && IsTranslatable(elementSelector))
                    {
                        return new EnumerableRelationalQueryExpression(
                            newSelectExpression
                                .UpdateProjection(new ServerProjectionExpression(elementSelector))
                                .AddToPredicate(predicate));
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

                #region  Experimental OPENJSON support

                case SqlFunctionExpression sqlFunctionExpression
                when sqlFunctionExpression.FunctionName == "JSON_QUERY":
                {
                    var openJsonTable
                        = new TableValuedExpressionTableExpression(
                            new SqlFunctionExpression(
                                "OPENJSON",
                                sqlFunctionExpression.Type,
                                sqlFunctionExpression.Arguments),
                            "j",
                            sqlFunctionExpression.Type);

                    return new EnumerableRelationalQueryExpression(
                        new SelectExpression(
                            new ServerProjectionExpression(
                                new SqlColumnExpression(
                                    openJsonTable,
                                    "value",
                                    sqlFunctionExpression.Type.GetSequenceType(),
                                    true,
                                    null)),
                            openJsonTable));
                }

                case SqlExpression sqlExpression:
                {
                    var openJsonTable
                        = new TableValuedExpressionTableExpression(
                            new SqlFunctionExpression("OPENJSON", sqlExpression.Type, sqlExpression),
                            "j",
                            sqlExpression.Type);

                    return new EnumerableRelationalQueryExpression(
                        new SelectExpression(
                            new ServerProjectionExpression(
                                new SqlColumnExpression(
                                    openJsonTable,
                                    "value",
                                    sqlExpression.Type.GetSequenceType(),
                                    true,
                                    null)),
                            openJsonTable));
                }

                case MemberExpression memberExpression:
                {
                    var path = new List<MemberInfo>();
                    var root = default(Expression);
                    var current = memberExpression;

                    do
                    {
                        path.Insert(0, current.Member);
                        root = current.Expression;
                        current = root as MemberExpression;
                    }
                    while (current != null);

                    if (!(root is SqlExpression sqlExpression))
                    {
                        return node;
                    }

                    var openJsonTable
                        = new TableValuedExpressionTableExpression(
                            new SqlFunctionExpression(
                                "OPENJSON",
                                memberExpression.Type,
                                sqlExpression,
                                Expression.Constant($"$.{string.Join(".", path.GetPropertyNamesForJson())}")),
                            "j",
                            memberExpression.Type);

                    return new EnumerableRelationalQueryExpression(
                        new SelectExpression(
                            new ServerProjectionExpression(
                                new SqlColumnExpression(
                                    openJsonTable,
                                    "value",
                                    memberExpression.Type.GetSequenceType(),
                                    true,
                                    null)),
                            openJsonTable));
                }

                #endregion

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
    }
}
