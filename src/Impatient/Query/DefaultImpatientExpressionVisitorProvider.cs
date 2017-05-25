using Impatient.Query.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.ExpressionVisitors.Rewriting;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query
{
    public class DefaultImpatientExpressionVisitorProvider : IImpatientExpressionVisitorProvider
    {
        private static readonly ExpressionVisitor[] rewritingExpressionVisitors = new ExpressionVisitor[]
        {
            new CollectionContainsRewritingExpressionVisitor(),
            new EnumerableContainsRewritingExpressionVisitor(),
        };

        private static readonly ExpressionVisitor[] optimizingExpressionVisitors = new ExpressionVisitor[]
        {
            new OperatorSplittingExpressionVisitor(),
            new SelectorMergingExpressionVisitor(),
            new PartialEvaluatingExpressionVisitor(),
            new BooleanOptimizingExpressionVisitor(),
        };

        public IEnumerable<ExpressionVisitor> RewritingExpressionVisitors => rewritingExpressionVisitors;

        public IEnumerable<ExpressionVisitor> OptimizingExpressionVisitors => optimizingExpressionVisitors;
    }
}
