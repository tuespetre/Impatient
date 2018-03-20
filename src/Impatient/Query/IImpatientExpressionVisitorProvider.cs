using Impatient.Query.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Generating;
using Impatient.Query.ExpressionVisitors.Utility;
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

        IEnumerable<ExpressionVisitor> ComposingExpressionVisitors { get; }
    }
}
