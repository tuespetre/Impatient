using Impatient.Query.ExpressionVisitors.Rewriting;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure;

public class DefaultRewritingExpressionVisitorProvider : IRewritingExpressionVisitorProvider
{
    private readonly ExpressionTranslatabilityAnalyzer translatabilityAnalyzer;
    private readonly ITypeMappingProvider typeMappingProvider;

    public DefaultRewritingExpressionVisitorProvider(
        ExpressionTranslatabilityAnalyzer translatabilityAnalyzer,
        ITypeMappingProvider typeMappingProvider)
    {
        this.translatabilityAnalyzer
            = translatabilityAnalyzer
            ?? throw new ArgumentNullException(nameof(translatabilityAnalyzer));

        this.typeMappingProvider
            = typeMappingProvider
            ?? throw new ArgumentNullException(nameof(typeMappingProvider));
    }

    public virtual IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
    {
        // this one might be better place with the 'normalizing' visitors

        yield return new EnumerableQueryEqualityRewritingExpressionVisitor();

        // this one may need to be special-cased 

        yield return new KeyEqualityRewritingExpressionVisitor(context.DescriptorSet, context.ParameterMapping.Values);

        // this one is kind of an odd duck, it might be special-cased

        yield return new TypeBinaryExpressionRewritingExpressionVisitor();

        // these should be rewritten as member/method translators instead of complete visitors

        yield return new DateOnlyMemberRewritingExpressionVisitor();

        yield return new DateTimeMemberRewritingExpressionVisitor();

        yield return new StringMemberRewritingExpressionVisitor();

        yield return new EnumerableContainsRewritingExpressionVisitor();
    }
}
