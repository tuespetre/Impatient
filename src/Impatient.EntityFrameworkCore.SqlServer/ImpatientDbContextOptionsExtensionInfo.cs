using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class ImpatientDbContextOptionsExtensionInfo(ImpatientDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        public override bool IsDatabaseProvider => true;

        public override string LogFragment => string.Empty;

        public override int GetServiceProviderHashCode() =>
            (((ImpatientDbContextOptionsExtension)Extension).Compatibility,
            typeof(ImpatientDbContextOptionsExtension)).GetHashCode();

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
        }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            return other is ImpatientDbContextOptionsExtensionInfo info
                && GetServiceProviderHashCode() == info.GetServiceProviderHashCode();
        }
    }
}
