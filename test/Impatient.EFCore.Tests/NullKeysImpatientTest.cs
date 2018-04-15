using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;

namespace Impatient.EFCore.Tests
{
    public class NullKeysImpatientTest : NullKeysTestBase<NullKeysImpatientFixture>
    {
        public NullKeysImpatientTest(NullKeysImpatientFixture fixture) : base(fixture)
        {
        }
    }

    public class NullKeysImpatientFixture : NullKeysTestBase<NullKeysImpatientFixture>.NullKeysFixtureBase
    {
        private readonly DbContextOptions options;

        public NullKeysImpatientFixture()
        {
            var services = new ServiceCollection();

            new ImpatientDbContextOptionsExtension().ApplyServices(services);

            options
                = new DbContextOptionsBuilder()
                    .UseSqlServer(@"Server=.\sqlexpress; Database=efcore-impatient-nullkeys; Trusted_Connection=true; MultipleActiveResultSets=True")
                    .UseInternalServiceProvider(services
                        .AddEntityFrameworkSqlServer()
                        .AddImpatientEFCoreQueryCompiler()
                        .AddSingleton(TestModelSource.GetFactory(base.OnModelCreating))
                        .BuildServiceProvider())
                    .Options;

            using (var context = new DbContext(options))
            {
                // context.Database.EnsureDeleted();
                if (context.Database.EnsureCreated())
                {
                    EnsureCreated();
                }
            }
        }

        public override DbContext CreateContext()
        {
            return new DbContext(options);
        }
    }
}
