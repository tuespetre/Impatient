using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query
{
    public interface IImpatientExpressionVisitorProvider
    {
        IEnumerable<ExpressionVisitor> RewritingExpressionVisitors { get; }

        IEnumerable<ExpressionVisitor> OptimizingExpressionVisitors { get; }
    }
}
