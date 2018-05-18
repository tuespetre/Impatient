using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class ComplexNavigationsOwnedQueryImpatientTest : ComplexNavigationsOwnedQueryTestBase<ComplexNavigationsOwnedQueryImpatientFixture>
    {
        public ComplexNavigationsOwnedQueryImpatientTest(ComplexNavigationsOwnedQueryImpatientFixture fixture) : base(fixture)
        {
        }

        #region skips

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

        [Fact(Skip = EFCoreSkipReasons.NullNavigationProtection)]
        public override void GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method()
        {
            base.GroupJoin_on_a_subquery_containing_another_GroupJoin_projecting_outer_with_client_method();
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
        public override void Required_navigation_on_a_subquery_with_First_in_projection()
        {
            base.Required_navigation_on_a_subquery_with_First_in_projection();
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Select_subquery_with_client_eval_and_navigation1()
        {
            base.Select_subquery_with_client_eval_and_navigation1();
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Select_subquery_with_client_eval_and_navigation2()
        {
            base.Select_subquery_with_client_eval_and_navigation2();
        }
    }

    public class ComplexNavigationsOwnedQueryImpatientFixture : ComplexNavigationsOwnedQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
