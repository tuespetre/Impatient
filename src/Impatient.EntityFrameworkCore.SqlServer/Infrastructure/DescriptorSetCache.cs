using Impatient.Metadata;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class DescriptorSetCache
    {
        private readonly ModelExpressionProvider modelExpressionProvider;
        private DescriptorSet cachedDescriptorSet;

        public DescriptorSetCache(ModelExpressionProvider modelExpressionProvider)
        {
            this.modelExpressionProvider = modelExpressionProvider ?? throw new System.ArgumentNullException(nameof(modelExpressionProvider));
        }

        public DescriptorSet GetDescriptorSet(DbContext context)
        {
            // TODO: This is probably broken as hell, doesn't consider 
            // that a different model could be passed in at any time.

            // TODO: Parameterize all DbContext references in query filters upfront.

            if (cachedDescriptorSet == null)
            {
                cachedDescriptorSet = new DescriptorSet(
                    modelExpressionProvider.CreatePrimaryKeyDescriptors(context).ToArray(),
                    modelExpressionProvider.CreateNavigationDescriptors(context).ToArray());
            }

            return cachedDescriptorSet;
        }
    }
}
