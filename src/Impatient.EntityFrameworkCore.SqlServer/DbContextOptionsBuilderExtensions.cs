using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static DbContextOptionsBuilder UseImpatientQueryCompiler(this DbContextOptionsBuilder builder)
        {
            ((IDbContextOptionsBuilderInfrastructure)builder)
                .AddOrUpdateExtension(new ImpatientDbContextOptionsExtension());

            return builder;
        }
    }
}
