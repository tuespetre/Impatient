using Impatient.Query.ExpressionVisitors.Generating;
using Impatient.Query.ExpressionVisitors.Utility;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure;

public class DefaultCompilingExpressionVisitorProvider : ICompilingExpressionVisitorProvider
{
    private readonly ExpressionTranslatabilityAnalyzer translatabilityAnalyzer;
    private readonly IQueryTranslatingExpressionVisitorFactory queryTranslatingExpressionVisitorFactory;
    private readonly IReadValueExpressionFactoryProvider readValueExpressionFactoryProvider;

    public DefaultCompilingExpressionVisitorProvider(
        ExpressionTranslatabilityAnalyzer translatabilityAnalyzer,
        IQueryTranslatingExpressionVisitorFactory queryTranslatingExpressionVisitorFactory,
        IReadValueExpressionFactoryProvider readValueExpressionFactoryProvider)
    {
        this.translatabilityAnalyzer = translatabilityAnalyzer;
        this.queryTranslatingExpressionVisitorFactory = queryTranslatingExpressionVisitorFactory;
        this.readValueExpressionFactoryProvider = readValueExpressionFactoryProvider;
    }

    public virtual IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
    {
        yield return new QueryCompilingExpressionVisitor(
            translatabilityAnalyzer,
            queryTranslatingExpressionVisitorFactory,
            new MaterializerGeneratingExpressionVisitor(
                translatabilityAnalyzer,
                readValueExpressionFactoryProvider));
    }
}
