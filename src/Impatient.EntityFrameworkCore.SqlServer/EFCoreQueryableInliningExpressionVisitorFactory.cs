using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    internal class EFCoreQueryableInliningExpressionVisitorFactory : IQueryableInliningExpressionVisitorFactory
    {
        private readonly IModel model;
        private readonly QueryOptions queryOptions;

        public EFCoreQueryableInliningExpressionVisitorFactory(IModel model, QueryOptions queryOptions)
        {
            this.model = model;
            this.queryOptions = queryOptions;
        }

        public QueryableInliningExpressionVisitor Create(QueryProcessingContext context)
        {
            return new EFCoreQueryableInliningExpressionVisitor(context.QueryProvider, context.ParameterMapping, model, queryOptions);
        }
    }
}
