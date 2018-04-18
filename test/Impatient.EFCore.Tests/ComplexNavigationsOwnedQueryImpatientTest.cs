using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.Extensions.DependencyInjection;
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

        [Fact(Skip = EFCoreSkipReasons.NullNavigationProtection)]
        public override void Nested_group_join_with_take()
        {
            base.Nested_group_join_with_take();
        }

        [Fact(Skip = EFCoreSkipReasons.NullNavigationProtection)]
        public override void Null_reference_protection_complex_client_eval()
        {
            base.Null_reference_protection_complex_client_eval();
        }

        [Fact, Trait("Skipped by EFCore", "Unskipped by us")]
        public override void Required_navigation_on_a_subquery_with_First_in_projection()
        {
            base.Required_navigation_on_a_subquery_with_First_in_projection();
        }

        [Fact, Trait("Skipped by EFCore", "Unskipped by us")]
        public override void Select_subquery_with_client_eval_and_navigation1()
        {
            base.Select_subquery_with_client_eval_and_navigation1();
        }

        [Fact, Trait("Skipped by EFCore", "Unskipped by us")]
        public override void Select_subquery_with_client_eval_and_navigation2()
        {
            base.Select_subquery_with_client_eval_and_navigation2();
        }
    }

    public class ComplexNavigationsOwnedQueryImpatientFixture : ComplexNavigationsOwnedQueryRelationalFixtureBase<ImpatientTestStore>
    {
        private const string connectionString = @"Server=.\sqlexpress; Database=efcore-impatient-complex-navigations-owned; Trusted_Connection=true; MultipleActiveResultSets=True";

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
                    .Options;

            using (var context = new ComplexNavigationsContext(
                new DbContextOptionsBuilder(options).UseSqlServer(connectionString).Options))
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
            var context 
                = new ComplexNavigationsContext(
                    new DbContextOptionsBuilder(options)
                        .UseSqlServer(testStore.Connection).Options);

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }

        public override ImpatientTestStore CreateTestStore()
        {
            return new ImpatientTestStore(connectionString);
        }
    }
}
