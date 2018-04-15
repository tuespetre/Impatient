using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    internal class EFCoreQueryableInliningExpressionVisitorFactory : IQueryableInliningExpressionVisitorFactory
    {
        private readonly ICurrentDbContext currentDbContext;

        public EFCoreQueryableInliningExpressionVisitorFactory(ICurrentDbContext currentDbContext)
        {
            this.currentDbContext = currentDbContext;
        }

        public QueryableInliningExpressionVisitor Create(QueryProcessingContext context)
        {
            return new EFCoreQueryableInliningExpressionVisitor(
                context.QueryProvider,
                context.ParameterMapping,
                currentDbContext);
        }
    }
}
