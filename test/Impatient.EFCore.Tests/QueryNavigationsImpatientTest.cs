using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class QueryNavigationsImpatientTest : QueryNavigationsTestBase<NorthwindQueryImpatientFixture>
    {
        public QueryNavigationsImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesGroupedEntitiesAreNotTracked)]
        public override void Navigation_projection_on_groupjoin_qsre()
        {
            base.Navigation_projection_on_groupjoin_qsre();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesGroupedEntitiesAreNotTracked)]
        public override void Navigation_projection_on_groupjoin_qsre_no_outer_in_final_result()
        {
            base.Navigation_projection_on_groupjoin_qsre_no_outer_in_final_result();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesGroupedEntitiesAreNotTracked)]
        public override void Navigation_projection_on_groupjoin_qsre_with_empty_grouping()
        {
            base.Navigation_projection_on_groupjoin_qsre_with_empty_grouping();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesIntermediateNavigationsAreTracked)]
        public override void Select_collection_navigation_multi_part()
        {
            base.Select_collection_navigation_multi_part();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesIntermediateNavigationsAreTracked)]
        public override void Select_collection_navigation_simple()
        {
            base.Select_collection_navigation_simple();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Select_collection_FirstOrDefault_project_entity()
        {
            base.Select_collection_FirstOrDefault_project_entity();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Take_Select_Navigation()
        {
            base.Take_Select_Navigation();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Skip_Select_Navigation()
        {
            base.Skip_Select_Navigation();
        }

        [Fact, Trait("Skipped by EFCore", "Unskipped by us")]
        public override void Project_first_or_default_on_empty_collection_of_value_types_returns_proper_default()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      select new { c.CustomerID, OrderId = c.Orders.OrderBy(o => o.OrderID).Select(o => o.OrderID).FirstOrDefault() },
                cs => from c in cs
                      select new { c.CustomerID, OrderId = c.Orders.OrderBy(o => o.OrderID).Select(o => o.OrderID).FirstOrDefault() });
        }

        [Fact, Trait("Skipped by EFCore", "Unskipped by us")]
        public override void Client_groupjoin_with_orderby_key_descending()
        {
            base.Client_groupjoin_with_orderby_key_descending();
        }
    }
}
