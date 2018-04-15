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
        }

        [Fact(Skip = "Not sure I agree with this test.")]
        public override void Average_with_division_on_decimal_no_significant_digits()
        {
            base.Average_with_division_on_decimal_no_significant_digits();
        }

        [Fact(Skip = "We do not support the exception behavior.")]
        public override void Average_no_data()
        {
            base.Average_no_data();
        }

        [Fact(Skip = "We do not support the exception behavior.")]
        public override void Average_no_data_subquery()
        {
            base.Average_no_data_subquery();
        }

        [Fact(Skip = "We do not support the exception behavior.")]
        public override void Max_no_data()
        {
            base.Max_no_data();
        }

        [Fact(Skip = "We do not support the exception behavior.")]
        public override void Max_no_data_subquery()
        {
            base.Max_no_data_subquery();
        }

        [Fact(Skip = "We do not support the exception behavior.")]
        public override void Min_no_data()
        {
            base.Min_no_data();
        }

        [Fact(Skip = "We do not support the exception behavior.")]
        public override void Min_no_data_subquery()
        {
            base.Min_no_data_subquery();
        }
        
        [Fact(Skip = "Need to clarify with EF Core team why they expect an entry from this.")]
        public override void Default_if_empty_top_level_arg()
        {
            base.Default_if_empty_top_level_arg();
        }

        [Fact(Skip = "Not sure what this is supposed to do")]
        public override void Where_nested_field_access_closure_via_query_cache_error_method_null()
        {
            base.Where_nested_field_access_closure_via_query_cache_error_method_null();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesGroupedEntitiesAreNotTracked)]
        public override void GroupBy_with_element_selector2()
        {
            base.GroupBy_with_element_selector2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesGroupedEntitiesAreNotTracked)]
        public override void GroupBy_with_element_selector3()
        {
            base.GroupBy_with_element_selector3();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesGroupedEntitiesAreNotTracked)]
        public override void GroupJoin_customers_orders()
        {
            base.GroupJoin_customers_orders();
        }

        [Fact(Skip = "I don't want to support ambiguous ordering semantics")]
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

        public override void String_Contains_Literal()
        {
            // Overridden; EFCore SQL Server test does this too.
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);
        }
    }
}
