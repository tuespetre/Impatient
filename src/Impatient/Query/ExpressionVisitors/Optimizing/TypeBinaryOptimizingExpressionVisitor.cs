using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Optimizing;

public class TypeBinaryOptimizingExpressionVisitor : ExpressionVisitor
{
    protected override Expression VisitTypeBinary(TypeBinaryExpression node)
    {
        if (node.TypeOperand.IsInterface)
        {
            return node;
        }

        if (node.TypeOperand.IsAssignableFrom(node.Expression.Type))
        {
            return Expression.Constant(true);
        }

        if (!node.Expression.Type.IsAssignableFrom(node.TypeOperand))
        {
            return Expression.Constant(false);
        }

        return node;
    }
}
