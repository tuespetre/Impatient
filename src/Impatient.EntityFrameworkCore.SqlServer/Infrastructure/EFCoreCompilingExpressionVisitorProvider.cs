using Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class EFCoreCompilingExpressionVisitorProvider : DefaultCompilingExpressionVisitorProvider
    {
        private readonly ICurrentDbContext currentDbContext;

        public EFCoreCompilingExpressionVisitorProvider(
            ICurrentDbContext currentDbContext,
            TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor, 
            IQueryTranslatingExpressionVisitorFactory queryTranslatingExpressionVisitorFactory,
            IReadValueExpressionFactoryProvider readValueExpressionFactoryProvider) 
            : base(translatabilityAnalyzingExpressionVisitor, 
                  queryTranslatingExpressionVisitorFactory,
                  readValueExpressionFactoryProvider)
        {
            this.currentDbContext = currentDbContext;
        }

        public override IEnumerable<ExpressionVisitor> CreateExpressionVisitors(QueryProcessingContext context)
        {
            var model = currentDbContext.Context.Model;

            // Deal with change tracking before we muck up the materializers

            yield return new ResultTrackingCompilingExpressionVisitor(model);

            foreach (var visitor in base.CreateExpressionVisitors(context))
            {
                yield return visitor;
            }

            yield return new ShadowPropertyCompilingExpressionVisitor(model);

            yield return new EntityMaterializationCompilingExpressionVisitor(model);

            yield return new IncludeCompilingExpressionVisitor();

            // TODO: this
            //yield return new ConcurrencyDetectionCompilingExpressionVisitor();
        }
    }
}