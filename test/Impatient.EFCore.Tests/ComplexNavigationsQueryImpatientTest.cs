using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class ComplexNavigationsQueryImpatientTest : ComplexNavigationsQueryTestBase<ImpatientTestStore, ComplexNavigationsQueryImpatientFixture>
    {
        public ComplexNavigationsQueryImpatientTest(ComplexNavigationsQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override void GroupJoin_reference_to_group_in_OrderBy()
        {
            base.GroupJoin_reference_to_group_in_OrderBy();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Comparing_collection_navigation_on_optional_reference_to_null()
        {
            base.Comparing_collection_navigation_on_optional_reference_to_null();
        }

        [Fact(Skip = EFCoreSkipReasons.NullNavigationProtection)]
        public override void Null_reference_protection_complex_client_eval()
        {
            base.Null_reference_protection_complex_client_eval();
        }

        [Fact(Skip = EFCoreSkipReasons.NullNavigationProtection)]
        public override void Complex_query_with_optional_navigations_and_client_side_evaluation()
        {
            base.Complex_query_with_optional_navigations_and_client_side_evaluation();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Manually_created_left_join_propagates_nullability_to_navigations()
        {
            base.Manually_created_left_join_propagates_nullability_to_navigations();
        }

        [Fact, Trait("Skipped by EFCore", "Unskipped by us")]
        public override void GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer()
        {
            base.GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer();
        }

        [Fact(Skip = EFCoreSkipReasons.NullNavigationProtection), Trait("Skipped by EFCore", "Unskipped by us")]
        public override void GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method()
        {
            base.GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method();
        }

        [Fact, Trait("Skipped by EFCore", "Unskipped by us")]
        public override void GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_inner()
        {
            base.GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_inner();
        }

        [Fact, Trait("Skipped by EFCore", "Unskipped by us")]
        public override void Select_subquery_with_client_eval_and_multi_level_navigation()
        {
            base.Select_subquery_with_client_eval_and_multi_level_navigation();
        }

        [Fact, Trait("Skipped by EFCore", "Unskipped by us")]
        public override void Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy()
        {
            base.Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy();
        }

        [Fact, Trait("Skipped by EFCore", "Unskipped by us")]
        public override void Include_inside_subquery()
        {
            base.Include_inside_subquery();
        }
    }

    public class ComplexNavigationsQueryImpatientFixture : ComplexNavigationsQueryFixtureBase<ImpatientTestStore>
    {
        private readonly DbContextOptions options;

        public ComplexNavigationsQueryImpatientFixture()
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
                    .UseSqlServer(@"Server=.\sqlexpress; Database=efcore-impatient-complex-navigations; Trusted_Connection=true; MultipleActiveResultSets=True")
                    .Options;

            using (var context = new ComplexNavigationsContext(options))
            {
                // context.Database.EnsureDeleted();
                if (context.Database.EnsureCreated())
                {
                    ComplexNavigationsModelInitializer.Seed(context, tableSplitting: false);
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
