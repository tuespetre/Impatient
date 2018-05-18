using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class ComplexNavigationsQueryImpatientTest : ComplexNavigationsQueryTestBase<ComplexNavigationsQueryImpatientFixture>
    {
        public ComplexNavigationsQueryImpatientTest(ComplexNavigationsQueryImpatientFixture fixture) : base(fixture)
        {
        }

        #region skips

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Comparing_collection_navigation_on_optional_reference_to_null()
        {
            base.Comparing_collection_navigation_on_optional_reference_to_null();
        }

        [Fact(Skip = EFCoreSkipReasons.ManualLeftJoinNullabilityPropagation)]
        public override void Manually_created_left_join_propagates_nullability_to_navigations()
        {
            base.Manually_created_left_join_propagates_nullability_to_navigations();
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

        [Fact(Skip = EFCoreSkipReasons.NullNavigationProtection)]
        public override void GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method()
        {
            base.GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method();
        }

        [Fact(Skip = EFCoreSkipReasons.NullNavigationProtection)]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Accessing_optional_property_inside_result_operator_subquery()
        {
            base.Accessing_optional_property_inside_result_operator_subquery();
        }

        #endregion

        [Fact]
        [Trait("Impatient", "Overridden for sorting")]
        public override void GroupJoin_reference_to_group_in_OrderBy()
        {
            // Had to add the call to ThenBy.

            AssertQueryScalar<Level1, Level2>(
                (l1s, l2s) =>
                    from l1 in l1s
                    join l2 in l2s on l1.Id equals l2.Level1_Optional_Id into groupJoin
                    from l2 in groupJoin.DefaultIfEmpty()
                    orderby groupJoin.Count(), l1.Id
                    select l1.Id,
                assertOrder: true);
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer()
        {
            base.GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer();
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_inner()
        {
            base.GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_inner();
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Select_subquery_with_client_eval_and_multi_level_navigation()
        {
            base.Select_subquery_with_client_eval_and_multi_level_navigation();
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy()
        {
            base.Subquery_with_Distinct_Skip_FirstOrDefault_without_OrderBy();
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Include_inside_subquery()
        {
            base.Include_inside_subquery();
        }
    }

    public class ComplexNavigationsQueryImpatientFixture : ComplexNavigationsQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
