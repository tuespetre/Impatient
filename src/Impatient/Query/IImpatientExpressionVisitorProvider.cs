using Impatient.Query.ExpressionVisitors;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query
{
    public interface IImpatientExpressionVisitorProvider
    {
        QueryTranslatingExpressionVisitor QueryTranslatingExpressionVisitor { get; }

        TranslatabilityAnalyzingExpressionVisitor TranslatabilityAnalyzingExpressionVisitor { get; }

        IEnumerable<ExpressionVisitor> RewritingExpressionVisitors { get; }

        IEnumerable<ExpressionVisitor> OptimizingExpressionVisitors { get; }
    }
}
