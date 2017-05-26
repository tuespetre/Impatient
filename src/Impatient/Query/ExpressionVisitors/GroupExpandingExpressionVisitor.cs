using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors
{
    public class GroupExpandingExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            var visited = base.Visit(node);

            switch (visited)
            {
                case GroupByResultExpression groupByResultExpression:
                {
                    var selectExpression = groupByResultExpression.SelectExpression;
                    var uniquifier = new TableUniquifyingExpressionVisitor();
                    var oldTables = selectExpression.Table.Flatten();

                    selectExpression = uniquifier.VisitAndConvert(selectExpression, nameof(VisitMethodCall));

                    var newTables = selectExpression.Table.Flatten();

                    var replacingVisitor
                        = new ExpressionReplacingExpressionVisitor(
                            oldTables.Zip(newTables, ValueTuple.Create<Expression, Expression>)
                                .ToDictionary(t => t.Item1, t => t.Item2));

                    var outerKeySelector = groupByResultExpression.OuterKeySelector;
                    var innerKeySelector = replacingVisitor.Visit(groupByResultExpression.InnerKeySelector);
                    var elementSelector = replacingVisitor.Visit(groupByResultExpression.ElementSelector);

                    var elements
                        = new EnumerableRelationalQueryExpression(
                            selectExpression
                                .UpdateProjection(new ServerProjectionExpression(elementSelector))
                                .AddToPredicate(Expression.Equal(outerKeySelector, innerKeySelector)));

                    return ExpandGroup(visited, outerKeySelector, elements);
                }

                case GroupedRelationalQueryExpression groupedRelationalQueryExpression:
                {
                    var elements 
                        = new EnumerableRelationalQueryExpression(
                            groupedRelationalQueryExpression.SelectExpression
                                .AddToPredicate(Expression.Equal(
                                    groupedRelationalQueryExpression.OuterKeySelector,
                                    groupedRelationalQueryExpression.InnerKeySelector)));

                    return ExpandGroup(visited, groupedRelationalQueryExpression.OuterKeySelector, elements);
                }

                default:
                {
                    return visited;
                }
            }
        }

        private static Expression ExpandGroup(
            Expression expression, 
            Expression keyExpression,
            EnumerableRelationalQueryExpression elementsExpression)
        {
            if (expression.Type.FindGenericType(typeof(IGrouping<,>)) != null)
            {
                var groupingType
                    = typeof(ExpandedGrouping<,>)
                        .MakeGenericType(expression.Type.GenericTypeArguments);

                return Expression.New(
                    groupingType.GetTypeInfo().DeclaredConstructors.Single(),
                    new[] { keyExpression, elementsExpression },
                    new[] { groupingType.GetRuntimeProperty("Key"), groupingType.GetRuntimeProperty("Elements") });
            }
            else
            {
                return elementsExpression;
            }
        }

        private class ExpandedGrouping<TKey, TElement> : IGrouping<TKey, TElement>
        {
            public ExpandedGrouping(TKey key, IEnumerable<TElement> elements)
            {
                Key = key;
                Elements = elements;
            }

            public TKey Key { get; }

            public IEnumerable<TElement> Elements { get; }

            public IEnumerator<TElement> GetEnumerator() 
                => Elements?.GetEnumerator() ?? Enumerable.Empty<TElement>().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() 
                => Elements?.GetEnumerator() ?? Enumerable.Empty<TElement>().GetEnumerator();
        }
    }
}
