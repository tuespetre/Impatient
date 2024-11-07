using System;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure;

public class MaterializerAccessorInfo
{
    public Func<object, object> GetValue;
    public Action<object, object> SetValue;
    public MaterializerAccessorInfo[] SubAccessors;
}
