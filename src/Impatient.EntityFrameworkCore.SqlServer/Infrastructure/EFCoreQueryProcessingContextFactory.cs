using Impatient.Metadata;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using System.Linq;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure
{
    public class EFCoreQueryProcessingContextFactory : IQueryProcessingContextFactory
    {
        private readonly ICurrentDbContext currentDbContext;

        public EFCoreQueryProcessingContextFactory(ICurrentDbContext currentDbContext)
        {
            this.currentDbContext = currentDbContext;
        }

        public QueryProcessingContext CreateQueryProcessingContext(
            IQueryProvider queryProvider, 
            DescriptorSet descriptorSet)
        {
            var processingContext = new QueryProcessingContext(queryProvider, descriptorSet);

            processingContext.ParameterMapping.Add(
                currentDbContext.Context,
                DbContextParameter.GetInstance(currentDbContext.Context.GetType()));

            return processingContext;
        }
    }
}
