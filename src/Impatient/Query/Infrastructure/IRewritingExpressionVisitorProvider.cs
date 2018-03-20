using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public interface IRewritingExpressionVisitorProvider
    {
        IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context);
    }
}
