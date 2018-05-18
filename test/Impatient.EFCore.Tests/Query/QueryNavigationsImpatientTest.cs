using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class QueryNavigationsImpatientTest : QueryNavigationsTestBase<NorthwindQueryImpatientFixture>
    {
        public QueryNavigationsImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        #region skips

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override void Navigation_projection_on_groupjoin_qsre()
        {
            base.Navigation_projection_on_groupjoin_qsre();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override void Navigation_projection_on_groupjoin_qsre_no_outer_in_final_result()
        {
            base.Navigation_projection_on_groupjoin_qsre_no_outer_in_final_result();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
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

        #endregion

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        [Trait("Impatient", "EFCore missing entries")]
        public override void Select_collection_FirstOrDefault_project_entity()
        {
            //base.Select_collection_FirstOrDefault_project_entity();

            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Take(2).Select(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()),
                entryCount: 2);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        [Trait("Impatient", "EFCore missing entries")]
        public override void Take_Select_Navigation()
        {
            //base.Take_Select_Navigation();
            
            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID).Take(2)
                    .Select(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()),
                entryCount: 2);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        [Trait("Impatient", "EFCore missing entries")]
        public override void Skip_Select_Navigation()
        {
            //base.Skip_Select_Navigation();

            AssertQuery<Customer>(
                cs => cs.OrderBy(c => c.CustomerID)
                    .Skip(20)
                    .Select(c => c.Orders.OrderBy(o => o.OrderID).FirstOrDefault()),
                assertOrder: true,
                entryCount: 69);
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Project_first_or_default_on_empty_collection_of_value_types_returns_proper_default()
        {
            AssertQuery<Customer>(
                cs => from c in cs
                      select new { c.CustomerID, OrderId = c.Orders.OrderBy(o => o.OrderID).Select(o => o.OrderID).FirstOrDefault() },
                cs => from c in cs
                      select new { c.CustomerID, OrderId = c.Orders.OrderBy(o => o.OrderID).Select(o => o.OrderID).FirstOrDefault() });
        }
    }
}
