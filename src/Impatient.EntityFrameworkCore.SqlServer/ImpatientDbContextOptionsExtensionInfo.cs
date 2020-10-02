using Microsoft.EntityFrameworkCore.Infrastructure;
using System.Collections.Generic;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class ImpatientDbContextOptionsExtensionInfo : DbContextOptionsExtensionInfo
    {
        public ImpatientDbContextOptionsExtensionInfo(ImpatientDbContextOptionsExtension extension) : base(extension)
        {
        }

        public override bool IsDatabaseProvider => true;

        public override string LogFragment => string.Empty;

        public override long GetServiceProviderHashCode() =>
            (((ImpatientDbContextOptionsExtension)Extension).Compatibility,
            typeof(ImpatientDbContextOptionsExtension)).GetHashCode();

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
        }
    }
}
