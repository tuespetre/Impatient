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
            // 'Simple' replacement rewriters

            yield return new ListContainsToEnumerableContainsRewritingExpressionVisitor();

            yield return new EqualsMethodRewritingExpressionVisitor();

            // 'Core' rewriters

            yield return new KeyEqualityRewritingExpressionVisitor(context.DescriptorSet);

            yield return new GroupingAggregationRewritingExpressionVisitor(translatabilityAnalyzingExpressionVisitor);

            yield return new TypeBinaryExpressionRewritingExpressionVisitor();

            yield return new NullableMemberRewritingExpressionVisitor();

            yield return new DateTimeMemberRewritingExpressionVisitor();

            yield return new StringMemberRewritingExpressionVisitor();

            yield return new CollectionContainsRewritingExpressionVisitor();

            yield return new EnumerableContainsRewritingExpressionVisitor();

            yield return new EnumerableQueryEqualityRewritingExpressionVisitor();

            // SQL Server specific rewriters (should be pulled from the default provider at some point)

            yield return new ObjectToStringRewritingExpressionVisitor();

            yield return new StringToNumberAsciiRewritingExpressionVisitor();

            yield return new SqlServerCountRewritingExpressionVisitor();

            yield return new SqlServerMathMethodRewritingExpressionVisitor();

            yield return new SqlServerJsonMemberRewritingExpressionVisitor();
        }
    }
}
