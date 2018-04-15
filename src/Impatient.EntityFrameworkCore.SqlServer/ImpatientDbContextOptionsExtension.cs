using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    public class ImpatientDbContextOptionsExtension : IDbContextOptionsExtension
    {
        public string LogFragment => string.Empty;

        public bool ApplyServices(IServiceCollection services)
        {
            services.AddImpatientEFCoreQueryCompiler();

            return false;
        }

        public long GetServiceProviderHashCode() => 0;

        public void Validate(IDbContextOptions options)
        {
        }
    }
}
