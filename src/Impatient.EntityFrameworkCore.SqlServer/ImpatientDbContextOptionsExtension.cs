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

        public ImpatientCompatibility Compatibility { get; }

        public DbContextOptionsExtensionInfo Info => new ImpatientDbContextOptionsExtensionInfo(this);

        public ImpatientDbContextOptionsExtension WithCompatibility(ImpatientCompatibility compatibility)
        {
            return new ImpatientDbContextOptionsExtension(this)
            {
                compatibility = compatibility
            };
        }

        void IDbContextOptionsExtension.ApplyServices(IServiceCollection services)
        {
            services.AddImpatientEFCoreQueryCompiler(compatibility);
        }

        public void Validate(IDbContextOptions options)
        {
            // TODO: ???
        }
    }
}
