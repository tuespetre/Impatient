using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System;

namespace Impatient.EntityFrameworkCore.SqlServer.Infrastructure
{
    public class EFCoreTypeMappingProvider : ITypeMappingProvider
    {
        private readonly IRelationalTypeMappingSource source;

        public EFCoreTypeMappingProvider(IRelationalTypeMappingSource source)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
        }

        public ITypeMapping FindMapping(Type clrType)
        {
            var sourceMapping = source.FindMapping(clrType);

            if (sourceMapping is RelationalTypeMapping relationalTypeMapping)
            {
                return new AdHocTypeMapping(
                    clrType,
                    relationalTypeMapping.Converter?.ProviderClrType ?? clrType,
                    relationalTypeMapping.DbType,
                    relationalTypeMapping.StoreType,
                    relationalTypeMapping.Converter?.ConvertFromProviderExpression,
                    relationalTypeMapping.Converter?.ConvertToProviderExpression);
            }

            return default;
        }
    }
}
