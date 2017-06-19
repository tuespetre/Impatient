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
                    var uniquifier = new TableUniquifyingExpressionVisitor();

                    var oldSelectExpression = groupByResultExpression.SelectExpression;
                    var newSelectExpression = uniquifier.VisitAndConvert(oldSelectExpression, nameof(VisitMethodCall));

                    var oldTables = oldSelectExpression.Table.Flatten().ToArray();
                    var newTables = newSelectExpression.Table.Flatten().ToArray();

                    var updater = new TableUpdatingExpressionVisitor(oldTables, newTables);

                    var outerKeySelector = groupByResultExpression.OuterKeySelector;
                    var innerKeySelector = updater.Visit(groupByResultExpression.InnerKeySelector);
                    var elementSelector = updater.Visit(groupByResultExpression.ElementSelector);

                    var elements
                        = new EnumerableRelationalQueryExpression(
                            newSelectExpression
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
