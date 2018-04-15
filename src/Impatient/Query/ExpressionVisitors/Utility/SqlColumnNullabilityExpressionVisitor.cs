using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    public class SqlColumnNullabilityExpressionVisitor : ProjectionExpressionVisitor
    {
        protected override Expression VisitLeaf(Expression node)
        {
            switch (node)
            {
                case SqlColumnExpression sqlColumnExpression:
                {
                    return new SqlColumnExpression(
                        sqlColumnExpression.Table,
                        sqlColumnExpression.ColumnName,
                        sqlColumnExpression.Type,
                        isNullable: true);
                }

                default:
                {
                    return node;
                }
            }
        }
    }
}
