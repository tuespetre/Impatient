using Impatient.Extensions;
using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Optimizing
{
    public class RedundantConversionStrippingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitUnary(UnaryExpression node)
        {
            var visited = Visit(node.Operand);

            if (node.NodeType == ExpressionType.Convert || node.NodeType == ExpressionType.ConvertChecked)
            {
                if (visited.Type == node.Type)
                {
                    return visited;
                }
                else if (visited is SqlColumnExpression sqlColumnExpression
                    && node.Type.UnwrapNullableType() == sqlColumnExpression.Type)
                {
                    return new SqlColumnExpression(
                        sqlColumnExpression.Table,
                        sqlColumnExpression.ColumnName,
                        node.Type,
                        sqlColumnExpression.IsNullable);
                }
            }

            return node.Update(visited);
        }
    }
}
