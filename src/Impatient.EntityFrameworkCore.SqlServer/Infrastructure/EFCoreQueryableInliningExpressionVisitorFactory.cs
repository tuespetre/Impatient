using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    internal class EFCoreQueryableInliningExpressionVisitorFactory : IQueryableInliningExpressionVisitorFactory
    {
        private readonly ICurrentDbContext currentDbContext;
        private readonly ModelQueryExpressionCache modelQueryExpressionCache;

        public EFCoreQueryableInliningExpressionVisitorFactory(
            ICurrentDbContext currentDbContext,
            ModelQueryExpressionCache modelQueryExpressionCache)
        {
            this.currentDbContext = currentDbContext;
            this.modelQueryExpressionCache = modelQueryExpressionCache;
        }

        public QueryableInliningExpressionVisitor Create(QueryProcessingContext context)
        {
            return new EFCoreQueryableInliningExpressionVisitor(
                context.QueryProvider,
                context.ParameterMapping,
                currentDbContext,
                modelQueryExpressionCache);
        }
    }
}
