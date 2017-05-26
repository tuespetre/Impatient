using Impatient.Query.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.ExpressionVisitors.Rewriting;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query
{
    public class DefaultImpatientExpressionVisitorProvider : IImpatientExpressionVisitorProvider
    {
        private static readonly TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor
            = new TranslatabilityAnalyzingExpressionVisitor();

        public IEnumerable<ExpressionVisitor> RewritingExpressionVisitors { get; } = new ExpressionVisitor[]
        {
            new CollectionContainsRewritingExpressionVisitor(translatabilityAnalyzingExpressionVisitor),
            new EnumerableContainsRewritingExpressionVisitor(translatabilityAnalyzingExpressionVisitor),
        };

        public IEnumerable<ExpressionVisitor> OptimizingExpressionVisitors { get; } = new ExpressionVisitor[]
        {
            new OperatorSplittingExpressionVisitor(),
            new SelectorMergingExpressionVisitor(),
            new PartialEvaluatingExpressionVisitor(),
            new BooleanOptimizingExpressionVisitor(),
        };

        public QueryTranslatingExpressionVisitor QueryTranslatingExpressionVisitor 
            => new QueryTranslatingExpressionVisitor(this);

        public TranslatabilityAnalyzingExpressionVisitor TranslatabilityAnalyzingExpressionVisitor
            => translatabilityAnalyzingExpressionVisitor;
    }
}
