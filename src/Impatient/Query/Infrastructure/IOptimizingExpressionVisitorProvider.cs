using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public interface IOptimizingExpressionVisitorProvider
    {
        IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context);
    }
}
