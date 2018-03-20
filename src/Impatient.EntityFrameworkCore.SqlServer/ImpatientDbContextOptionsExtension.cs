using Impatient.Query;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
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

            services.AddSingleton<DescriptorSetCache>();

            services.AddScoped<TranslatabilityAnalyzingExpressionVisitor>();

            services.AddScoped<IOptimizingExpressionVisitorProvider, DefaultOptimizingExpressionVisitorProvider>();

            services.AddScoped<IComposingExpressionVisitorProvider, EFCoreComposingExpressionVisitorProvider>();

            services.AddScoped<IRewritingExpressionVisitorProvider, DefaultRewritingExpressionVisitorProvider>();

            services.AddScoped<ICompilingExpressionVisitorProvider, EFCoreCompilingExpressionVisitorProvider>();

            services.AddScoped<IQueryTranslatingExpressionVisitorFactory, DefaultQueryTranslatingExpressionVisitorFactory>();

            services.AddScoped<IQueryableInliningExpressionVisitorFactory, EFCoreQueryableInliningExpressionVisitorFactory>();

            services.AddScoped<IDbCommandExecutor, EFCoreDbCommandExecutor>();

            services.AddScoped<IImpatientQueryExecutor, DefaultImpatientQueryExecutor>();

            services.AddScoped(provider =>
            {
                var cache = provider.GetRequiredService<DescriptorSetCache>();
                var model = provider.GetRequiredService<ICurrentDbContext>().Context.Model;

                return cache.GetDescriptorSet(model);
            });

            return false;
        }

        public long GetServiceProviderHashCode() => 0;

        public void Validate(IDbContextOptions options)
        {
        }
    }
}
