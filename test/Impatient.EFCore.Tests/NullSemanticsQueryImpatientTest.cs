using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class NullSemanticsQueryImpatientTest : NullSemanticsQueryTestBase<ImpatientTestStore, NullSemanticsQueryImpatientFixture>
    {
        public NullSemanticsQueryImpatientTest(NullSemanticsQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void From_sql_composed_with_relational_null_comparison()
        {
            base.From_sql_composed_with_relational_null_comparison();
        }
    }

    public class NullSemanticsQueryImpatientFixture : NullSemanticsQueryRelationalFixture<ImpatientTestStore>
    {
        private const string connectionString = @"Server=.\sqlexpress; Database=efcore-impatient-nullsemantics; Trusted_Connection=true; MultipleActiveResultSets=True";

        private readonly DbContextOptions options;

        public NullSemanticsQueryImpatientFixture()
        {
            var services = new ServiceCollection();

            new ImpatientDbContextOptionsExtension().ApplyServices(services);

            var provider
                = services
                    .AddEntityFrameworkSqlServer()
                    .AddImpatientEFCoreQueryCompiler()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

            options
                = new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(provider)
                    .Options;

            using (var context = new NullSemanticsContext(
                new DbContextOptionsBuilder(options).UseSqlServer(connectionString).Options))
            {
                // context.Database.EnsureDeleted();
                if (context.Database.EnsureCreated())
                {
                    NullSemanticsModelInitializer.Seed(context);
                }
            }
        }

        public override NullSemanticsContext CreateContext(ImpatientTestStore testStore, bool flag)
        {
            var options
                = new DbContextOptionsBuilder(this.options)
                    .UseSqlServer(testStore.Connection, sql =>
                    {
                        sql.UseRelationalNulls(flag);
                    })
                    .Options;

            var context = new NullSemanticsContext(options);

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }

        public override ImpatientTestStore CreateTestStore()
        {
            return new ImpatientTestStore(connectionString);
        }
    }
}
