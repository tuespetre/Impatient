using Impatient.Metadata;
using Microsoft.EntityFrameworkCore.Metadata;

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
                    ModelHelper.CreatePrimaryKeyDescriptors(model),
                    ModelHelper.CreateNavigationDescriptors(model));
            }

            return cachedDescriptorSet;
        }
    }
}
