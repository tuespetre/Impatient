using Impatient.Query.ExpressionVisitors.Generating;

namespace Impatient.Query.Infrastructure
{
    public class DefaultQueryTranslatingExpressionVisitorFactory : IQueryTranslatingExpressionVisitorFactory
    {
        private readonly ITypeMappingProvider typeMappingProvider;
        private readonly IQueryFormattingProvider queryFormattingProvider;

        public DefaultQueryTranslatingExpressionVisitorFactory(
            ITypeMappingProvider typeMappingProvider,
            IQueryFormattingProvider queryFormattingProvider)
        {
            this.typeMappingProvider = typeMappingProvider ?? throw new System.ArgumentNullException(nameof(typeMappingProvider));
            this.queryFormattingProvider = queryFormattingProvider ?? throw new System.ArgumentNullException(nameof(queryFormattingProvider));
        }

        public QueryTranslatingExpressionVisitor Create()
        {
            return new QueryTranslatingExpressionVisitor(
                new DefaultDbCommandExpressionBuilder(typeMappingProvider, queryFormattingProvider), 
                typeMappingProvider,
                queryFormattingProvider);
        }
    }
}
