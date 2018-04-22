using Impatient.Metadata;
using System.Linq;

namespace Impatient.Query.Infrastructure
{
    public class DefaultQueryProcessingContextFactory : IQueryProcessingContextFactory
    {
        public QueryProcessingContext CreateQueryProcessingContext(
            IQueryProvider queryProvider, 
            DescriptorSet descriptorSet)
        {
            return new QueryProcessingContext(queryProvider, descriptorSet);
        }
    }
}
