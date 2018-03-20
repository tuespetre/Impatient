using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    public class TableUniquifyingExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case SqlColumnExpression sqlColumnExpression:
                {
                    return sqlColumnExpression;
                }

                case BaseTableExpression baseTableExpression:
                {
                    return baseTableExpression.Clone();
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }
    }
}
