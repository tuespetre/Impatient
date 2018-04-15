using System;
using System.Collections.Generic;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure
{
    public class MaterializerAccessorInfo
    {
        public Type Type;
        public Func<object, object> GetValue;
        public Action<object, object> SetValue;
        public IList<MaterializerAccessorInfo> SubAccessors;
    }
}
