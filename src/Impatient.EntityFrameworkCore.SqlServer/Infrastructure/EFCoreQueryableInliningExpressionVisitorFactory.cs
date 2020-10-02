using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    internal class EFCoreQueryableInliningExpressionVisitorFactory : IQueryableInliningExpressionVisitorFactory
    {
        private readonly ModelExpressionProvider modelExpressionProvider;
        private readonly ModelQueryExpressionCache modelQueryExpressionCache;
        private readonly ICurrentDbContext currentDbContext;

        public EFCoreQueryableInliningExpressionVisitorFactory(
            ModelExpressionProvider modelExpressionProvider,
            ModelQueryExpressionCache modelQueryExpressionCache,
            ICurrentDbContext currentDbContext)
        {
            this.modelExpressionProvider = modelExpressionProvider ?? throw new System.ArgumentNullException(nameof(modelExpressionProvider));
            this.modelQueryExpressionCache = modelQueryExpressionCache ?? throw new System.ArgumentNullException(nameof(modelQueryExpressionCache));
            this.currentDbContext = currentDbContext ?? throw new System.ArgumentNullException(nameof(currentDbContext));
        }

        public QueryableInliningExpressionVisitor Create(QueryProcessingContext context)
        {
            return new EFCoreQueryableInliningExpressionVisitor(
                context.QueryProvider,
                context.ParameterMapping,
                modelExpressionProvider,
                modelQueryExpressionCache,
                currentDbContext);
        }
    }
}
