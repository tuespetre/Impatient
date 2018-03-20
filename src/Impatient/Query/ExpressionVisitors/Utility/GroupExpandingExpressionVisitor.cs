using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    /// <summary>
    /// An <see cref="ExpressionVisitor"/> that finds instances of 
    /// <see cref="GroupByResultExpression"/> and <see cref="GroupedRelationalQueryExpression"/>
    /// and replaces them with an expanded form that can be compiled.
    /// </summary>
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

                    return ExpandedGrouping.Create(visited, outerKeySelector, elements);
                }

                case GroupedRelationalQueryExpression groupedRelationalQueryExpression:
                {
                    var outerKeySelector = groupedRelationalQueryExpression.OuterKeySelector;
                    var innerKeySelector = groupedRelationalQueryExpression.InnerKeySelector;

                    if (groupedRelationalQueryExpression.RequiresDenullification)
                    {
                        outerKeySelector = JoinKeyDenullifyingExpressionVisitor.Instance.Visit(outerKeySelector);
                        innerKeySelector = JoinKeyDenullifyingExpressionVisitor.Instance.Visit(innerKeySelector);
                    }

                    var elements
                        = new EnumerableRelationalQueryExpression(
                            groupedRelationalQueryExpression.SelectExpression
                                .AddToPredicate(Expression.Equal(outerKeySelector, innerKeySelector)));

                    return ExpandedGrouping.Create(visited, groupedRelationalQueryExpression.OuterKeySelector, elements);
                }

                default:
                {
                    return visited;
                }
            }
        }
    }
}
