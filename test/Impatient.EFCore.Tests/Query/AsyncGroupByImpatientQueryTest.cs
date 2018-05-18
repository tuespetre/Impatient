using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class AsyncGroupByImpatientQueryTest : AsyncGroupByQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public AsyncGroupByImpatientQueryTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        #region skips

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override Task GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg()
        {
            return base.GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override Task GroupBy_Constant_Select_Sum_Min_Key_Max_Avg()
        {
            return base.GroupBy_Constant_Select_Sum_Min_Key_Max_Avg();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override Task GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg()
        {
            return base.GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override Task GroupBy_param_Select_Sum_Min_Key_Max_Avg()
        {
            return base.GroupBy_param_Select_Sum_Min_Key_Max_Avg();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override Task GroupBy_param_with_element_selector_Select_Sum_Min_Key_Max_Avg()
        {
            return base.GroupBy_param_with_element_selector_Select_Sum_Min_Key_Max_Avg();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override Task GroupBy_Shadow2()
        {
            return base.GroupBy_Shadow2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override Task GroupBy_with_element_selector2()
        {
            return base.GroupBy_with_element_selector2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override Task GroupBy_with_element_selector3()
        {
            return base.GroupBy_with_element_selector3();
        }

        #endregion

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override async Task GroupBy_anonymous_subquery()
        {
            await AssertQuery<Customer>(
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
        public override async Task Union_simple_groupby()
        {
            // Corrected entry count from 19 to 0.

            await AssertQuery<Customer>(
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
