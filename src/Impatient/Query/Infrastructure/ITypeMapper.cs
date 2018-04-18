using System;

namespace Impatient.Query.Infrastructure
{
    public interface ITypeMapper
    {
        ITypeMapping FindMapping(Type clrType);
    }
}
