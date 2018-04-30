using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public static class ImpatientEFCoreServiceCollectionExtensions
    {
        public static IServiceCollection AddImpatientEFCoreQueryCompiler(this IServiceCollection services)
        {
            foreach (var descriptor in services.Where(s => s.ServiceType == typeof(IQueryCompiler)).ToArray())
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<IImpatientQueryCache, DefaultImpatientQueryCache>();

            services.AddSingleton<DescriptorSetCache>();

            services.AddSingleton<ModelQueryExpressionCache>();

            services.AddSingleton<TranslatabilityAnalyzingExpressionVisitor>();

            services.AddSingleton<IQueryTranslatingExpressionVisitorFactory, DefaultQueryTranslatingExpressionVisitorFactory>();

            services.AddSingleton<IReadValueExpressionFactoryProvider, DefaultReadValueExpressionFactoryProvider>();

            services.AddScoped<IOptimizingExpressionVisitorProvider, DefaultOptimizingExpressionVisitorProvider>();

            services.AddScoped<IComposingExpressionVisitorProvider, EFCoreComposingExpressionVisitorProvider>();

            services.AddScoped<IRewritingExpressionVisitorProvider, EFCoreRewritingExpressionVisitorProvider>();

            services.AddScoped<ICompilingExpressionVisitorProvider, EFCoreCompilingExpressionVisitorProvider>();

            services.AddScoped<IQueryableInliningExpressionVisitorFactory, EFCoreQueryableInliningExpressionVisitorFactory>();

            services.AddScoped<IDbCommandExecutorFactory, EFCoreDbCommandExecutorFactory>();

            services.AddScoped<IQueryProcessingContextFactory, EFCoreQueryProcessingContextFactory>();

            services.AddScoped<IImpatientQueryExecutor, DefaultImpatientQueryExecutor>();

            services.AddScoped<IQueryCompiler, ImpatientQueryCompiler>();

            services.AddScoped(provider =>
            {
                var cache = provider.GetRequiredService<DescriptorSetCache>();
                var model = provider.GetRequiredService<ICurrentDbContext>().Context.Model;

                return cache.GetDescriptorSet(model);
            });

            return services;
        }
    }
}
