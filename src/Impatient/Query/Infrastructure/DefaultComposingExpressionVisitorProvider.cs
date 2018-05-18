using Impatient.Query.ExpressionVisitors.Composing;
using Impatient.Query.ExpressionVisitors.Rewriting;
using Impatient.Query.ExpressionVisitors.Utility;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public class DefaultComposingExpressionVisitorProvider : IComposingExpressionVisitorProvider
    {
        private readonly TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor;
        private readonly IRewritingExpressionVisitorProvider rewritingExpressionVisitorProvider;

        public DefaultComposingExpressionVisitorProvider(
            TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor,
            IRewritingExpressionVisitorProvider rewritingExpressionVisitorProvider)
        {
            this.translatabilityAnalyzingExpressionVisitor = translatabilityAnalyzingExpressionVisitor;
            this.rewritingExpressionVisitorProvider = rewritingExpressionVisitorProvider;
        }

        public virtual IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
        {
            yield return new KeyEqualityComposingExpressionVisitor(context.DescriptorSet);

            yield return new NavigationComposingExpressionVisitor(context.DescriptorSet.NavigationDescriptors);
            
            yield return new TableAliasComposingExpressionVisitor();

            yield return new QueryComposingExpressionVisitor(
                translatabilityAnalyzingExpressionVisitor, 
                rewritingExpressionVisitorProvider.CreateExpressionVisitors(context),
                new SqlParameterRewritingExpressionVisitor(context.ParameterMapping.Values));
        }
    }
}
