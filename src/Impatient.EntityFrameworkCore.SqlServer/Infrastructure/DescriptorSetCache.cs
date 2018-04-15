using Impatient.Metadata;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class DescriptorSetCache
    {
        private DescriptorSet cachedDescriptorSet;

        public DescriptorSet GetDescriptorSet(IModel model)
        {
            if (cachedDescriptorSet == null)
            {
                cachedDescriptorSet = new DescriptorSet(
                    ModelHelper.CreatePrimaryKeyDescriptors(model).ToArray(),
                    ModelHelper.CreateNavigationDescriptors(model).ToArray());
            }

            return cachedDescriptorSet;
        }
    }
}
