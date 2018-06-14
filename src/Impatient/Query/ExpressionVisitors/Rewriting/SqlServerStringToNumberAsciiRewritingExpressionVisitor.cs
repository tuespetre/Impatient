using Impatient.Extensions;
using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class SqlServerStringToNumberAsciiRewritingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (node.NodeType == ExpressionType.Convert
                && node.Operand.Type.IsTextType()
                && node.Type.IsNumericType())
            {
                return new SqlFunctionExpression("ASCII", node.Type, node.Operand);
            }

            return base.VisitUnary(node);
        }
    }
}
