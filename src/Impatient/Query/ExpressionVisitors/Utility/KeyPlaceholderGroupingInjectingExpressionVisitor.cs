using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    /// <summary>
    /// An <see cref="ExpressionVisitor"/> that replaces any instances of
    /// <see cref="GroupByResultExpression"/> or <see cref="GroupedRelationalQueryExpression"/>
    /// with instances of <see cref="MemberInitExpression"/> that represent the creation of a
    /// <see cref="KeyPlaceholderGrouping{TKey, TElement}"/>. This is needed for pushing down
    /// a grouping into a subquery. It basically replaces the 'grouping column' with a reference
    /// to the grouping key, and if the grouping is then referenced 'above and outside' of the subquery,
    /// it can be reconstructed by using the key to correlate things.
    /// </summary>
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
