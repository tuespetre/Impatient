using Impatient.Metadata;
using Impatient.Query.ExpressionVisitors.Utility;
using Impatient.Query.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Impatient.Tests.Utilities
{
    public static class ExtensionMethods
    {
        public static IServiceProvider CreateServiceProvider(
            DescriptorSet descriptorSet = null, 
            string connectionString = null,
            ImpatientCompatibility compatibility = ImpatientCompatibility.Default)
        {
            var services = new ServiceCollection();

            #region services that would typically be DI-handled

            services.AddSingleton<IImpatientQueryCache, DefaultImpatientQueryCache>();

            services.AddSingleton<TranslatabilityAnalyzingExpressionVisitor>();

            services.AddSingleton<ITypeMappingProvider, DefaultTypeMappingProvider>();

            services.AddSingleton<IQueryFormattingProvider, SqlServerQueryFormattingProvider>();

            services.AddScoped<IReadValueExpressionFactoryProvider, DefaultReadValueExpressionFactoryProvider>();

            services.AddScoped<IRewritingExpressionVisitorProvider, DefaultRewritingExpressionVisitorProvider>();

            services.AddScoped<IProviderSpecificRewritingExpressionVisitorProvider, SqlServerRewritingExpressionVisitorProvider>();

            services.AddScoped<IOptimizingExpressionVisitorProvider, DefaultOptimizingExpressionVisitorProvider>();

            services.AddScoped<IComposingExpressionVisitorProvider, DefaultComposingExpressionVisitorProvider>();

            services.AddScoped<ICompilingExpressionVisitorProvider, DefaultCompilingExpressionVisitorProvider>();

            services.AddScoped<IQueryableInliningExpressionVisitorFactory, DefaultQueryInliningExpressionVisitorFactory>();

            services.AddScoped<IQueryTranslatingExpressionVisitorFactory, DefaultQueryTranslatingExpressionVisitorFactory>();

            services.AddScoped<IQueryProcessingContextFactory>(provider =>
            {
                return new DefaultQueryProcessingContextFactory(
                    provider.GetRequiredService<DescriptorSet>(),
                    compatibility);
            });

            services.AddScoped<IDbCommandExecutorFactory>(provider => provider.GetRequiredService<TestDbCommandExecutorFactory>());

            services.AddScoped<IImpatientQueryProcessor, DefaultImpatientQueryProcessor>();

            #endregion

            services.AddSingleton(descriptorSet ?? DescriptorSet.Empty);

            services.AddSingleton<ImpatientQueryProvider>();

            services.AddScoped<NorthwindQueryContext>();

            services.AddScoped(provider => new TestDbCommandExecutorFactory(connectionString));

            return services.BuildServiceProvider();
        }
    }
}
