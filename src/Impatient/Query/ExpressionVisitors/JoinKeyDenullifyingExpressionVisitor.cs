using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors
{
    public class JoinKeyDenullifyingExpressionVisitor : ExpressionVisitor
    {
        public static JoinKeyDenullifyingExpressionVisitor Instance { get; }
            = new JoinKeyDenullifyingExpressionVisitor();

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case SqlColumnExpression sqlColumnExpression:
                {
                    return new SqlColumnExpression(
                        sqlColumnExpression.Table,
                        sqlColumnExpression.ColumnName,
                        sqlColumnExpression.Type,
                        isNullable: false);
                }

                default:
                {
                    return node;
                }
            }
        }
    }
}
