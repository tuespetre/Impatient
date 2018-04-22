using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class MappingQueryImpatientTest : MappingQueryTestBase, IClassFixture<MappingQueryImpatientFixture>
    {
        private readonly MappingQueryImpatientFixture fixture;

        public MappingQueryImpatientTest(MappingQueryImpatientFixture fixture)
        {
            this.fixture = fixture;
        }

        protected override DbContext CreateContext() => fixture.CreateContext();
    }

    public class MappingQueryImpatientFixture : MappingQueryFixtureBase
    {
        private const string connectionString = @"Server=.\sqlexpress; Database=efcore-impatient-mappingquery; Trusted_Connection=true; MultipleActiveResultSets=True";

        private readonly DbContextOptions options;

        public MappingQueryImpatientFixture()
        {
            var services = new ServiceCollection();

            new ImpatientDbContextOptionsExtension().ApplyServices(services);

            var provider
                = services
                    .AddEntityFrameworkSqlServer()
                    .AddImpatientEFCoreQueryCompiler()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

            var options
                = new DbContextOptionsBuilder()
                    .UseModel(CreateModel())
                    .UseInternalServiceProvider(provider)
                    .UseSqlServer(@"Server=.\sqlexpress; Database=NORTHWND; Trusted_Connection=true; MultipleActiveResultSets=True")
                    .Options;

            this.options = options;
        }

        public DbContext CreateContext()
        {
            var context = new DbContext(options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }

        protected override string DatabaseSchema { get; } = "dbo";

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MappingQueryTestBase.MappedCustomer>(e =>
            {
                e.Property(c => c.CompanyName2).Metadata.SqlServer().ColumnName = "CompanyName";
                e.Metadata.SqlServer().TableName = "Customers";
                e.Metadata.SqlServer().Schema = "dbo";
            });
        }
    }
}
