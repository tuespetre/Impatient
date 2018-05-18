using Impatient.Metadata;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Linq;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure
{
    public class EFCoreQueryProcessingContextFactory : IQueryProcessingContextFactory
    {
        private readonly ICurrentDbContext currentDbContext;
        private readonly DescriptorSet descriptorSet;

        public EFCoreQueryProcessingContextFactory(ICurrentDbContext currentDbContext, DescriptorSet descriptorSet)
        {
            this.currentDbContext = currentDbContext ?? throw new ArgumentNullException(nameof(currentDbContext));
            this.descriptorSet = descriptorSet ?? throw new ArgumentNullException(nameof(descriptorSet));
        }

        public QueryProcessingContext CreateQueryProcessingContext(IQueryProvider queryProvider)
        {
            if (queryProvider == null)
            {
                throw new ArgumentNullException(nameof(queryProvider));
            }

            var processingContext = new QueryProcessingContext(queryProvider, descriptorSet);

            processingContext.ParameterMapping.Add(
                currentDbContext.Context,
                DbContextParameter.GetInstance(currentDbContext.Context.GetType()));

            return processingContext;
        }
    }
}
