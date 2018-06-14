using Impatient.Metadata;
using System;
using System.Linq;

namespace Impatient.Query.Infrastructure
{
    public class DefaultQueryProcessingContextFactory : IQueryProcessingContextFactory
    {
        private readonly DescriptorSet descriptorSet;
        private readonly ImpatientCompatibility compatibility;

        public DefaultQueryProcessingContextFactory(DescriptorSet descriptorSet) 
            : this(descriptorSet, ImpatientCompatibility.Default)
        {
        }

        public DefaultQueryProcessingContextFactory(
            DescriptorSet descriptorSet,
            ImpatientCompatibility compatibility)
        {
            this.descriptorSet = descriptorSet ?? throw new ArgumentNullException(nameof(descriptorSet));
            this.compatibility = compatibility;
        }

        public QueryProcessingContext CreateQueryProcessingContext(IQueryProvider queryProvider)
        {
            if (queryProvider == null)
            {
                throw new ArgumentNullException(nameof(queryProvider));
            }

            return new QueryProcessingContext(queryProvider, descriptorSet, compatibility);
        }
    }
}
