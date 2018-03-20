using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public interface IComposingExpressionVisitorProvider
    {
        IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context);
    }
}
