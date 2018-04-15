using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;
using Microsoft.Extensions.DependencyInjection;

namespace Impatient.EFCore.Tests
{
    // These tests take too damn long to run.
    /*public class FunkyDataQueryImpatientTest : FunkyDataQueryTestBase<ImpatientTestStore, FunkyDataQueryImpatientFixture>
    {
        public FunkyDataQueryImpatientTest(FunkyDataQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }*/

    public class FunkyDataQueryImpatientFixture : FunkyDataQueryFixtureBase<ImpatientTestStore>
    {
        private readonly DbContextOptions options;

        public FunkyDataQueryImpatientFixture()
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
                    .UseSqlServer(@"Server=.\sqlexpress; Database=efcore-impatient-funkydata; Trusted_Connection=true; MultipleActiveResultSets=True")
                    .Options;

            using (var context = new FunkyDataContext(options))
            {
                context.Database.EnsureCreated();

                FunkyDataModelInitializer.Seed(context);
            }
        }

        public override FunkyDataContext CreateContext(ImpatientTestStore testStore)
        {
            var context = new FunkyDataContext(options);

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }

        public override ImpatientTestStore CreateTestStore()
        {
            return new ImpatientTestStore();
        }
    }
}
