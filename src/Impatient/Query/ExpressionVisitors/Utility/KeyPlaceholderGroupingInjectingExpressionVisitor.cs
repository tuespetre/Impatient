using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    public class KeyPlaceholderGroupingInjectingExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case GroupByResultExpression groupByResultExpression:
                {
                    return KeyPlaceholderGrouping.Create(node, groupByResultExpression.OuterKeySelector);
                }

                case GroupedRelationalQueryExpression groupedRelationalQueryExpression:
                {
                    return KeyPlaceholderGrouping.Create(node, groupedRelationalQueryExpression.InnerKeySelector);
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }
    }
}
