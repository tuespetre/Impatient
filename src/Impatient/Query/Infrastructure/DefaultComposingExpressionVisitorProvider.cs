using Impatient.Query.ExpressionVisitors.Composing;
using Impatient.Query.ExpressionVisitors.Rewriting;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure;

public class DefaultComposingExpressionVisitorProvider : IComposingExpressionVisitorProvider
{
    private readonly ExpressionTranslatabilityAnalyzer translatabilityAnalyzer;
    private readonly IRewritingExpressionVisitorProvider rewritingExpressionVisitorProvider;
    private readonly IProviderSpecificRewritingExpressionVisitorProvider providerSpecificRewritingExpressionVisitorProvider;

    public DefaultComposingExpressionVisitorProvider(
        ExpressionTranslatabilityAnalyzer translatabilityAnalyzer,
        IRewritingExpressionVisitorProvider rewritingExpressionVisitorProvider,
        IProviderSpecificRewritingExpressionVisitorProvider providerSpecificRewritingExpressionVisitorProvider)
    {
        this.translatabilityAnalyzer = translatabilityAnalyzer ?? throw new ArgumentNullException(nameof(translatabilityAnalyzer));
        this.rewritingExpressionVisitorProvider = rewritingExpressionVisitorProvider ?? throw new ArgumentNullException(nameof(rewritingExpressionVisitorProvider));
        this.providerSpecificRewritingExpressionVisitorProvider = providerSpecificRewritingExpressionVisitorProvider ?? throw new ArgumentNullException(nameof(providerSpecificRewritingExpressionVisitorProvider));
    }

    public virtual IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
    {
        yield return new KeyEqualityComposingExpressionVisitor(context.DescriptorSet, context.ParameterMapping.Values);

        yield return new NavigationComposingExpressionVisitor(context.DescriptorSet.NavigationDescriptors);
        
        yield return new TableAliasComposingExpressionVisitor();

        yield return new QueryComposingExpressionVisitor(
            translatabilityAnalyzer, 
            rewritingExpressionVisitorProvider.CreateExpressionVisitors(context),
            providerSpecificRewritingExpressionVisitorProvider.CreateExpressionVisitors(context),
            new SqlParameterRewritingExpressionVisitor(context.ParameterMapping.Values));
    }
}
