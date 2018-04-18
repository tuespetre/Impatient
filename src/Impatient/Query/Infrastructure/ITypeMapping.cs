using System;

namespace Impatient.Query.Infrastructure
{
    public interface ITypeMapping
    {
        Type ClrType { get; }

        string DbType { get; }
    }
}
