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
        private readonly ITypeMappingProvider typeMappingProvider;

        public DefaultRewritingExpressionVisitorProvider(
            TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor,
            ITypeMappingProvider typeMappingProvider)
        {
            this.translatabilityAnalyzingExpressionVisitor
                = translatabilityAnalyzingExpressionVisitor
                ?? throw new ArgumentNullException(nameof(translatabilityAnalyzingExpressionVisitor));

            this.typeMappingProvider
                = typeMappingProvider
                ?? throw new ArgumentNullException(nameof(typeMappingProvider));
        }

        public virtual IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
        {
            /*
            
            Order-dependency notes:

            - ListContainsToEnumerableContainsRewritingExpressionVisitor
                - has to run before EnumerableContainsRewritingExpressionVisitor

            - GroupingAggregationRewritingExpressionVisitor
                - has to run before the QueryComposingExpressionVisitor recurses

            - SqlServerStringJoinRewritingExpressionVisitor
                - has to run after the QueryComposingExpressionVisitor recurses

            Ideally none of these would be order-dependent beyond running before
            or after the QueryComposingExpressionVisitor recurses.

            */

            yield return new ListContainsToEnumerableContainsRewritingExpressionVisitor();

            yield return new EqualsMethodRewritingExpressionVisitor();

            yield return new KeyEqualityRewritingExpressionVisitor(context.DescriptorSet);

            yield return new GroupingAggregationRewritingExpressionVisitor(translatabilityAnalyzingExpressionVisitor);

            yield return new TypeBinaryExpressionRewritingExpressionVisitor();

            yield return new NullableMemberRewritingExpressionVisitor();

            yield return new DateTimeMemberRewritingExpressionVisitor();

            yield return new StringMemberRewritingExpressionVisitor();

            yield return new CollectionContainsRewritingExpressionVisitor();

            yield return new EnumerableContainsRewritingExpressionVisitor();

            yield return new EnumerableQueryEqualityRewritingExpressionVisitor();

            yield return new EnumHasFlagRewritingExpressionVisitor(typeMappingProvider);
        }
    }
}
