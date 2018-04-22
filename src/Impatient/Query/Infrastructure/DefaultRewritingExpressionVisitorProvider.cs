using Impatient.Query.ExpressionVisitors.Rewriting;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public class DefaultRewritingExpressionVisitorProvider : IRewritingExpressionVisitorProvider
    {
        private readonly TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor;

        public DefaultRewritingExpressionVisitorProvider(
            TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor)
        {
            this.translatabilityAnalyzingExpressionVisitor 
                = translatabilityAnalyzingExpressionVisitor 
                    ?? throw new ArgumentNullException(nameof(translatabilityAnalyzingExpressionVisitor));
        }

        public virtual IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
        {
            yield return new EqualsMethodRewritingExpressionVisitor();

            yield return new KeyEqualityRewritingExpressionVisitor(context.DescriptorSet);

            yield return new GroupingAggregationRewritingExpressionVisitor(translatabilityAnalyzingExpressionVisitor);

            yield return new TypeBinaryExpressionRewritingExpressionVisitor();

            yield return new NullableMemberRewritingExpressionVisitor();

            yield return new DateTimeMemberRewritingExpressionVisitor();

            yield return new StringMemberRewritingExpressionVisitor(translatabilityAnalyzingExpressionVisitor);

            yield return new CollectionContainsRewritingExpressionVisitor(translatabilityAnalyzingExpressionVisitor);

            yield return new EnumerableContainsRewritingExpressionVisitor(translatabilityAnalyzingExpressionVisitor);

            yield return new EnumerableQueryEqualityRewritingExpressionVisitor();

            // TODO: Consider pulling these from the Default provider.

            yield return new SqlServerCountRewritingExpressionVisitor();

            yield return new SqlServerMathMethodRewritingExpressionVisitor(translatabilityAnalyzingExpressionVisitor);
        }
    }
}
