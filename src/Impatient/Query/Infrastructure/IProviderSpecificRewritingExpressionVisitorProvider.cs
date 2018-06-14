using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public interface IProviderSpecificRewritingExpressionVisitorProvider
    {
        IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context);
    }
}
