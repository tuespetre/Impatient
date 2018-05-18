using System;

namespace Impatient.Query.Infrastructure
{
    public interface ITypeMappingProvider
    {
        ITypeMapping FindMapping(Type clrType);
    }
}
