using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class SqlServerCountRewritingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            if (node is SqlAggregateExpression sqlAggregate 
                && sqlAggregate.FunctionName == "COUNT"
                && sqlAggregate.Type == typeof(long))
            {
                return new SqlAggregateExpression(
                    "COUNT_BIG",
                    sqlAggregate.Expression,
                    sqlAggregate.Type,
                    sqlAggregate.IsDistinct);
            }
            else
            {
                return base.VisitExtension(node);
            }
        }
    }
}
