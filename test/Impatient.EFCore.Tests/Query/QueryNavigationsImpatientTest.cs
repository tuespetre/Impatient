using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class QueryNavigationsImpatientTest : QueryNavigationsTestBase<NorthwindQueryImpatientFixture>
    {
        public QueryNavigationsImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Navigation_projection_on_groupjoin_qsre()
        {
            //base.Navigation_projection_on_groupjoin_qsre();

            AssertQuery<Customer, Order>(
                (cs, os) => from c in cs
                            join o in os on c.CustomerID equals o.CustomerID into grouping
                            where c.CustomerID == "ALFKI"
                            select new { c, G = grouping.Select(o => o.OrderDetails).ToList() },
                elementSorter: e => e.c.CustomerID,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.c.CustomerID, a.c.CustomerID);
                    CollectionAsserter<OrderDetail>(
                        ee => ee.OrderID + " " + ee.ProductID,
                        (ee, aa) =>
                        {
                            Assert.Equal(ee.OrderID, aa.OrderID);
                            Assert.Equal(ee.ProductID, aa.ProductID);
                        });
                },
                entryCount: 13);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Navigation_projection_on_groupjoin_qsre_no_outer_in_final_result()
        {
            //base.Navigation_projection_on_groupjoin_qsre_no_outer_in_final_result();

            AssertQuery<Customer, Order>(
                (cs, os) => from c in cs
                            join o in os on c.CustomerID equals o.CustomerID into grouping
                            where c.CustomerID == "ALFKI"
                            orderby c.CustomerID
                            select grouping.Select(o => o.OrderDetails).ToList(),
                assertOrder: true,
                elementAsserter: (e, a) =>
                {
                    var expected = ((IEnumerable<IEnumerable<OrderDetail>>)e).SelectMany(i => i).ToList();
                    var actual = ((IEnumerable<IEnumerable<OrderDetail>>)e).SelectMany(i => i).ToList();

                    Assert.Equal(expected, actual);
                },
                entryCount: 12);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Navigation_projection_on_groupjoin_qsre_with_empty_grouping()
        {
            //base.Navigation_projection_on_groupjoin_qsre_with_empty_grouping();

            var anatrsOrders = new[] { 10308, 10625, 10759, 10926 };

            AssertQuery<Customer, Order>(
                (cs, os) => from c in cs
                            join o in os.Where(oo => !anatrsOrders.Contains(oo.OrderID)) on c.CustomerID equals o.CustomerID into grouping
                            where c.CustomerID.StartsWith("A")
                            select new { c, G = grouping.Select(o => o.OrderDetails).ToList() },
                elementSorter: e => e.c.CustomerID,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.c.CustomerID, a.c.CustomerID);

                    var expected = ((IEnumerable<IEnumerable<OrderDetail>>)e.G).SelectMany(i => i).ToList();
                    var actual = ((IEnumerable<IEnumerable<OrderDetail>>)e.G).SelectMany(i => i).ToList();

                    Assert.Equal(expected, actual);
                },
                entryCount: 63);
        }

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
