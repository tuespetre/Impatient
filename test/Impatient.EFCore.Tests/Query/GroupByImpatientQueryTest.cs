using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class GroupByImpatientQueryTest : GroupByQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public GroupByImpatientQueryTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        #region skips

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override void GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg()
        {
            base.GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override void GroupBy_Constant_Select_Sum_Min_Key_Max_Avg()
        {
            base.GroupBy_Constant_Select_Sum_Min_Key_Max_Avg();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override void GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg()
        {
            base.GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override void GroupBy_param_Select_Sum_Min_Key_Max_Avg()
        {
            base.GroupBy_param_Select_Sum_Min_Key_Max_Avg();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override void GroupBy_param_with_element_selector_Select_Sum_Min_Key_Max_Avg()
        {
            base.GroupBy_param_with_element_selector_Select_Sum_Min_Key_Max_Avg();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override void GroupBy_Shadow2()
        {
            base.GroupBy_Shadow2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override void GroupBy_with_element_selector2()
        {
            base.GroupBy_with_element_selector2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override void GroupBy_with_element_selector3()
        {
            base.GroupBy_with_element_selector3();
        }

        #endregion


        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void GroupBy_anonymous_subquery()
        {
            AssertQuery<Customer>(
                cs => cs
                    .Select(c => new { c.City, c.CustomerID })
                    .GroupBy(a => from c2 in cs select c2),
                elementAsserter: (a, b) =>
                {
                    var ca = (IGrouping<IQueryable<Customer>, dynamic>)a;
                    var cb = (IGrouping<IQueryable<Customer>, dynamic>)b;

                    Assert.Equal(ca.Key.AsEnumerable(), cb.Key.AsEnumerable());
                    Assert.Equal(ca.AsEnumerable().OrderBy(d => d.CustomerID), cb.AsEnumerable().OrderBy(d => d.CustomerID));
                },
                elementSorter: o =>
                {
                    var co = (IGrouping<IQueryable<Customer>, dynamic>)o;

                    return co.First().CustomerID;
                },
                assertOrder: false,
                entryCount: 91);
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Union_simple_groupby()
        {
            // Corrected entry count from 19 to 0.

            AssertQuery<Customer>(
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Union(cs.Where(c => c.City == "México D.F."))
                    .GroupBy(c => c.City)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Total = g.Count()
                        }),
                entryCount: 0);
        }
    }
}
