using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;
using Xunit.Sdk;

namespace Impatient.EFCore.Tests.Query
{
    public class AsyncGroupByQueryImpatientTest : AsyncGroupByQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public AsyncGroupByQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
        
        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override async Task GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg()
        {
            var ex = await Assert.ThrowsAsync<EqualException>(() => base.GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("577", ex.Actual);
        }
        
        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override async Task GroupBy_Constant_Select_Sum_Min_Key_Max_Avg()
        {
            var ex = await Assert.ThrowsAsync<EqualException>(() => base.GroupBy_Constant_Select_Sum_Min_Key_Max_Avg());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("830", ex.Actual);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override async Task GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg()
        {
            var ex = await Assert.ThrowsAsync<EqualException>(() => base.GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("830", ex.Actual);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override async Task GroupBy_empty_key_Aggregate_Key()
        {
            var ex = await Assert.ThrowsAsync<EqualException>(() => base.GroupBy_empty_key_Aggregate_Key());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("830", ex.Actual);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override async Task GroupBy_param_Select_Sum_Min_Key_Max_Avg()
        {
            var ex = await Assert.ThrowsAsync<EqualException>(() => base.GroupBy_param_Select_Sum_Min_Key_Max_Avg());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("830", ex.Actual);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override async Task GroupBy_param_with_element_selector_Select_Sum_Min_Key_Max_Avg()
        {
            var ex = await Assert.ThrowsAsync<EqualException>(() => base.GroupBy_param_with_element_selector_Select_Sum_Min_Key_Max_Avg());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("830", ex.Actual);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override async Task GroupBy_Select_First_GroupBy()
        {
            await AssertQuery<Customer>(
                cs =>
                    cs.GroupBy(c => c.City)
                      .Select(g => g.OrderBy(c => c.CustomerID).First())
                      .GroupBy(c => c.ContactName),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.CustomerID),
                entryCount: 91);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override async Task GroupBy_Shadow2()
        {
            var ex = await Assert.ThrowsAsync<EqualException>(() => base.GroupBy_Shadow2());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("1", ex.Actual);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override async Task GroupBy_with_element_selector2()
        {
            var ex = await Assert.ThrowsAsync<EqualException>(() => base.GroupBy_with_element_selector2());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("830", ex.Actual);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override async Task GroupBy_with_element_selector3()
        {
            var ex = await Assert.ThrowsAsync<EqualException>(() => base.GroupBy_with_element_selector3());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("9", ex.Actual);
        }

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

        [Fact]
        [Trait("Impatient", "EFCore missing entries")]
        [Trait("Impatient", "Adjusted entry count")]
        public override Task Join_GroupBy_Aggregate_in_subquery()
        {
            return AssertQuery<Order, Customer>(
                (os, cs) =>
                    from o in os.Where(o => o.OrderID < 10400)
                    join i in (from c in cs
                               join a in os.GroupBy(o => o.CustomerID)
                                           .Where(g => g.Count() > 5)
                                           .Select(g => new { CustomerID = g.Key, LastOrderID = g.Max(o => o.OrderID) })
                                   on c.CustomerID equals a.CustomerID
                               select new { c, a.LastOrderID })
                        on o.CustomerID equals i.c.CustomerID
                    select new { o, i.c, i.c.CustomerID },
                entryCount: 187);
        }
    }
}
