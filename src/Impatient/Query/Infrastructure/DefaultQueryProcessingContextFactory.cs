using Impatient.Metadata;
using System;
using System.Linq;

namespace Impatient.Query.Infrastructure
{
    public class DefaultQueryProcessingContextFactory : IQueryProcessingContextFactory
    {
        private readonly DescriptorSet descriptorSet;

        public DefaultQueryProcessingContextFactory(DescriptorSet descriptorSet)
        {
            this.descriptorSet = descriptorSet ?? throw new ArgumentNullException(nameof(descriptorSet));
        }

        public QueryProcessingContext CreateQueryProcessingContext(IQueryProvider queryProvider)
        {
            if (queryProvider == null)
            {
                throw new ArgumentNullException(nameof(queryProvider));
            }

            return new QueryProcessingContext(queryProvider, descriptorSet);
        }
    }
}
