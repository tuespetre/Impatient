using Impatient.Query.ExpressionVisitors.Generating;

namespace Impatient.Query.Infrastructure
{
    public class DefaultQueryTranslatingExpressionVisitorFactory : IQueryTranslatingExpressionVisitorFactory
    {
        private readonly ITypeMappingProvider typeMappingProvider;

        public DefaultQueryTranslatingExpressionVisitorFactory(
            ITypeMappingProvider typeMappingProvider)
        {
            this.typeMappingProvider = typeMappingProvider;
        }

        public QueryTranslatingExpressionVisitor Create()
        {
            return new QueryTranslatingExpressionVisitor(
                new DefaultDbCommandExpressionBuilder(typeMappingProvider), 
                typeMappingProvider,
                new SqlServerForJsonComplexTypeSubqueryFormatter());
        }
    }
}
