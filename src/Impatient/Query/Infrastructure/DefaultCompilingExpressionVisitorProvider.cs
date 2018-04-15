using Impatient.Query.ExpressionVisitors.Generating;
using Impatient.Query.ExpressionVisitors.Utility;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public class DefaultCompilingExpressionVisitorProvider : ICompilingExpressionVisitorProvider
    {
        private readonly TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor;
        private readonly IQueryTranslatingExpressionVisitorFactory queryTranslatingExpressionVisitorFactory;
        private readonly IReadValueExpressionFactoryProvider readValueExpressionFactoryProvider;

        public DefaultCompilingExpressionVisitorProvider(
            TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor,
            IQueryTranslatingExpressionVisitorFactory queryTranslatingExpressionVisitorFactory,
            IReadValueExpressionFactoryProvider readValueExpressionFactoryProvider)
        {
            this.translatabilityAnalyzingExpressionVisitor = translatabilityAnalyzingExpressionVisitor;
            this.queryTranslatingExpressionVisitorFactory = queryTranslatingExpressionVisitorFactory;
            this.readValueExpressionFactoryProvider = readValueExpressionFactoryProvider;
        }

        public virtual IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
        {
            yield return new QueryCompilingExpressionVisitor(
                translatabilityAnalyzingExpressionVisitor,
                queryTranslatingExpressionVisitorFactory,
                new MaterializerGeneratingExpressionVisitor(
                    translatabilityAnalyzingExpressionVisitor,
                    readValueExpressionFactoryProvider),
                context.ExecutionContextParameter);
        }
    }
}
