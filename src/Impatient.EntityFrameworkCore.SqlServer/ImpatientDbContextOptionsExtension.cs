using Impatient.Query;
using Impatient.Query.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class ImpatientDbContextOptionsExtension : IDbContextOptionsExtension
    {
        public string LogFragment => string.Empty;

        public bool ApplyServices(IServiceCollection services)
        {
            // TODO: Document all of the 'core services' that Impatient needs to operate

            services.AddSingleton<IImpatientQueryCache, DefaultImpatientQueryCache>();

            services.AddSingleton<TranslatabilityAnalyzingExpressionVisitor>();

            services.AddSingleton<IOptimizingExpressionVisitorProvider, DefaultOptimizingExpressionVisitorProvider>();

            services.AddSingleton<IComposingExpressionVisitorProvider, EFCoreComposingExpressionVisitorProvider>();

            services.AddSingleton<IRewritingExpressionVisitorProvider, DefaultRewritingExpressionVisitorProvider>();

            services.AddSingleton<ICompilingExpressionVisitorProvider, EFCoreCompilingExpressionVisitorProvider>();

            services.AddSingleton<IQueryTranslatingExpressionVisitorFactory, DefaultQueryTranslatingExpressionVisitorFactory>();
            
            services.AddScoped<IDbCommandExecutor, EFCoreDbCommandExecutor>();
            
            services.AddSingleton<DescriptorSetCache>();

            return false;
        }

        public long GetServiceProviderHashCode() => 0;

        public void Validate(IDbContextOptions options)
        {
        }
    }
}
