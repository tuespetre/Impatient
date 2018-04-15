using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class ComplexNavigationsOwnedQueryImpatientTest : ComplexNavigationsOwnedQueryTestBase<ImpatientTestStore, ComplexNavigationsOwnedQueryImpatientFixture>
    {
        public ComplexNavigationsOwnedQueryImpatientTest(ComplexNavigationsOwnedQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override void GroupJoin_reference_to_group_in_OrderBy()
        {
            base.GroupJoin_reference_to_group_in_OrderBy();
        }
    }

    public class ComplexNavigationsOwnedQueryImpatientFixture : ComplexNavigationsOwnedQueryRelationalFixtureBase<ImpatientTestStore>
    {
        private readonly DbContextOptions options;

        public ComplexNavigationsOwnedQueryImpatientFixture()
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
                    .UseSqlServer(@"Server=.\sqlexpress; Database=efcore-impatient-complex-navigations-owned; Trusted_Connection=true; MultipleActiveResultSets=True")
                    .Options;

            using (var context = new ComplexNavigationsContext(options))
            {
                // context.Database.EnsureDeleted();
                if (context.Database.EnsureCreated())
                {
                    ComplexNavigationsModelInitializer.Seed(context, tableSplitting: true);
                }
            }
        }

        public override ComplexNavigationsContext CreateContext(ImpatientTestStore testStore)
        {
            var context = new ComplexNavigationsContext(options);

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }

        public override ImpatientTestStore CreateTestStore()
        {
            return new ImpatientTestStore();
        }
    }
}
