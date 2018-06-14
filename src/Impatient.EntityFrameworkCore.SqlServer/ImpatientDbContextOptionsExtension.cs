using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class ImpatientDbContextOptionsExtension : IDbContextOptionsExtension
    {
        private ImpatientCompatibility compatibility;

        public ImpatientDbContextOptionsExtension()
        {
            compatibility = ImpatientCompatibility.Default;
        }

        protected ImpatientDbContextOptionsExtension(ImpatientDbContextOptionsExtension template)
        {
            compatibility = template.compatibility;
        }

        public string LogFragment => string.Empty;

        public ImpatientCompatibility Compatibility { get; }

        public ImpatientDbContextOptionsExtension WithCompatibility(ImpatientCompatibility compatibility)
        {
            return new ImpatientDbContextOptionsExtension(this)
            {
                compatibility = compatibility
            };
        }

        public bool ApplyServices(IServiceCollection services)
        {
            services.AddImpatientEFCoreQueryCompiler(compatibility);

            return false;
        }

        public long GetServiceProviderHashCode() => (compatibility).GetHashCode();

        public void Validate(IDbContextOptions options)
        {
        }
    }
}
