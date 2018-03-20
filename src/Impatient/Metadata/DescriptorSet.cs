using System.Collections.Generic;
using System.Linq;

namespace Impatient.Metadata
{
    public sealed class DescriptorSet
    {
        public DescriptorSet(
            IEnumerable<PrimaryKeyDescriptor> primaryKeyDescriptors,
            IEnumerable<NavigationDescriptor> navigationDescriptors)
        {
            PrimaryKeyDescriptors = primaryKeyDescriptors;
            NavigationDescriptors = navigationDescriptors;
        }

        public IEnumerable<PrimaryKeyDescriptor> PrimaryKeyDescriptors { get; }

        public IEnumerable<NavigationDescriptor> NavigationDescriptors { get; }

        public static DescriptorSet Empty { get; }
            = new DescriptorSet(
                Enumerable.Empty<PrimaryKeyDescriptor>(),
                Enumerable.Empty<NavigationDescriptor>());
    }
}
