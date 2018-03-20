using Impatient.Query.ExpressionVisitors.Generating;

namespace Impatient.Query.Infrastructure
{
    public interface IQueryTranslatingExpressionVisitorFactory
    {
        QueryTranslatingExpressionVisitor Create();
    }
}
