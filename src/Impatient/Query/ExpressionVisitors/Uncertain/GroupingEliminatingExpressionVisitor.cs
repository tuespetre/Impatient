using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.ImpatientExtensions;

namespace Impatient.Query.ExpressionVisitors
{
    public class GroupingEliminatingExpressionVisitor : ExpressionVisitor
    {
        private class EscapedReferenceCountingExpressionVisitor : ProjectionExpressionVisitor
        {
            private readonly Expression targetExpression;

            public int EscapedReferenceCount { get; private set; }

            public EscapedReferenceCountingExpressionVisitor(Expression targetExpression)
            {
                this.targetExpression = targetExpression;
            }

            public override Expression Visit(Expression node)
            {
                if (InLeaf && node == targetExpression)
                {
                    EscapedReferenceCount++;
                }

                return base.Visit(node);
            }
        }

        /// <summary>
        ///     Finds all GroupBy nodes without result selector lambdas
        ///     and injects result selector lambdas that project a 
        ///     GroupingOriginationExpression.
        /// </summary>
        public class GroupingReferenceAnnotatingExpressionVisitor : ExpressionVisitor
        {
            private static Expression GetPreviousProjection(Expression node)
            {
                var current = node;

                while (node is MethodCallExpression methodCall && methodCall.Method.DeclaringType == typeof(Queryable))
                {
                    switch (methodCall.Method.Name)
                    {
                        case nameof(Queryable.GroupBy):
                        {
                            var resultSelectorIndex 
                                = methodCall.Method.GetParameters().ToList()
                                    .FindIndex(p => p.Name == "resultSelector");

                            return methodCall.Arguments[resultSelectorIndex].UnwrapLambda().Body;
                        }

                        case nameof(Queryable.GroupJoin):
                        {
                            return methodCall.Arguments[4].UnwrapLambda().Body;
                        }

                        case nameof(Queryable.Join):
                        {
                            return methodCall.Arguments[4].UnwrapLambda().Body;
                        }

                        case nameof(Queryable.Select):
                        {
                            return methodCall.Arguments[1].UnwrapLambda().Body;
                        }

                        case nameof(Queryable.SelectMany):
                        {
                            return methodCall.Arguments.Last().UnwrapLambda().Body;
                        }

                        case nameof(Queryable.Zip):
                        {
                            return methodCall.Arguments[2].UnwrapLambda().Body;
                        }
                    }

                    current = methodCall.Arguments[0];
                }

                return current;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                var @object = Visit(node.Object);
                var arguments = Visit(node.Arguments).ToArray();

                if (node.Method.DeclaringType == typeof(Queryable))
                {
                    var outerProjection = GetPreviousProjection(arguments[0]);

                    switch (node.Method.Name)
                    {
                        case nameof(Queryable.GroupBy):
                        {
                            var genericArguments = node.Method.GetGenericArguments();
                            var keyType = genericArguments[1];
                            var elementType = genericArguments[0];
                            var groupingType 
                                = genericArguments.Length == 3
                                    ? typeof(IEnumerable<>).MakeGenericType(elementType)
                                    : typeof(IGrouping<,>).MakeGenericType(keyType, elementType);

                            var reference = new GroupingReferenceExpression(node, groupingType);

                            var resultSelectorIndex
                                = node.Method.GetParameters().ToList()
                                    .FindIndex(p => p.Name == "resultSelector");

                            if (resultSelectorIndex > -1)
                            {
                                var resultSelector = arguments[resultSelectorIndex].UnwrapLambda();

                                arguments[resultSelectorIndex]
                                    = Expression.Quote(
                                        Expression.Lambda(
                                            resultSelector.ExpandParameters(resultSelector.Parameters[0], reference),
                                            resultSelector.Parameters));

                                return node.Update(@object, arguments);
                            }

                            var keyParameter = Expression.Parameter(keyType, "k");
                            var elementsParameter = Expression.Parameter(typeof(IEnumerable<>).MakeGenericType(elementType), "g");
                            var newArguments = arguments.ToList();

                            newArguments.Add(
                                Expression.Quote(
                                    Expression.Lambda(
                                        reference,
                                        keyParameter,
                                        elementsParameter)));

                            if (node.Method.MatchesGenericMethod(groupByKey))
                            {
                                return Expression.Call(
                                    groupByKeyResult.MakeGenericMethod(elementType, keyType, groupingType),
                                    newArguments);
                            }
                            else if (node.Method.MatchesGenericMethod(groupByKeyElement))
                            {
                                return Expression.Call(
                                    groupByKeyElementResult.MakeGenericMethod(elementType, keyType, groupingType),
                                    newArguments);
                            }

                            break;
                        }

                        case nameof(Queryable.Select):
                        {
                            var selector = arguments[1].UnwrapLambda();

                            arguments[1]
                                = Expression.Quote(
                                    Expression.Lambda(
                                        selector.ExpandParameters(outerProjection),
                                        selector.Parameters));

                            break;
                        }

                        case nameof(Queryable.Aggregate):
                        case nameof(Queryable.All):
                        case nameof(Queryable.Any):
                        case nameof(Queryable.Average):
                        case nameof(Queryable.Count):
                        case nameof(Queryable.First):
                        case nameof(Queryable.FirstOrDefault):
                        case nameof(Queryable.GroupJoin):
                        case nameof(Queryable.Join):
                        case nameof(Queryable.Last):
                        case nameof(Queryable.LastOrDefault):
                        case nameof(Queryable.LongCount):
                        case nameof(Queryable.Max):
                        case nameof(Queryable.Min):
                        case nameof(Queryable.OrderBy):
                        case nameof(Queryable.OrderByDescending):
                        case nameof(Queryable.SelectMany):
                        case nameof(Queryable.Single):
                        case nameof(Queryable.SingleOrDefault):
                        case nameof(Queryable.SkipWhile):
                        case nameof(Queryable.Sum):
                        case nameof(Queryable.TakeWhile):
                        case nameof(Queryable.ThenBy):
                        case nameof(Queryable.ThenByDescending):
                        case nameof(Queryable.Where):
                        case nameof(Queryable.Zip):
                        {
                            break;
                        }
                    }
                }

                return node.Update(@object, arguments);
            }
        }

