using Impatient.Metadata;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Linq;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure
{
    public class EFCoreQueryProcessingContextFactory : IQueryProcessingContextFactory
    {
        private readonly ICurrentDbContext currentDbContext;
        private readonly DescriptorSet descriptorSet;
        private readonly ImpatientCompatibility compatibility;

        public EFCoreQueryProcessingContextFactory(
            ICurrentDbContext currentDbContext, 
            DescriptorSet descriptorSet,
            ImpatientCompatibility compatibility)
        {
            this.currentDbContext = currentDbContext ?? throw new ArgumentNullException(nameof(currentDbContext));
            this.descriptorSet = descriptorSet ?? throw new ArgumentNullException(nameof(descriptorSet));
            this.compatibility = compatibility;
        }

        public QueryProcessingContext CreateQueryProcessingContext(IQueryProvider queryProvider)
        {
            if (queryProvider is null)
            {
                throw new ArgumentNullException(nameof(queryProvider));
            }

            var processingContext = new QueryProcessingContext(queryProvider, descriptorSet, compatibility);

            processingContext.ParameterMapping.Add(
                currentDbContext.Context,
                DbContextParameter.GetInstance(currentDbContext.Context.GetType()));

            return processingContext;
        }
    }
}
