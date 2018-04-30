using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure
{
    public struct EntityMaterializationInfo
    {
        public object Entity;
        public object[] KeyValues;
        public object[] ShadowPropertyValues;
        public IEntityType EntityType;
        public IKey Key;
        public HashSet<INavigation> Includes;
    }
}
