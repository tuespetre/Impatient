using Impatient.Query.ExpressionVisitors.Generating;

namespace Impatient.Query.Infrastructure
{
    public class DefaultQueryTranslatingExpressionVisitorFactory : IQueryTranslatingExpressionVisitorFactory
    {
        public QueryTranslatingExpressionVisitor Create()
        {
            return new QueryTranslatingExpressionVisitor(
                new DefaultDbCommandExpressionBuilder(), 
                new SqlServerForJsonComplexTypeSubqueryFormatter());
        }
    }
}
