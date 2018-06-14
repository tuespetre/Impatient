using Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure
{
    public class EFCoreRewritingExpressionVisitorProvider : DefaultRewritingExpressionVisitorProvider
    {
        private readonly ICurrentDbContext currentDbContext;

        public EFCoreRewritingExpressionVisitorProvider(
            ICurrentDbContext currentDbContext,
            TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor,
            ITypeMappingProvider typeMappingProvider)
            : base(translatabilityAnalyzingExpressionVisitor, typeMappingProvider)
        {
            this.currentDbContext = currentDbContext ?? throw new System.ArgumentNullException(nameof(currentDbContext));
        }

        public override IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
        {
            yield return new IncludeRewritingExpressionVisitor();

            yield return new ShadowPropertyPushdownExpressionVisitor();

            yield return new ShadowPropertyRewritingExpressionVisitor(currentDbContext.Context.Model);

            yield return new DbFunctionRewritingExpressionVisitor(currentDbContext.Context.Model);

            foreach (var visitor in base.CreateExpressionVisitors(context))
            {
                yield return visitor;
            }
        }
    }
}
