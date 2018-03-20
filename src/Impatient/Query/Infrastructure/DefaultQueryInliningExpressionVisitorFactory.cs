using Impatient.Query.ExpressionVisitors.Optimizing;

namespace Impatient.Query.Infrastructure
{
    public class DefaultQueryInliningExpressionVisitorFactory : IQueryableInliningExpressionVisitorFactory
    {
        public QueryableInliningExpressionVisitor Create(QueryProcessingContext context)
        {
            return new QueryableInliningExpressionVisitor(context.QueryProvider, context.ParameterMapping);
        }
    }
}