        private class GroupingReferenceExpressionInjectingExpressionVisitor : ExpressionVisitor
        {
            private readonly GroupingReferenceExpression groupingReference;
            private readonly ExpressionReplacingExpressionVisitor replacingVisitor;

            public GroupingReferenceExpressionInjectingExpressionVisitor(
                GroupingReferenceExpression groupingReference,
                IEnumerable<(Expression, Expression)> mapping)
            {
                this.groupingReference = groupingReference;

                replacingVisitor = new ExpressionReplacingExpressionVisitor(mapping.Select(t => (t.Item1, t.Item2)));
            }

            public override Expression Visit(Expression node)
            {
                if (node.Type == groupingReference.Type)
                {
                    var result = replacingVisitor.Visit(node).ReduceMemberAccess();

                    if (result == groupingReference)
                    {
                        return result;
                    }
                }

                return base.Visit(node);
            }
        }

        private class QueryProjectionReducingExpressionVisitor : ExpressionVisitor
        {
            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.DeclaringType == typeof(Queryable) || node.Method.DeclaringType == typeof(Enumerable))
                {
                    var arguments = Visit(node.Arguments);

                    Expression GetProjection(int index)
                    {
                        return (arguments[index] as QueryProjectionExpression)?.Projection ?? arguments[index];
                    }

                    Expression ExpandProjection(int index, params Expression[] expansions)
                    {
                        return arguments[index].UnwrapLambda().ExpandParameters(expansions);
                    }

                    var projection = GetProjection(0);

                    switch (node.Method.Name)
                    {
                        case nameof(Queryable.GroupBy):
                        {
                            var parameters = node.Method.GetParameters().ToList();

                            // Key

                            var keySelectorIndex = parameters.FindIndex(p => p.Name == "keySelector");

                            var keySelector = ExpandProjection(keySelectorIndex, projection);

                            // Element

                            var elementSelectorIndex = parameters.FindIndex(p => p.Name == "elementSelector");

                            var elementSelector = projection;

                            if (elementSelectorIndex > -1)
                            {
                                elementSelector = ExpandProjection(elementSelectorIndex, projection);
                            }

                            // Result

                            var resultSelectorIndex = parameters.FindIndex(p => p.Name == "resultSelector");

                            if (resultSelectorIndex > -1)
                            {
                                projection
                                    = ExpandProjection(resultSelectorIndex,
                                        ExpandProjection(keySelectorIndex, projection),
                                        ExpandProjection(elementSelectorIndex, projection));
                            }
                            else
                            {
                                var genericArguments = node.Method.GetGenericArguments();
                                var keyType = genericArguments[0];
                                var elementType = genericArguments[1];
                                var groupingType = typeof(Grouping<,>).MakeGenericType(keyType, elementType);

                                projection = Expression.New(groupingType).Update(new[] { keySelector,  });
                            }

                            break;
                        }

                        case nameof(Queryable.GroupJoin):
                        {
                            projection = ExpandProjection(4, projection, GetProjection(1));

                            break;
                        }

                        case nameof(Queryable.Join):
                        {
                            projection = ExpandProjection(4, projection, GetProjection(1));

                            break;
                        }

                        case nameof(Queryable.Select):
                        {
                            projection = ExpandProjection(1, projection);

                            break;
                        }

                        case nameof(Queryable.SelectMany):
                        {
                            projection = ExpandProjection(1, projection);

                            if (node.Arguments.Count == 3)
                            {
                                projection = ExpandProjection(2, projection);
                            }

                            break;
                        }

                        case nameof(Queryable.Zip):
                        {
                            projection = ExpandProjection(2, projection, GetProjection(1));

                            break;
                        }
                    }

                    return new QueryProjectionExpression(node, projection);
                }

