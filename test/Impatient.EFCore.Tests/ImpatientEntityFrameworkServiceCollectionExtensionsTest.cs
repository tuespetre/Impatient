using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using System.Data.SqlClient;

namespace Impatient.EFCore.Tests
{
    public class ImpatientEntityFrameworkServiceCollectionExtensionsTest : RelationalServiceCollectionExtensionsTestBase
    {
        public ImpatientEntityFrameworkServiceCollectionExtensionsTest() 
            : base(ImpatientTestHelpers.Instance)
        {
        }
    }

    public class ImpatientTestHelpers : TestHelpers
    {
        protected ImpatientTestHelpers()
        {
        }

        public static ImpatientTestHelpers Instance { get; } = new ImpatientTestHelpers();

        public override IServiceCollection AddProviderServices(IServiceCollection services)
            => services.AddEntityFrameworkSqlServer();

        protected override void UseProviderOptions(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(new SqlConnection("Database=DummyDatabase"));
    }
}
