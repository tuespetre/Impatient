using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class ObjectToStringRewritingExpressionVisitor : ExpressionVisitor
    {        
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(ToString) && node.Arguments.Count == 0 && node.Object != null)
            {
                return new SqlFunctionExpression(
                    "CONVERT", 
                    node.Type, 
                    new SqlFragmentExpression("VARCHAR(100)"), 
                    node.Object);
            }

            return node;
        }
    }
}
