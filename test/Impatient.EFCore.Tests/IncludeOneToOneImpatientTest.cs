using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class IncludeOneToOneImpatientTest : IncludeOneToOneTestBase, IClassFixture<OneToOneQueryImpatientFixture>
    {
        private readonly OneToOneQueryImpatientFixture fixture;

        public IncludeOneToOneImpatientTest(OneToOneQueryImpatientFixture fixture)
        {
            this.fixture = fixture;
        }

        protected override DbContext CreateContext()
        {
            return new DbContext(fixture.Options);
        }
    }

    public class OneToOneQueryImpatientFixture : OneToOneQueryFixtureBase
    {
        public DbContextOptions Options { get; }

        public OneToOneQueryImpatientFixture()
        {
            var services = new ServiceCollection();

            new ImpatientDbContextOptionsExtension().ApplyServices(services);

            Options
                = new DbContextOptionsBuilder()
                    .UseSqlServer(@"Server=.\sqlexpress; Database=efcore-impatient-onetoone; Trusted_Connection=true; MultipleActiveResultSets=True")
                    .UseInternalServiceProvider(services
                        .AddEntityFrameworkSqlServer()
                        .AddImpatientEFCoreQueryCompiler()
                        .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                        .BuildServiceProvider())
                    .Options;

            using (var context = new DbContext(Options))
            {
                // context.Database.EnsureDeleted();
                if (context.Database.EnsureCreated())
                {
                    AddTestData(context);
                }
            }
        }
    }
}
