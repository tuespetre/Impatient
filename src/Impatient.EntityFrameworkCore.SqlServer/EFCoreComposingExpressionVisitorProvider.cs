using Impatient.Metadata;
using Impatient.Query.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class EFCoreComposingExpressionVisitorProvider : DefaultComposingExpressionVisitorProvider
    {
        public EFCoreComposingExpressionVisitorProvider(
            TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor, 
            IRewritingExpressionVisitorProvider rewritingExpressionVisitorProvider) 
            : base(translatabilityAnalyzingExpressionVisitor, 
                  rewritingExpressionVisitorProvider)
        {
        }

        public override IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
        {
            yield return new IncludeComposingExpressionVisitor(context.DescriptorSet);

            foreach (var visitor in base.CreateExpressionVisitors(context))
            {
                yield return visitor;
            }
        }
    }
}
