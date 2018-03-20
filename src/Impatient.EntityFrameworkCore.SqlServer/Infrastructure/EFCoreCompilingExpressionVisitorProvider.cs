using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class EFCoreCompilingExpressionVisitorProvider : DefaultCompilingExpressionVisitorProvider
    {
        public EFCoreCompilingExpressionVisitorProvider(
            TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor, 
            IQueryTranslatingExpressionVisitorFactory queryTranslatingExpressionVisitorFactory) 
            : base(translatabilityAnalyzingExpressionVisitor, 
                  queryTranslatingExpressionVisitorFactory)
        {
        }

        public override IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
        {
            foreach (var visitor in base.CreateExpressionVisitors(context))
            {
                yield return visitor;
            }

            yield return new ChangeTrackerInjectingExpressionVisitor(context.ExecutionContextParameter);
        }
    }
}