                return base.VisitMethodCall(node);
            }
        }

        private class QueryProjectionExpression : Expression
        {
            public QueryProjectionExpression(Expression query, Expression projection)
            {
                Query = query;
                Projection = projection;
            }

            public Expression Query { get; }

            public Expression Projection { get; }

            public override Type Type => Query.Type;

            public override ExpressionType NodeType => ExpressionType.Extension;
        }

        private class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
        {
            private readonly IEnumerable<TElement> elements;

            public Grouping(TKey key, IEnumerable<TElement> elements)
            {
                Key = key;
                this.elements = elements;
            }

            public TKey Key { get; }

            public IEnumerator<TElement> GetEnumerator() => elements.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => elements.GetEnumerator();
        }

        private class GroupingOriginationExpression : Expression
        {
            private readonly ParameterExpression keyParameter;
            private readonly ParameterExpression elementsParameter;

            public GroupingOriginationExpression(
                MethodCallExpression source, 
                Type type, 
                ParameterExpression keyParameter, 
                ParameterExpression elementsParameter)
            {
                Source = source;
                Type = type;

                this.keyParameter = keyParameter;
                this.elementsParameter = elementsParameter;
            }

            public MethodCallExpression Source { get; }

            public override Type Type { get; }

            public override ExpressionType NodeType => ExpressionType.Extension;

            public override bool CanReduce => true;

            public override Expression Reduce()
                => New(typeof(Grouping<,>).MakeGenericType(Type.GenericTypeArguments))
                    .Update(new[] { keyParameter, elementsParameter });

            protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
        }

        private class GroupingReferenceExpression : Expression
        {
            public GroupingReferenceExpression(MethodCallExpression source, Type type)
            {
                Source = source;
                Type = type;
            }

            public MethodCallExpression Source { get; }

            public override Type Type { get; }

            public override ExpressionType NodeType => ExpressionType.Extension;

            protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
        }

        private static readonly MethodInfo groupByKey 
            = GetGenericMethodDefinition((IQueryable<object> q) => q.GroupBy(x => x));

        private static readonly MethodInfo groupByKeyElement 
            = GetGenericMethodDefinition((IQueryable<object> q) => q.GroupBy(x => x, x => x));

        private static readonly MethodInfo groupByKeyResult
            = GetGenericMethodDefinition((IQueryable<object> q) => q.GroupBy(x => x, (x, y) => x));

        private static readonly MethodInfo groupByKeyElementResult 
            = GetGenericMethodDefinition((IQueryable<object> q) => q.GroupBy(x => x, x => x, (x, y) => x));
    }
}
