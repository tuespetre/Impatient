using Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
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
            foreach (var visitor in base.CreateExpressionVisitors(context))
            {
                yield return visitor;
            }

            yield return new ShadowPropertyCompilingExpressionVisitor(
                currentDbContext.Context.Model,
                context.ExecutionContextParameter);

            yield return new EntityMaterializationCompilingExpressionVisitor(
                currentDbContext.Context.Model,
                context.ExecutionContextParameter);
        }
    }
}