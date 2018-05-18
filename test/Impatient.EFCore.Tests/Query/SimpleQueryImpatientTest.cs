using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class SimpleQueryImpatientTest : SimpleQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public SimpleQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        #region math

        [Fact]
        public override void Select_math_round_int()
        {
            base.Select_math_round_int();

            AssertSql(@"SELECT ROUND(CAST(CAST([o].[OrderID] AS float) AS float), 0) AS [A]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10250");
        }

        [Fact]
        public override void Select_math_truncate_int()
        {
            base.Select_math_truncate_int();

            AssertSql(@"SELECT ROUND(CAST(CAST([o].[OrderID] AS float) AS float), 0, 1) AS [A]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10250");
        }

        [Fact]
        public override void Where_math_abs1()
        {
            base.Where_math_abs1();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ABS([od].[ProductID]) > 10");
        }

        [Fact]
        public override void Where_math_abs2()
        {
            base.Where_math_abs2();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE CAST(ABS([od].[Quantity]) AS int) > 10");
        }

        [Fact]
        public override void Where_math_abs3()
        {
            base.Where_math_abs3();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ABS([od].[UnitPrice]) > 10.0");
        }

        [Fact]
        public override void Where_math_abs_uncorrelated()
        {
            base.Where_math_abs_uncorrelated();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE 10 < [od].[ProductID]");
        }

        [Fact]
        public override void Where_math_acos()
        {
            base.Where_math_acos();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (ACOS(CAST([od].[Discount] AS float)) > 1)");
        }

        [Fact]
        public override void Where_math_asin()
        {
            base.Where_math_asin();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (ASIN(CAST([od].[Discount] AS float)) > 0)");
        }

        [Fact]
        public override void Where_math_atan()
        {
            base.Where_math_atan();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (ATAN(CAST([od].[Discount] AS float)) > 0)");
        }

        [Fact]
        public override void Where_math_atan2()
        {
            base.Where_math_atan2();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (ATN2(CAST([od].[Discount] AS float), 1) > 0)");
        }

        [Fact]
        public override void Where_math_ceiling1()
        {
            base.Where_math_ceiling1();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE CEILING(CAST([od].[Discount] AS float)) > 0");
        }

        [Fact]
        public override void Where_math_ceiling2()
        {
            base.Where_math_ceiling2();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE CEILING([od].[UnitPrice]) > 10.0");
        }

        [Fact]
        public override void Where_math_cos()
        {
            base.Where_math_cos();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (COS(CAST([od].[Discount] AS float)) > 0)");
        }

        [Fact]
        public override void Where_math_exp()
        {
            base.Where_math_exp();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (EXP(CAST([od].[Discount] AS float)) > 1)");
        }

        [Fact]
        public override void Where_math_floor()
        {
            base.Where_math_floor();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE FLOOR([od].[UnitPrice]) > 10.0");
        }

        [Fact]
        public override void Where_math_log()
        {
            base.Where_math_log();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE (([od].[OrderID] = 11077) AND ([od].[Discount] > 0)) AND (LOG(CAST([od].[Discount] AS float)) < 0)");
        }

        [Fact]
        public override void Where_math_log10()
        {
            base.Where_math_log10();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE (([od].[OrderID] = 11077) AND ([od].[Discount] > 0)) AND (LOG10(CAST([od].[Discount] AS float)) < 0)");
        }

        [Fact]
        public override void Where_math_log_new_base()
        {
            base.Where_math_log_new_base();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE (([od].[OrderID] = 11077) AND ([od].[Discount] > 0)) AND (LOG(CAST([od].[Discount] AS float), 7) < 0)");
        }

        [Fact]
        public override void Where_math_power()
        {
            base.Where_math_power();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE POWER(CAST([od].[Discount] AS float), 2) > 0.05000000074505806");
        }

        [Fact]
        public override void Where_math_round()
        {
            base.Where_math_round();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ROUND([od].[UnitPrice], 0) > 10.0");
        }

        [Fact]
        public override void Where_math_round2()
        {
            base.Where_math_round2();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ROUND([od].[UnitPrice], 2) > 100.0");
        }

        [Fact]
        public override void Where_math_sign()
        {
            base.Where_math_sign();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (SIGN([od].[Discount]) > 0)");
        }

        [Fact]
        public override void Where_math_sin()
        {
            base.Where_math_sin();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (SIN(CAST([od].[Discount] AS float)) > 0)");
        }

        [Fact]
        public override void Where_math_sqrt()
        {
            base.Where_math_sqrt();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (SQRT(CAST([od].[Discount] AS float)) > 0)");
        }

        [Fact]
        public override void Where_math_tan()
        {
            base.Where_math_tan();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (TAN(CAST([od].[Discount] AS float)) > 0)");
        }

        [Fact]
        public override void Where_math_truncate()
        {
            base.Where_math_truncate();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ROUND([od].[UnitPrice], 0, 1) > 10.0");
        }

        #endregion

        #region skips

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Join_client_new_expression()
        {
            base.Join_client_new_expression();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void OrderBy_multiple_queries()
        {
            base.OrderBy_multiple_queries();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Average_no_data()
        {
            base.Average_no_data();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Average_no_data_subquery()
        {
            base.Average_no_data_subquery();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Max_no_data()
        {
            base.Max_no_data();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Max_no_data_subquery()
        {
            base.Max_no_data_subquery();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Min_no_data()
        {
            base.Min_no_data();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Min_no_data_subquery()
        {
            base.Min_no_data_subquery();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Default_if_empty_top_level_arg()
        {
            base.Default_if_empty_top_level_arg();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Where_nested_field_access_closure_via_query_cache_error_method_null()
        {
            base.Where_nested_field_access_closure_via_query_cache_error_method_null();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void First_client_predicate()
        {
            // TODO: split predicate from method call during composition
            base.First_client_predicate();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Parameter_extraction_short_circuits_1()
        {
            base.Parameter_extraction_short_circuits_1();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Parameter_extraction_short_circuits_2()
        {
            base.Parameter_extraction_short_circuits_2();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Parameter_extraction_short_circuits_3()
        {
            base.Parameter_extraction_short_circuits_3();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Parameter_extraction_can_throw_exception_from_user_code()
        {
            base.Parameter_extraction_can_throw_exception_from_user_code();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Select_expression_date_add_milliseconds_above_the_range()
        {
            base.Select_expression_date_add_milliseconds_above_the_range();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Select_expression_date_add_milliseconds_below_the_range()
        {
            base.Select_expression_date_add_milliseconds_below_the_range();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Where_nested_field_access_closure_via_query_cache_error_null()
        {
            base.Where_nested_field_access_closure_via_query_cache_error_null();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Comparing_collection_navigation_to_null()
        {
            base.Comparing_collection_navigation_to_null();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void QueryType_select_where_navigation()
        {
            base.QueryType_select_where_navigation();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void QueryType_select_where_navigation_multi_level()
        {
            base.QueryType_select_where_navigation_multi_level();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void QueryType_with_defining_query()
        {
            base.QueryType_with_defining_query();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void QueryType_with_included_nav()
        {
            base.QueryType_with_included_nav();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void QueryType_with_mixed_tracking()
        {
            base.QueryType_with_mixed_tracking();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void QueryType_with_included_navs_multi_level()
        {
            base.QueryType_with_included_navs_multi_level();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override void GroupJoin_customers_orders()
        {
            base.GroupJoin_customers_orders();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override void Select_correlated_subquery_filtered()
        {
            base.Select_correlated_subquery_filtered();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override void Select_correlated_subquery_ordered()
        {
            base.Select_correlated_subquery_ordered();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override void Select_correlated_subquery_projection()
        {
            base.Select_correlated_subquery_projection();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override void Select_subquery_recursive_trivial()
        {
            base.Select_subquery_recursive_trivial();
        }

        #endregion

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        [Trait("Impatient", "EFCore missing entries")]
        public override void Join_customers_orders_with_subquery_anonymous_property_method()
        {
            //base.Join_customers_orders_with_subquery_anonymous_property_method();

            AssertQuery<Customer, Order>(
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
        [Trait("Impatient", "Adjusted entry count")]
        [Trait("Impatient", "EFCore missing entries")]
        public override void Queryable_simple_anonymous_subquery()
        {
            //base.Queryable_simple_anonymous_subquery();

            AssertQuery<Customer>(
                cs => cs.Select(c => new { c }).Take(91).Select(a => a.c),
                entryCount: 91);
        }

        private static IEnumerable<TElement> ClientDefaultIfEmpty<TElement>(IEnumerable<TElement> source)
        {
            return source?.Count() == 0 ? new[] { default(TElement) } : source;
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        [Trait("Impatient", "Pessimistic tracking")]
        public override void Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ()
        {
            AssertQuery<Employee>(
                es => from e1 in es
                      join e2 in es on e1.EmployeeID equals e2.ReportsTo into grouping
                      from e2 in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                      select new { City1 = e1.City, City2 = e2 != null ? e2.City : null },
#pragma warning restore IDE0031 // Use null propagation
                elementSorter: e => e.City1 + " " + e.City2,
                entryCount: 9);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        [Trait("Impatient", "Pessimistic tracking")]
        public override void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from o in os
                    join c in cs on o.CustomerID equals c.CustomerID into grouping
                    from c in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new { Id1 = o.CustomerID, Id2 = c != null ? c.CustomerID : null },
#pragma warning restore IDE0031 // Use null propagation
                elementSorter: e => e.Id1 + " " + e.Id2,
                entryCount: 919);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        [Trait("Impatient", "Pessimistic tracking")]
        public override void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from o in os
                    join c in cs on new { o.CustomerID, o.OrderID } equals new { c.CustomerID, OrderID = 10000 } into grouping
                    from c in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new { Id1 = o.CustomerID, Id2 = c != null ? c.CustomerID : null },
#pragma warning restore IDE0031 // Use null propagation
                elementSorter: e => e.Id1 + " " + e.Id2,
                entryCount: 830);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        [Trait("Impatient", "Pessimistic tracking")]
        public override void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from o in os
                    join c in cs on new { o.OrderID, o.CustomerID } equals new { OrderID = 10000, c.CustomerID } into grouping
                    from c in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new { Id1 = o.CustomerID, Id2 = c != null ? c.CustomerID : null },
#pragma warning restore IDE0031 // Use null propagation
                elementSorter: e => e.Id1 + " " + e.Id2,
                entryCount: 830);
        }

        [Fact]
        [Trait("Impatient", "Overridden for sorting")]
        public override void GroupJoin_outer_projection3()
        {
            AssertQuery<Customer, Order>(
                (cs, os) => cs.GroupJoin(os, c => c.CustomerID, o => o.CustomerID, (c, g) => new { g = g.Select(o => o.CustomerID) }),
                elementSorter: e => ((IEnumerable<string>)e.g).FirstOrDefault(),
                elementAsserter: (e, a) => CollectionAsserter<string>(s => s)(e.g, a.g));
        }

        [Fact]
        [Trait("Impatient", "Overridden for sorting")]
        public override void GroupJoin_outer_projection4()
        {
            AssertQuery<Customer, Order>(
                (cs, os) => cs.GroupJoin(os, c => c.CustomerID, o => o.CustomerID, (c, g) => g.Select(o => o.CustomerID)),
                elementSorter: e => ((IEnumerable<string>)e).FirstOrDefault(),
                elementAsserter: CollectionAsserter<string>(s => s));
        }

        [Fact]
        [Trait("Impatient", "Overridden for sorting")]
        public override void Include_with_orderby_skip_preserves_ordering()
        {
            // Had to add the ThenBy call.

            AssertQuery<Customer>(
                cs => cs
                    .Include(c => c.Orders)
                    .Where(c => c.CustomerID != "VAFFE" && c.CustomerID != "DRACD")
                    .OrderBy(c => c.City)
                    .ThenBy(c => c.CustomerID)
                    .Skip(40)
                    .Take(5),
                entryCount: 48,
                assertOrder: true);
        }

        [Fact]
        [Trait("Impatient", "Overridden for sorting")]
        public override void GroupJoin_tracking_groups()
        {
            AssertQuery<Customer, Order>(
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
        public override void OrderBy_multiple()
        {
            // Enumerable.OrderBy is implemented with a stable sort algorithm
            // but Queryable.OrderBy does not offer the same guarantee, even though
            // EFCore implements theirs that way.
            AssertQuery<Customer>(
                cs =>
                    cs.OrderBy(c => c.CustomerID)
                        // ReSharper disable once MultipleOrderBy
                        .OrderBy(c => c.Country)
                        .Select(c => c.City),
                assertOrder: false);
        }

        [Fact]
        [Trait("Impatient", "Overridden for semantics")]
        public override void String_Contains_Literal()
        {
            // Overridden; EFCore SQL Server test does this too.
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Where_subquery_anon()
        {
            base.Where_subquery_anon();
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Where_subquery_anon_nested()
        {
            AssertQuery<Employee, Order, Customer>(
                (es, os, cs) =>
                    from t in (
                        from e in es.OrderBy(ee => ee.EmployeeID).Take(3).Select(e => new { e }).Where(e => e.e.City == "Seattle")
                        from o in os.OrderBy(oo => oo.OrderID).Take(5).Select(o => new { o })
                        select new { e, o })
                    from c in cs.Take(2).Select(c => new { c })
                    select new { t.e, t.o, c },
                entryCount: 8);
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Compare_two_collection_navigations_using_equals()
        {
            base.Compare_two_collection_navigations_using_equals();
        }

        [Fact]
        [Trait("Impatient", "Overridden for infrastructure")]
        public override void Method_with_constant_queryable_arg()
        {
            // Overridden because the base test performs checks using EF Core's own
            // query compilation cache, which does not apply for us

            using (var context = CreateContext())
            {
                var count = QueryableArgQuery(context, new[] { "ALFKI" }.AsQueryable()).Count();
                Assert.Equal(1, count);

                count = QueryableArgQuery(context, new[] { "FOO" }.AsQueryable()).Count();
                Assert.Equal(0, count);
            }
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2()
        {
            // Overridden to correct entry count
            AssertQuery<Order>(
                os => os.Where(o => o.OrderID < 10250)
                .Select(
                    o => o.OrderDetails.OrderBy(od => od.Product.ProductName).Take(1).FirstOrDefault()),
                entryCount: 2);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Projection_when_arithmetic_mixed_subqueries()
        {
            // Overridden to correct entry count
            AssertQuery<Order, Employee>(
                (os, es) =>
                    from o in os.OrderBy(o => o.OrderID).Take(3).Select(o2 => new { o2, Mod = o2.OrderID % 2 })
                    from e in es.OrderBy(e => e.EmployeeID).Take(2).Select(e2 => new { e2, Square = e2.EmployeeID ^ 2 })
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
                entryCount: 5);
        }

        private static IQueryable<Customer> QueryableArgQuery(NorthwindContext context, IQueryable<string> ids)
        {
            return context.Customers.Where(c => ids.Contains(c.CustomerID));
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
