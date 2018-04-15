using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Inheritance;
using Microsoft.Extensions.DependencyInjection;

namespace Impatient.EFCore.Tests
{
    public class InheritanceImpatientFixture : InheritanceRelationalFixture<ImpatientTestStore>
    {
        private readonly DbContextOptions options;

        public InheritanceImpatientFixture()
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
                    .UseSqlServer(@"Server=.\sqlexpress; Database=efcore-impatient-inheritance; Trusted_Connection=true; MultipleActiveResultSets=True")
                    .Options;

            using (var context = new InheritanceContext(options))
            {
                //context.Database.EnsureDeleted();

                if (context.Database.EnsureCreated())
                {
                    InheritanceModelInitializer.SeedData(context);
                }
            }
        }

        public override InheritanceContext CreateContext(ImpatientTestStore testStore)
        {
            var context = new InheritanceContext(options);

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }

        public override ImpatientTestStore CreateTestStore()
        {
            return new ImpatientTestStore();
        }
    }
}
