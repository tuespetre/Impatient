using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class QueryImpatientTest :
        QueryTestBase<NorthwindQueryImpatientFixture>,
        IClassFixture<NorthwindQueryImpatientFixture>
    {
        public QueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
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
        public override void Average_with_division_on_decimal_no_significant_digits()
        {
            base.Average_with_division_on_decimal_no_significant_digits();
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

        [Fact(Skip = EFCoreSkipReasons.TestAssumesNestedSubqueryResultsAreNotTracked)]
        public override void GroupJoin_customers_orders()
        {
            base.GroupJoin_customers_orders();
        }

        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override void OrderBy_multiple()
        {
            base.OrderBy_multiple();
        }
        
        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Join_client_new_expression()
        {
            base.Join_client_new_expression();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void GroupBy_Shadow2()
        {
            base.GroupBy_Shadow2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void OrderBy_multiple_queries()
        {
            base.OrderBy_multiple_queries();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Where_subquery_on_collection()
        {
            base.Where_subquery_on_collection();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Where_equals_on_mismatched_types_nullable_long_nullable_int()
        {
            base.Where_equals_on_mismatched_types_nullable_long_nullable_int();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Where_equals_on_mismatched_types_nullable_int_long()
        {
            base.Where_equals_on_mismatched_types_nullable_int_long();
        }
        
        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Where_equals_using_object_overload_on_mismatched_types()
        {
            base.Where_equals_using_object_overload_on_mismatched_types();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Queryable_simple_anonymous_subquery()
        {
            base.Queryable_simple_anonymous_subquery();
        }

        [Fact(Skip = EFCoreSkipReasons.PessimisticTracking)]
        public override void Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ()
        {
            base.Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override void Join_customers_orders_with_subquery_anonymous_property_method()
        {
            base.Join_customers_orders_with_subquery_anonymous_property_method();
        }

        [Fact(Skip = EFCoreSkipReasons.PessimisticTracking)]
        public override void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ()
        {
            base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ();
        }

        [Fact(Skip = EFCoreSkipReasons.PessimisticTracking)]
        public override void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1()
        {
            base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1();
        }

        [Fact(Skip = EFCoreSkipReasons.PessimisticTracking)]
        public override void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2()
        {
            base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void First_client_predicate()
        {
            // TODO: split predicate from method call during composition
            base.First_client_predicate();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Throws_on_concurrent_query_list()
        {
            base.Throws_on_concurrent_query_list();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Throws_on_concurrent_query_first()
        {
            base.Throws_on_concurrent_query_first();
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

        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override void GroupJoin_outer_projection3()
        {
            base.GroupJoin_outer_projection3();
        }

        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override void GroupJoin_outer_projection4()
        {
            base.GroupJoin_outer_projection4();
        }

        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override void Include_with_orderby_skip_preserves_ordering()
        {
            base.Include_with_orderby_skip_preserves_ordering();
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

        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override void GroupJoin_tracking_groups()
        {
            base.GroupJoin_tracking_groups();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Comparing_collection_navigation_to_null()
        {
            base.Comparing_collection_navigation_to_null();
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
        public override void String_Contains_Literal()
        {
            // Overridden; EFCore SQL Server test does this too.
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);
        }

        [Fact, Trait("Skipped by EFCore", "Unskipped by us")]
        public override void Union_simple_groupby()
        {
            AssertQuery<Customer>(
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Union(cs.Where(c => c.City == "México D.F."))
                    .GroupBy(c => c.City)
                    .Select(g => new
                    {
                        g.Key,
                        Total = g.Count()
                    }),
                elementSorter: e => e.Key,
                entryCount: 0);
        }

        [Fact, Trait("Skipped by EFCore", "Unskipped by us")]
        public override void Where_subquery_anon()
        {
            base.Where_subquery_anon();
        }

        [Fact, Trait("Skipped by EFCore", "Unskipped by us")]
        public override void GroupBy_anonymous_subquery()
        {
            // Overridden to apply ordering for assertion purposes
            AssertQuery<Customer>(cs =>
                cs.Select(c => new { c.City, c.CustomerID })
                    .OrderBy(c => c.CustomerID)
                    .GroupBy(a => from c2 in cs select c2),
                assertOrder: true);
        }

        [Fact, Trait("Skipped by EFCore", "Unskipped by us")]
        public override void Compare_two_collection_navigations_using_equals()
        {
            base.Compare_two_collection_navigations_using_equals();
        }

        private void AssertSql(params string[] expected)
            => Fixture.TestSqlLoggerFactory.AssertBaseline(expected);

        protected override void ClearLog()
            => Fixture.TestSqlLoggerFactory.Clear();
    }
}
