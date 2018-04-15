using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.Extensions.DependencyInjection;

namespace Impatient.EFCore.Tests
{
    public class GearsOfWarQueryImpatientFixture : GearsOfWarQueryFixtureBase<ImpatientTestStore>
    {
        private readonly DbContextOptions options;

        public GearsOfWarQueryImpatientFixture()
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
                    .UseSqlServer(@"Server=.\sqlexpress; Database=efcore-impatient-gears-of-war; Trusted_Connection=true; MultipleActiveResultSets=True")
                    .Options;

            using (var context = new GearsOfWarContext(options))
            {
                // context.Database.EnsureDeleted();
                if (context.Database.EnsureCreated())
                {
                    GearsOfWarModelInitializer.Seed(context);
                }
            }
        }

        public override GearsOfWarContext CreateContext(ImpatientTestStore testStore)
        {
            var context = new GearsOfWarContext(options);

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }

        public override ImpatientTestStore CreateTestStore()
        {
            return new ImpatientTestStore();
        }
    }
}
