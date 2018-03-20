using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public interface ICompilingExpressionVisitorProvider
    {
        IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context);
    }
}
