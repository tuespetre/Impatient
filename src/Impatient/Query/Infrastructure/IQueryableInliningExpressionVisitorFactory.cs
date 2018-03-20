using Impatient.Query.ExpressionVisitors.Optimizing;

namespace Impatient.Query.Infrastructure
{
    public interface IQueryableInliningExpressionVisitorFactory
    {
        QueryableInliningExpressionVisitor Create(QueryProcessingContext context);
    }
}
