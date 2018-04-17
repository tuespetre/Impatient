using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure
{
    public class EntityMaterializationInfo
    {
        public object Entity;
        public object[] KeyValues;
        public object[] ShadowPropertyValues;

        public IEntityType EntityType;
        public IKey Key;
        public IProperty[] ShadowProperties;
        public List<List<INavigation>> Includes;
    }
}
