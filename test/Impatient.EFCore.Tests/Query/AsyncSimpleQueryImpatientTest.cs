using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class AsyncSimpleQueryImpatientTest : AsyncSimpleQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public AsyncSimpleQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        #region skips

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override async Task Join_client_new_expression()
        {
            await base.Join_client_new_expression();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override async Task OrderBy_multiple_queries()
        {
            await base.OrderBy_multiple_queries();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override Task Default_if_empty_top_level_arg()
        {
            return base.Default_if_empty_top_level_arg();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override Task Where_nested_field_access_closure_via_query_cache_error_method_null()
        {
            return base.Where_nested_field_access_closure_via_query_cache_error_method_null();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override Task Mixed_sync_async_query()
        {
            return base.Mixed_sync_async_query();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override Task Where_nested_field_access_closure_via_query_cache_error_null()
        {
            return base.Where_nested_field_access_closure_via_query_cache_error_null();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override Task First_client_predicate()
        {
            // TODO: split predicate from method call during composition
            return base.First_client_predicate();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override Task Query_with_nav()
        {
            return base.Query_with_nav();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override Task Select_query_where_navigation()
        {
            return base.Select_query_where_navigation();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override Task Select_query_where_navigation_multi_level()
        {
            return base.Select_query_where_navigation_multi_level();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override Task Select_correlated_subquery_filtered()
        {
            return base.Select_correlated_subquery_filtered();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override Task Select_correlated_subquery_ordered()
        {
            return base.Select_correlated_subquery_ordered();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override Task Select_correlated_subquery_projection()
        {
            return base.Select_correlated_subquery_projection();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override Task Select_subquery_recursive_trivial()
        {
            return base.Select_subquery_recursive_trivial();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override Task GroupJoin_customers_orders()
        {
            return base.GroupJoin_customers_orders();
        }

        #endregion

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        [Trait("Impatient", "EFCore missing entries")]
        public override async Task Join_customers_orders_with_subquery_anonymous_property_method()
        {
            //await base.Join_customers_orders_with_subquery_anonymous_property_method();

            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o1 in
                        (from o2 in os orderby o2.OrderID select new { o2 }) on c.CustomerID equals o1.o2.CustomerID
                    where EF.Property<string>(o1.o2, "CustomerID") == "ALFKI"
                    select new { o1, o1.o2, Shadow = EF.Property<DateTime?>(o1.o2, "OrderDate") },
                elementSorter: e => e.o1.o2.OrderID,
                entryCount: 6);
        }

        [Fact]
        [Trait("Impatient", "Overridden for sorting")]
        public override async Task GroupJoin_tracking_groups()
        {
            await AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID into orders
                    select orders,
                elementSorter: os => ((IEnumerable<Order>)os).Select(o => o.CustomerID).FirstOrDefault(),
                elementAsserter: CollectionAsserter<Order>(o => o.OrderID),
                entryCount: 830);
        }

        [Fact]
        [Trait("Impatient", "Overridden for sorting")]
        public override async Task OrderBy_multiple()
        {
            // Enumerable.OrderBy is implemented with a stable sort algorithm
            // but Queryable.OrderBy does not offer the same guarantee, even though
            // EFCore implements theirs that way.
            await AssertQuery<Customer>(
                cs =>
                    cs.OrderBy(c => c.CustomerID)
                        // ReSharper disable once MultipleOrderBy
                        .OrderBy(c => c.Country)
                        .Select(c => c.City),
                assertOrder: false);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override async Task Projection_when_arithmetic_mixed_subqueries()
        {
            // Overridden to correct entry count. For proof, see:

            /*
SELECT COUNT(DISTINCT [e2.EmployeeID]), COUNT(DISTINCT [o2.OrderID])
FROM (
    SELECT CAST(CAST([e2].[EmployeeID] AS bigint) + CAST([o2].[OrderID] AS bigint) AS bigint) AS [Add], CAST([e2].[EmployeeID] ^ 2 AS bigint) AS [Square], [e2].[EmployeeID] AS [e2.EmployeeID], [e2].[City] AS [e2.City], [e2].[Country] AS [e2.Country], [e2].[FirstName] AS [e2.FirstName], [e2].[ReportsTo] AS [e2.ReportsTo], [e2].[Title] AS [e2.Title], 42 AS [Literal], [o2].[OrderID] AS [o2.OrderID], [o2].[CustomerID] AS [o2.CustomerID], [o2].[EmployeeID] AS [o2.EmployeeID], [o2].[OrderDate] AS [o2.OrderDate], CAST([o2].[OrderID] % 2 AS int) AS [Mod]
    FROM [Orders] AS [o2]
    CROSS JOIN [Employees] AS [e2]
) as t
             */

            await AssertQuery<Order, Employee>(
                (os, es) =>
                    from o in os.Select(o2 => new { o2, Mod = o2.OrderID % 2 })
                    from e in es.Select(e2 => new { e2, Square = e2.EmployeeID ^ 2 })
                    select new
                    {
                        Add = e.e2.EmployeeID + o.o2.OrderID,
                        e.Square,
                        e.e2,
                        Literal = 42,
                        o.o2,
                        o.Mod
                    },
                elementSorter: e => e.e2.EmployeeID + " " + e.o2.OrderID,
                entryCount: 839);
        }

        [Fact]
        [Trait("Impatient", "Overridden for semantics")]
        public override Task String_Contains_Literal()
        {
            // Overridden; EFCore SQL Server test does this too
            return AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override Task Where_subquery_anon()
        {
            return base.Where_subquery_anon();
        }
    }
}
