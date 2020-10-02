using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace Microsoft.EntityFrameworkCore
{
    public static class DbContextOptionsBuilderExtensions
    {
        [Obsolete("Use \"UseImpatient\" instead.", error: false)]
        public static DbContextOptionsBuilder UseImpatientQueryCompiler(
            this DbContextOptionsBuilder builder)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var extension = GetOrCreateExtension(builder);
            
            ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(extension);

            return builder;
        }

        public static DbContextOptionsBuilder UseImpatient(
            this DbContextOptionsBuilder builder,
            Action<ImpatientDbContextOptionsBuilder> configure = null)
        {
            if (builder is null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var extension = GetOrCreateExtension(builder);

            ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(extension);

            configure?.Invoke(new ImpatientDbContextOptionsBuilder(builder));

            return builder;
        }

        private static ImpatientDbContextOptionsExtension GetOrCreateExtension(
            DbContextOptionsBuilder builder)
        {
            return builder.Options.FindExtension<ImpatientDbContextOptionsExtension>()
                ?? new ImpatientDbContextOptionsExtension();
        }
    }
}
