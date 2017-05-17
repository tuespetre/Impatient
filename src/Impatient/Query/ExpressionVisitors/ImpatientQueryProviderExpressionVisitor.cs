using Impatient.Query.ExpressionVisitors.Optimizing;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors
{
    public class ImpatientQueryProviderExpressionVisitor : ExpressionVisitor
    {
        private readonly ImpatientQueryProvider provider;
        private readonly IEnumerable<ExpressionVisitor> visitors;

        public ImpatientQueryProviderExpressionVisitor(ImpatientQueryProvider provider)
        {
            this.provider = provider;

            visitors = new ExpressionVisitor[]
            {
                // General optimizations
                new GroupJoinRemovingExpressionVisitor(),
                new OperatorSplittingExpressionVisitor(),
                new SelectorMergingExpressionVisitor(),
                new PartialEvaluatingExpressionVisitor(),

                // Specific to our purposes
                new QueryActivatingExpressionVisitor(provider),
                new QueryCompilingExpressionVisitor(provider),
            };
        }

        public override Expression Visit(Expression node) => visitors.Aggregate(node, (n, v) => v.Visit(n));
    }
}
