using Impatient.Metadata;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class DescriptorSetCache
    {
        private readonly Dictionary<QueryOptions, DescriptorSet> cache = new Dictionary<QueryOptions, DescriptorSet>();

        public DescriptorSet GetDescriptorSet(IModel model, QueryOptions options)
        {
            if (!cache.TryGetValue(options, out var result))
            {
                result = cache[options] = new DescriptorSet(
                    ModelHelper.CreatePrimaryKeyDescriptors(model),
                    ModelHelper.CreateNavigationDescriptors(model, options));
            }

            return result;
        }
    }
}
