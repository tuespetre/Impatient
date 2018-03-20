using Impatient.Metadata;
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

        public DefaultCompilingExpressionVisitorProvider(
            TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor,
            IQueryTranslatingExpressionVisitorFactory queryTranslatingExpressionVisitorFactory)
        {
            this.translatabilityAnalyzingExpressionVisitor = translatabilityAnalyzingExpressionVisitor;
            this.queryTranslatingExpressionVisitorFactory = queryTranslatingExpressionVisitorFactory;
        }

        public virtual IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
        {
            yield return new QueryCompilingExpressionVisitor(
                translatabilityAnalyzingExpressionVisitor,
                queryTranslatingExpressionVisitorFactory,
                context.ExecutionContextParameter);
        }
    }
}
