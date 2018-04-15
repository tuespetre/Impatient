using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class AsyncQueryImpatientTest : AsyncQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public AsyncQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = "Need to clarify with EF Core team why they expect an entry from this.")]
        public override Task Default_if_empty_top_level_arg()
        {
            return base.Default_if_empty_top_level_arg();
        }

        [Fact(Skip = "Not sure what this is supposed to do")]
        public override Task Where_nested_field_access_closure_via_query_cache_error_method_null()
        {
            return base.Where_nested_field_access_closure_via_query_cache_error_method_null();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesGroupedEntitiesAreNotTracked)]
        public override Task GroupBy_with_element_selector2()
        {
            return base.GroupBy_with_element_selector2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesGroupedEntitiesAreNotTracked)]
        public override Task GroupBy_with_element_selector3()
        {
            return base.GroupBy_with_element_selector3();
        }

        [Fact(Skip = "I don't want to support ambiguous ordering semantics")]
        public override Task OrderBy_multiple()
        {
            return base.OrderBy_multiple();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override Task Join_client_new_expression()
        {
            return base.Join_client_new_expression();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override Task GroupBy_Shadow2()
        {
            return base.GroupBy_Shadow2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override Task OrderBy_multiple_queries()
        {
            return base.OrderBy_multiple_queries();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override Task Where_subquery_on_collection()
        {
            return base.Where_subquery_on_collection();
        }

        public override Task Projection_when_client_evald_subquery()
        {
            return base.Projection_when_client_evald_subquery();
        }

        public override Task ToArray_on_nav_subquery_in_projection()
        {
            return base.ToArray_on_nav_subquery_in_projection();
        }

        public override Task ToArray_on_nav_subquery_in_projection_nested()
        {
            return base.ToArray_on_nav_subquery_in_projection_nested();
        }

        public override Task ToList_on_nav_subquery_in_projection()
        {
            return base.ToList_on_nav_subquery_in_projection();
        }

        public override Task Average_on_nav_subquery_in_projection()
        {
            return base.Average_on_nav_subquery_in_projection();
        }

        public override Task Where_subquery_correlated_client_eval()
        {
            return base.Where_subquery_correlated_client_eval();
        }

        public override Task ToListAsync_can_be_canceled()
        {
            return base.ToListAsync_can_be_canceled();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override Task Mixed_sync_async_query()
        {
            return base.Mixed_sync_async_query();
        }

        public override Task LoadAsync_should_track_results()
        {
            return base.LoadAsync_should_track_results();
        }

        public override Task Where_all_any_client()
        {
            return base.Where_all_any_client();
        }

        protected override Task Single_Predicate_Cancellation_test(CancellationToken cancellationToken)
        {
            return base.Single_Predicate_Cancellation_test(cancellationToken);
        }

        public override Task Mixed_sync_async_in_query_cache()
        {
            return base.Mixed_sync_async_in_query_cache();
        }

        public override Task Queryable_simple()
        {
            return base.Queryable_simple();
        }

        public override Task Queryable_simple_anonymous()
        {
            return base.Queryable_simple_anonymous();
        }

        public override Task Queryable_nested_simple()
        {
            return base.Queryable_nested_simple();
        }

        public override Task Take_simple()
        {
            return base.Take_simple();
        }

        public override Task Take_simple_projection()
        {
            return base.Take_simple_projection();
        }

        public override Task Skip()
        {
            return base.Skip();
        }

        public override Task Take_Skip()
        {
            return base.Take_Skip();
        }

        public override Task Distinct_Skip()
        {
            return base.Distinct_Skip();
        }

        public override Task Skip_Take()
        {
            return base.Skip_Take();
        }

        public override Task Distinct_Skip_Take()
        {
            return base.Distinct_Skip_Take();
        }

        public override Task Skip_Distinct()
        {
            return base.Skip_Distinct();
        }

        public override Task Skip_Take_Distinct()
        {
            return base.Skip_Take_Distinct();
        }

        public override Task Take_Skip_Distinct()
        {
            return base.Take_Skip_Distinct();
        }

        public override Task Take_Distinct()
        {
            return base.Take_Distinct();
        }

        public override Task Distinct_Take()
        {
            return base.Distinct_Take();
        }

        public override Task Distinct_Take_Count()
        {
            return base.Distinct_Take_Count();
        }

        public override Task Take_Distinct_Count()
        {
            return base.Take_Distinct_Count();
        }

        public override Task Any_simple()
        {
            return base.Any_simple();
        }

        public override Task OrderBy_Take_Count()
        {
            return base.OrderBy_Take_Count();
        }

        public override Task Take_OrderBy_Count()
        {
            return base.Take_OrderBy_Count();
        }

        public override Task Any_predicate()
        {
            return base.Any_predicate();
        }

        public override Task All_top_level()
        {
            return base.All_top_level();
        }

        public override Task All_top_level_subquery()
        {
            return base.All_top_level_subquery();
        }

        public override Task Select_into()
        {
            return base.Select_into();
        }

        public override Task Projection_when_arithmetic_expressions()
        {
            return base.Projection_when_arithmetic_expressions();
        }

        public override Task Projection_when_arithmetic_mixed()
        {
            return base.Projection_when_arithmetic_mixed();
        }

        public override Task Projection_when_arithmetic_mixed_subqueries()
        {
            return base.Projection_when_arithmetic_mixed_subqueries();
        }

        public override Task Projection_when_null_value()
        {
            return base.Projection_when_null_value();
        }

        public override Task Take_with_single()
        {
            return base.Take_with_single();
        }

        public override Task Take_with_single_select_many()
        {
            return base.Take_with_single_select_many();
        }

        public override Task Cast_results_to_object()
        {
            return base.Cast_results_to_object();
        }

        public override Task Where_simple()
        {
            return base.Where_simple();
        }

        public override Task Where_simple_closure()
        {
            return base.Where_simple_closure();
        }

        public override Task Where_simple_closure_constant()
        {
            return base.Where_simple_closure_constant();
        }

        public override Task Where_simple_closure_via_query_cache()
        {
            return base.Where_simple_closure_via_query_cache();
        }

        public override Task Where_method_call_nullable_type_closure_via_query_cache()
        {
            return base.Where_method_call_nullable_type_closure_via_query_cache();
        }

        public override Task Where_method_call_nullable_type_reverse_closure_via_query_cache()
        {
            return base.Where_method_call_nullable_type_reverse_closure_via_query_cache();
        }

        public override Task Where_method_call_closure_via_query_cache()
        {
            return base.Where_method_call_closure_via_query_cache();
        }

        public override Task Where_field_access_closure_via_query_cache()
        {
            return base.Where_field_access_closure_via_query_cache();
        }

        public override Task Where_property_access_closure_via_query_cache()
        {
            return base.Where_property_access_closure_via_query_cache();
        }

        public override Task Where_static_field_access_closure_via_query_cache()
        {
            return base.Where_static_field_access_closure_via_query_cache();
        }

        public override Task Where_static_property_access_closure_via_query_cache()
        {
            return base.Where_static_property_access_closure_via_query_cache();
        }

        public override Task Where_nested_field_access_closure_via_query_cache()
        {
            return base.Where_nested_field_access_closure_via_query_cache();
        }

        public override Task Where_nested_property_access_closure_via_query_cache()
        {
            return base.Where_nested_property_access_closure_via_query_cache();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override Task Where_nested_field_access_closure_via_query_cache_error_null()
        {
            return base.Where_nested_field_access_closure_via_query_cache_error_null();
        }

        public override Task Where_new_instance_field_access_closure_via_query_cache()
        {
            return base.Where_new_instance_field_access_closure_via_query_cache();
        }

        public override Task Where_simple_closure_via_query_cache_nullable_type()
        {
            return base.Where_simple_closure_via_query_cache_nullable_type();
        }

        public override Task Where_simple_closure_via_query_cache_nullable_type_reverse()
        {
            return base.Where_simple_closure_via_query_cache_nullable_type_reverse();
        }

        public override Task Where_simple_shadow()
        {
            return base.Where_simple_shadow();
        }

        public override Task Where_simple_shadow_projection()
        {
            return base.Where_simple_shadow_projection();
        }

        public override Task Where_simple_shadow_projection_mixed()
        {
            return base.Where_simple_shadow_projection_mixed();
        }

        public override Task Where_simple_shadow_subquery()
        {
            return base.Where_simple_shadow_subquery();
        }

        public override Task Where_shadow_subquery_first()
        {
            return base.Where_shadow_subquery_first();
        }

        public override Task Where_client()
        {
            return base.Where_client();
        }

        public override Task First_client_predicate()
        {
            return base.First_client_predicate();
        }

        public override Task Where_equals_method_string()
        {
            return base.Where_equals_method_string();
        }

        public override Task Where_equals_method_int()
        {
            return base.Where_equals_method_int();
        }

        public override Task Where_comparison_nullable_type_not_null()
        {
            return base.Where_comparison_nullable_type_not_null();
        }

        public override Task Where_comparison_nullable_type_null()
        {
            return base.Where_comparison_nullable_type_null();
        }

        public override Task Where_string_length()
        {
            return base.Where_string_length();
        }

        public override Task Where_simple_reversed()
        {
            return base.Where_simple_reversed();
        }

        public override Task Where_is_null()
        {
            return base.Where_is_null();
        }

        public override Task Where_null_is_null()
        {
            return base.Where_null_is_null();
        }

        public override Task Where_constant_is_null()
        {
            return base.Where_constant_is_null();
        }

        public override Task Where_is_not_null()
        {
            return base.Where_is_not_null();
        }

        public override Task Where_null_is_not_null()
        {
            return base.Where_null_is_not_null();
        }

        public override Task Where_constant_is_not_null()
        {
            return base.Where_constant_is_not_null();
        }

        public override Task Where_identity_comparison()
        {
            return base.Where_identity_comparison();
        }

        public override Task Where_select_many_or()
        {
            return base.Where_select_many_or();
        }

        public override Task Where_select_many_or2()
        {
            return base.Where_select_many_or2();
        }

        public override Task Where_select_many_or3()
        {
            return base.Where_select_many_or3();
        }

        public override Task Where_select_many_or4()
        {
            return base.Where_select_many_or4();
        }

        public override Task Where_select_many_or_with_parameter()
        {
            return base.Where_select_many_or_with_parameter();
        }

        public override Task Where_in_optimization_multiple()
        {
            return base.Where_in_optimization_multiple();
        }

        public override Task Where_not_in_optimization1()
        {
            return base.Where_not_in_optimization1();
        }

        public override Task Where_not_in_optimization2()
        {
            return base.Where_not_in_optimization2();
        }

        public override Task Where_not_in_optimization3()
        {
            return base.Where_not_in_optimization3();
        }

        public override Task Where_not_in_optimization4()
        {
            return base.Where_not_in_optimization4();
        }

        public override Task Where_select_many_and()
        {
            return base.Where_select_many_and();
        }

        public override Task Where_primitive()
        {
            return base.Where_primitive();
        }

        public override Task Where_primitive_tracked()
        {
            return base.Where_primitive_tracked();
        }

        public override Task Where_primitive_tracked2()
        {
            return base.Where_primitive_tracked2();
        }

        public override Task Where_subquery_anon()
        {
            return base.Where_subquery_anon();
        }

        public override Task Where_bool_member()
        {
            return base.Where_bool_member();
        }

        public override Task Where_bool_member_false()
        {
            return base.Where_bool_member_false();
        }

        public override Task Where_bool_member_negated_twice()
        {
            return base.Where_bool_member_negated_twice();
        }

        public override Task Where_bool_member_shadow()
        {
            return base.Where_bool_member_shadow();
        }

        public override Task Where_bool_member_false_shadow()
        {
            return base.Where_bool_member_false_shadow();
        }

        public override Task Where_bool_member_equals_constant()
        {
            return base.Where_bool_member_equals_constant();
        }

        public override Task Where_bool_member_in_complex_predicate()
        {
            return base.Where_bool_member_in_complex_predicate();
        }

        public override Task Where_bool_member_compared_to_binary_expression()
        {
            return base.Where_bool_member_compared_to_binary_expression();
        }

        public override Task Where_not_bool_member_compared_to_not_bool_member()
        {
            return base.Where_not_bool_member_compared_to_not_bool_member();
        }

        public override Task Where_negated_boolean_expression_compared_to_another_negated_boolean_expression()
        {
            return base.Where_negated_boolean_expression_compared_to_another_negated_boolean_expression();
        }

        public override Task Where_not_bool_member_compared_to_binary_expression()
        {
            return base.Where_not_bool_member_compared_to_binary_expression();
        }

        public override Task Where_bool_parameter()
        {
            return base.Where_bool_parameter();
        }

        public override Task Where_bool_parameter_compared_to_binary_expression()
        {
            return base.Where_bool_parameter_compared_to_binary_expression();
        }

        public override Task Where_bool_member_and_parameter_compared_to_binary_expression_nested()
        {
            return base.Where_bool_member_and_parameter_compared_to_binary_expression_nested();
        }

        public override Task Where_de_morgan_or_optimizated()
        {
            return base.Where_de_morgan_or_optimizated();
        }

        public override Task Where_de_morgan_and_optimizated()
        {
            return base.Where_de_morgan_and_optimizated();
        }

        public override Task Where_complex_negated_expression_optimized()
        {
            return base.Where_complex_negated_expression_optimized();
        }

        public override Task Where_short_member_comparison()
        {
            return base.Where_short_member_comparison();
        }

        public override Task Where_true()
        {
            return base.Where_true();
        }

        public override Task Where_false()
        {
            return base.Where_false();
        }

        public override Task Where_bool_closure()
        {
            return base.Where_bool_closure();
        }

        public override Task Select_bool_closure()
        {
            return base.Select_bool_closure();
        }

        public override Task Where_compare_constructed_equal()
        {
            return base.Where_compare_constructed_equal();
        }

        public override Task Where_compare_constructed_multi_value_equal()
        {
            return base.Where_compare_constructed_multi_value_equal();
        }

        public override Task Where_compare_constructed_multi_value_not_equal()
        {
            return base.Where_compare_constructed_multi_value_not_equal();
        }

        public override Task Where_compare_constructed()
        {
            return base.Where_compare_constructed();
        }

        public override Task Where_compare_null()
        {
            return base.Where_compare_null();
        }

        public override Task Where_projection()
        {
            return base.Where_projection();
        }

        public override Task Select_scalar()
        {
            return base.Select_scalar();
        }

        public override Task Select_anonymous_one()
        {
            return base.Select_anonymous_one();
        }

        public override Task Select_anonymous_two()
        {
            return base.Select_anonymous_two();
        }

        public override Task Select_anonymous_three()
        {
            return base.Select_anonymous_three();
        }

        public override Task Select_anonymous_conditional_expression()
        {
            return base.Select_anonymous_conditional_expression();
        }

        public override Task Select_customer_table()
        {
            return base.Select_customer_table();
        }

        public override Task Select_customer_identity()
        {
            return base.Select_customer_identity();
        }

        public override Task Select_anonymous_with_object()
        {
            return base.Select_anonymous_with_object();
        }

        public override Task Select_anonymous_nested()
        {
            return base.Select_anonymous_nested();
        }

        public override Task Select_anonymous_empty()
        {
            return base.Select_anonymous_empty();
        }

        public override Task Select_anonymous_literal()
        {
            return base.Select_anonymous_literal();
        }

        public override Task Select_constant_int()
        {
            return base.Select_constant_int();
        }

        public override Task Select_constant_null_string()
        {
            return base.Select_constant_null_string();
        }

        public override Task Select_local()
        {
            return base.Select_local();
        }

        public override Task Select_scalar_primitive()
        {
            return base.Select_scalar_primitive();
        }

        public override Task Select_scalar_primitive_after_take()
        {
            return base.Select_scalar_primitive_after_take();
        }

        public override Task Select_project_filter()
        {
            return base.Select_project_filter();
        }

        public override Task Select_project_filter2()
        {
            return base.Select_project_filter2();
        }

        public override Task Select_nested_collection()
        {
            return base.Select_nested_collection();
        }

        public override Task Select_correlated_subquery_projection()
        {
            return base.Select_correlated_subquery_projection();
        }

        public override Task Select_correlated_subquery_filtered()
        {
            return base.Select_correlated_subquery_filtered();
        }

        public override Task Select_correlated_subquery_ordered()
        {
            return base.Select_correlated_subquery_ordered();
        }

        public override Task Select_nested_collection_in_anonymous_type()
        {
            return base.Select_nested_collection_in_anonymous_type();
        }

        public override Task Select_subquery_recursive_trivial()
        {
            return base.Select_subquery_recursive_trivial();
        }

        public override Task Where_query_composition()
        {
            return base.Where_query_composition();
        }

        public override Task Where_subquery_recursive_trivial()
        {
            return base.Where_subquery_recursive_trivial();
        }

        public override Task Select_nested_collection_deep()
        {
            return base.Select_nested_collection_deep();
        }

        public override Task OrderBy_scalar_primitive()
        {
            return base.OrderBy_scalar_primitive();
        }

        public override Task SelectMany_mixed()
        {
            return base.SelectMany_mixed();
        }

        public override Task SelectMany_simple1()
        {
            return base.SelectMany_simple1();
        }

        public override Task SelectMany_simple2()
        {
            return base.SelectMany_simple2();
        }

        public override Task SelectMany_entity_deep()
        {
            return base.SelectMany_entity_deep();
        }

        public override Task SelectMany_projection1()
        {
            return base.SelectMany_projection1();
        }

        public override Task SelectMany_projection2()
        {
            return base.SelectMany_projection2();
        }

        public override Task SelectMany_nested_simple()
        {
            return base.SelectMany_nested_simple();
        }

        public override Task SelectMany_correlated_simple()
        {
            return base.SelectMany_correlated_simple();
        }

        public override Task SelectMany_correlated_subquery_simple()
        {
            return base.SelectMany_correlated_subquery_simple();
        }

        public override Task SelectMany_correlated_subquery_hard()
        {
            return base.SelectMany_correlated_subquery_hard();
        }

        public override Task SelectMany_cartesian_product_with_ordering()
        {
            return base.SelectMany_cartesian_product_with_ordering();
        }

        public override Task SelectMany_primitive()
        {
            return base.SelectMany_primitive();
        }

        public override Task SelectMany_primitive_select_subquery()
        {
            return base.SelectMany_primitive_select_subquery();
        }

        public override Task Join_customers_orders_projection()
        {
            return base.Join_customers_orders_projection();
        }

        public override Task Join_customers_orders_entities()
        {
            return base.Join_customers_orders_entities();
        }

        public override Task Join_select_many()
        {
            return base.Join_select_many();
        }

        public override Task Join_customers_orders_select()
        {
            return base.Join_customers_orders_select();
        }

        public override Task Join_customers_orders_with_subquery()
        {
            return base.Join_customers_orders_with_subquery();
        }

        public override Task Join_customers_orders_with_subquery_anonymous_property_method()
        {
            return base.Join_customers_orders_with_subquery_anonymous_property_method();
        }

        public override Task Join_customers_orders_with_subquery_predicate()
        {
            return base.Join_customers_orders_with_subquery_predicate();
        }

        public override Task Join_composite_key()
        {
            return base.Join_composite_key();
        }

        public override Task Join_Where_Count()
        {
            return base.Join_Where_Count();
        }

        public override Task Multiple_joins_Where_Order_Any()
        {
            return base.Multiple_joins_Where_Order_Any();
        }

        public override Task Join_OrderBy_Count()
        {
            return base.Join_OrderBy_Count();
        }

        public override Task GroupJoin_customers_orders()
        {
            return base.GroupJoin_customers_orders();
        }

        public override Task GroupJoin_customers_orders_count()
        {
            return base.GroupJoin_customers_orders_count();
        }

        public override Task Default_if_empty_top_level()
        {
            return base.Default_if_empty_top_level();
        }

        public override Task Default_if_empty_top_level_positive()
        {
            return base.Default_if_empty_top_level_positive();
        }

        public override Task Default_if_empty_top_level_projection()
        {
            return base.Default_if_empty_top_level_projection();
        }

        public override Task GroupJoin_customers_employees_shadow()
        {
            return base.GroupJoin_customers_employees_shadow();
        }

        public override Task GroupJoin_customers_employees_subquery_shadow()
        {
            return base.GroupJoin_customers_employees_subquery_shadow();
        }

        public override Task SelectMany_customer_orders()
        {
            return base.SelectMany_customer_orders();
        }

        public override Task SelectMany_Count()
        {
            return base.SelectMany_Count();
        }

        public override Task SelectMany_LongCount()
        {
            return base.SelectMany_LongCount();
        }

        public override Task SelectMany_OrderBy_ThenBy_Any()
        {
            return base.SelectMany_OrderBy_ThenBy_Any();
        }

        public override Task OrderBy()
        {
            return base.OrderBy();
        }

        public override Task OrderBy_client_mixed()
        {
            return base.OrderBy_client_mixed();
        }

        public override Task OrderBy_shadow()
        {
            return base.OrderBy_shadow();
        }

        public override Task OrderBy_ThenBy_predicate()
        {
            return base.OrderBy_ThenBy_predicate();
        }

        public override Task OrderBy_correlated_subquery_lol()
        {
            return base.OrderBy_correlated_subquery_lol();
        }

        public override Task OrderBy_Select()
        {
            return base.OrderBy_Select();
        }

        public override Task OrderBy_ThenBy()
        {
            return base.OrderBy_ThenBy();
        }

        public override Task OrderByDescending()
        {
            return base.OrderByDescending();
        }

        public override Task OrderByDescending_ThenBy()
        {
            return base.OrderByDescending_ThenBy();
        }

        public override Task OrderByDescending_ThenByDescending()
        {
            return base.OrderByDescending_ThenByDescending();
        }

        public override Task OrderBy_ThenBy_Any()
        {
            return base.OrderBy_ThenBy_Any();
        }

        public override Task OrderBy_Join()
        {
            return base.OrderBy_Join();
        }

        public override Task OrderBy_SelectMany()
        {
            return base.OrderBy_SelectMany();
        }

        public override Task GroupBy_SelectMany()
        {
            return base.GroupBy_SelectMany();
        }

        public override Task GroupBy_simple()
        {
            return base.GroupBy_simple();
        }

        public override Task GroupBy_simple2()
        {
            return base.GroupBy_simple2();
        }

        public override Task GroupBy_first()
        {
            return base.GroupBy_first();
        }

        public override Task GroupBy_Sum()
        {
            return base.GroupBy_Sum();
        }

        public override Task GroupBy_Count()
        {
            return base.GroupBy_Count();
        }

        public override Task GroupBy_LongCount()
        {
            return base.GroupBy_LongCount();
        }

        public override Task GroupBy_Shadow()
        {
            return base.GroupBy_Shadow();
        }

        public override Task GroupBy_Shadow3()
        {
            return base.GroupBy_Shadow3();
        }

        public override Task GroupBy_Sum_Min_Max_Avg()
        {
            return base.GroupBy_Sum_Min_Max_Avg();
        }

        public override Task GroupBy_with_result_selector()
        {
            return base.GroupBy_with_result_selector();
        }

        public override Task GroupBy_with_element_selector_sum()
        {
            return base.GroupBy_with_element_selector_sum();
        }

        public override Task GroupBy_with_element_selector()
        {
            return base.GroupBy_with_element_selector();
        }

        public override Task GroupBy_with_element_selector_sum_max()
        {
            return base.GroupBy_with_element_selector_sum_max();
        }

        public override Task GroupBy_with_aggregate_function_by_navigation_property()
        {
            return base.GroupBy_with_aggregate_function_by_navigation_property();
        }

        public override Task GroupBy_with_anonymous_element()
        {
            return base.GroupBy_with_anonymous_element();
        }

        public override Task GroupBy_with_two_part_key()
        {
            return base.GroupBy_with_two_part_key();
        }

        public override Task OrderBy_GroupBy()
        {
            return base.OrderBy_GroupBy();
        }

        public override Task OrderBy_GroupBy_SelectMany()
        {
            return base.OrderBy_GroupBy_SelectMany();
        }

        public override Task OrderBy_GroupBy_SelectMany_shadow()
        {
            return base.OrderBy_GroupBy_SelectMany_shadow();
        }

        public override Task Sum_with_no_arg()
        {
            return base.Sum_with_no_arg();
        }

        public override Task Sum_with_binary_expression()
        {
            return base.Sum_with_binary_expression();
        }

        public override Task Sum_with_no_arg_empty()
        {
            return base.Sum_with_no_arg_empty();
        }

        public override Task Sum_with_arg()
        {
            return base.Sum_with_arg();
        }

        public override Task Sum_with_arg_expression()
        {
            return base.Sum_with_arg_expression();
        }

        public override Task Sum_with_coalesce()
        {
            return base.Sum_with_coalesce();
        }

        public override Task Sum_over_subquery_is_client_eval()
        {
            return base.Sum_over_subquery_is_client_eval();
        }

        public override Task Average_with_no_arg()
        {
            return base.Average_with_no_arg();
        }

        public override Task Average_with_binary_expression()
        {
            return base.Average_with_binary_expression();
        }

        public override Task Average_with_arg()
        {
            return base.Average_with_arg();
        }

        public override Task Average_with_arg_expression()
        {
            return base.Average_with_arg_expression();
        }

        public override Task Average_with_coalesce()
        {
            return base.Average_with_coalesce();
        }

        public override Task Average_over_subquery_is_client_eval()
        {
            return base.Average_over_subquery_is_client_eval();
        }

        public override Task Min_with_no_arg()
        {
            return base.Min_with_no_arg();
        }

        public override Task Min_with_arg()
        {
            return base.Min_with_arg();
        }

        public override Task Min_with_coalesce()
        {
            return base.Min_with_coalesce();
        }

        public override Task Min_over_subquery_is_client_eval()
        {
            return base.Min_over_subquery_is_client_eval();
        }

        public override Task Max_with_no_arg()
        {
            return base.Max_with_no_arg();
        }

        public override Task Max_with_arg()
        {
            return base.Max_with_arg();
        }

        public override Task Max_with_coalesce()
        {
            return base.Max_with_coalesce();
        }

        public override Task Max_over_subquery_is_client_eval()
        {
            return base.Max_over_subquery_is_client_eval();
        }

        public override Task Count_with_no_predicate()
        {
            return base.Count_with_no_predicate();
        }

        public override Task Count_with_predicate()
        {
            return base.Count_with_predicate();
        }

        public override Task Count_with_order_by()
        {
            return base.Count_with_order_by();
        }

        public override Task Where_OrderBy_Count()
        {
            return base.Where_OrderBy_Count();
        }

        public override Task OrderBy_Where_Count()
        {
            return base.OrderBy_Where_Count();
        }

        public override Task OrderBy_Count_with_predicate()
        {
            return base.OrderBy_Count_with_predicate();
        }

        public override Task OrderBy_Where_Count_with_predicate()
        {
            return base.OrderBy_Where_Count_with_predicate();
        }

        public override Task Where_OrderBy_Count_client_eval()
        {
            return base.Where_OrderBy_Count_client_eval();
        }

        public override Task Where_OrderBy_Count_client_eval_mixed()
        {
            return base.Where_OrderBy_Count_client_eval_mixed();
        }

        public override Task OrderBy_Where_Count_client_eval()
        {
            return base.OrderBy_Where_Count_client_eval();
        }

        public override Task OrderBy_Where_Count_client_eval_mixed()
        {
            return base.OrderBy_Where_Count_client_eval_mixed();
        }

        public override Task OrderBy_Count_with_predicate_client_eval()
        {
            return base.OrderBy_Count_with_predicate_client_eval();
        }

        public override Task OrderBy_Count_with_predicate_client_eval_mixed()
        {
            return base.OrderBy_Count_with_predicate_client_eval_mixed();
        }

        public override Task OrderBy_Where_Count_with_predicate_client_eval()
        {
            return base.OrderBy_Where_Count_with_predicate_client_eval();
        }

        public override Task OrderBy_Where_Count_with_predicate_client_eval_mixed()
        {
            return base.OrderBy_Where_Count_with_predicate_client_eval_mixed();
        }

        public override Task Distinct()
        {
            return base.Distinct();
        }

        public override Task Distinct_Scalar()
        {
            return base.Distinct_Scalar();
        }

        public override Task OrderBy_Distinct()
        {
            return base.OrderBy_Distinct();
        }

        public override Task Distinct_OrderBy()
        {
            return base.Distinct_OrderBy();
        }

        public override Task Distinct_GroupBy()
        {
            return base.Distinct_GroupBy();
        }

        public override Task GroupBy_Distinct()
        {
            return base.GroupBy_Distinct();
        }

        public override Task Distinct_Count()
        {
            return base.Distinct_Count();
        }

        public override Task Select_Distinct_Count()
        {
            return base.Select_Distinct_Count();
        }

        public override Task Select_Select_Distinct_Count()
        {
            return base.Select_Select_Distinct_Count();
        }

        public override Task Single_Throws()
        {
            return base.Single_Throws();
        }

        public override Task Single_Predicate()
        {
            return base.Single_Predicate();
        }

        public override Task Where_Single()
        {
            return base.Where_Single();
        }

        public override Task SingleOrDefault_Throws()
        {
            return base.SingleOrDefault_Throws();
        }

        public override Task SingleOrDefault_Predicate()
        {
            return base.SingleOrDefault_Predicate();
        }

        public override Task Where_SingleOrDefault()
        {
            return base.Where_SingleOrDefault();
        }

        public override Task FirstAsync()
        {
            return base.FirstAsync();
        }

        public override Task First_Predicate()
        {
            return base.First_Predicate();
        }

        public override Task Where_First()
        {
            return base.Where_First();
        }

        public override Task FirstOrDefault()
        {
            return base.FirstOrDefault();
        }

        public override Task FirstOrDefault_Predicate()
        {
            return base.FirstOrDefault_Predicate();
        }

        public override Task Where_FirstOrDefault()
        {
            return base.Where_FirstOrDefault();
        }

        public override Task Last()
        {
            return base.Last();
        }

        public override Task Last_when_no_order_by()
        {
            return base.Last_when_no_order_by();
        }

        public override Task Last_Predicate()
        {
            return base.Last_Predicate();
        }

        public override Task Where_Last()
        {
            return base.Where_Last();
        }

        public override Task LastOrDefault()
        {
            return base.LastOrDefault();
        }

        public override Task LastOrDefault_Predicate()
        {
            return base.LastOrDefault_Predicate();
        }

        public override Task Where_LastOrDefault()
        {
            return base.Where_LastOrDefault();
        }

        public override Task String_StartsWith_Literal()
        {
            return base.String_StartsWith_Literal();
        }

        public override Task String_StartsWith_Identity()
        {
            return base.String_StartsWith_Identity();
        }

        public override Task String_StartsWith_Column()
        {
            return base.String_StartsWith_Column();
        }

        public override Task String_StartsWith_MethodCall()
        {
            return base.String_StartsWith_MethodCall();
        }

        public override Task String_EndsWith_Literal()
        {
            return base.String_EndsWith_Literal();
        }

        public override Task String_EndsWith_Identity()
        {
            return base.String_EndsWith_Identity();
        }

        public override Task String_EndsWith_Column()
        {
            return base.String_EndsWith_Column();
        }

        public override Task String_EndsWith_MethodCall()
        {
            return base.String_EndsWith_MethodCall();
        }

        public override Task String_Contains_Literal()
        {
            // Overridden; EFCore SQL Server test does this too
            return AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);
        }

        public override Task String_Contains_Identity()
        {
            return base.String_Contains_Identity();
        }

        public override Task String_Contains_Column()
        {
            return base.String_Contains_Column();
        }

        public override Task String_Contains_MethodCall()
        {
            return base.String_Contains_MethodCall();
        }

        public override Task GroupJoin_simple()
        {
            return base.GroupJoin_simple();
        }

        public override Task GroupJoin_simple3()
        {
            return base.GroupJoin_simple3();
        }

        public override Task GroupJoin_projection()
        {
            return base.GroupJoin_projection();
        }

        public override Task GroupJoin_DefaultIfEmpty()
        {
            return base.GroupJoin_DefaultIfEmpty();
        }

        public override Task GroupJoin_DefaultIfEmpty2()
        {
            return base.GroupJoin_DefaultIfEmpty2();
        }

        public override Task GroupJoin_DefaultIfEmpty3()
        {
            return base.GroupJoin_DefaultIfEmpty3();
        }

        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override Task GroupJoin_tracking_groups()
        {
            return base.GroupJoin_tracking_groups();
        }

        public override Task GroupJoin_tracking_groups2()
        {
            return base.GroupJoin_tracking_groups2();
        }

        public override Task SelectMany_Joined()
        {
            return base.SelectMany_Joined();
        }

        public override Task SelectMany_Joined_DefaultIfEmpty()
        {
            return base.SelectMany_Joined_DefaultIfEmpty();
        }

        public override Task SelectMany_Joined_DefaultIfEmpty2()
        {
            return base.SelectMany_Joined_DefaultIfEmpty2();
        }

        public override Task Select_many_cross_join_same_collection()
        {
            return base.Select_many_cross_join_same_collection();
        }

        public override Task Join_same_collection_multiple()
        {
            return base.Join_same_collection_multiple();
        }

        public override Task Join_same_collection_force_alias_uniquefication()
        {
            return base.Join_same_collection_force_alias_uniquefication();
        }

        public override Task Contains_with_subquery()
        {
            return base.Contains_with_subquery();
        }

        public override Task Contains_with_local_array_closure()
        {
            return base.Contains_with_local_array_closure();
        }

        public override Task Contains_with_local_array_inline()
        {
            return base.Contains_with_local_array_inline();
        }

        public override Task Contains_with_local_list_closure()
        {
            return base.Contains_with_local_list_closure();
        }

        public override Task Contains_with_local_list_inline()
        {
            return base.Contains_with_local_list_inline();
        }

        public override Task Contains_with_local_list_inline_closure_mix()
        {
            return base.Contains_with_local_list_inline_closure_mix();
        }

        public override Task Contains_with_local_collection_false()
        {
            return base.Contains_with_local_collection_false();
        }

        public override Task Contains_with_local_collection_complex_predicate_and()
        {
            return base.Contains_with_local_collection_complex_predicate_and();
        }

        public override Task Contains_with_local_collection_complex_predicate_or()
        {
            return base.Contains_with_local_collection_complex_predicate_or();
        }

        public override Task Contains_with_local_collection_complex_predicate_not_matching_ins1()
        {
            return base.Contains_with_local_collection_complex_predicate_not_matching_ins1();
        }

        public override Task Contains_with_local_collection_complex_predicate_not_matching_ins2()
        {
            return base.Contains_with_local_collection_complex_predicate_not_matching_ins2();
        }

        public override Task Contains_with_local_collection_sql_injection()
        {
            return base.Contains_with_local_collection_sql_injection();
        }

        public override Task Contains_with_local_collection_empty_closure()
        {
            return base.Contains_with_local_collection_empty_closure();
        }

        public override Task Contains_with_local_collection_empty_inline()
        {
            return base.Contains_with_local_collection_empty_inline();
        }

        public override Task Contains_top_level()
        {
            return base.Contains_top_level();
        }

        public override Task Where_chain()
        {
            return base.Where_chain();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override Task Throws_on_concurrent_query_list()
        {
            return base.Throws_on_concurrent_query_list();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override Task Throws_on_concurrent_query_first()
        {
            return base.Throws_on_concurrent_query_first();
        }

        public override Task Concat_dbset()
        {
            return base.Concat_dbset();
        }

        public override Task Concat_simple()
        {
            return base.Concat_simple();
        }

        public override Task Concat_nested()
        {
            return base.Concat_nested();
        }

        public override Task Concat_non_entity()
        {
            return base.Concat_non_entity();
        }

        public override Task Except_dbset()
        {
            return base.Except_dbset();
        }

        public override Task Except_simple()
        {
            return base.Except_simple();
        }

        public override Task Except_nested()
        {
            return base.Except_nested();
        }

        public override Task Except_non_entity()
        {
            return base.Except_non_entity();
        }

        public override Task Intersect_dbset()
        {
            return base.Intersect_dbset();
        }

        public override Task Intersect_simple()
        {
            return base.Intersect_simple();
        }

        public override Task Intersect_nested()
        {
            return base.Intersect_nested();
        }

        public override Task Intersect_non_entity()
        {
            return base.Intersect_non_entity();
        }

        public override Task Union_dbset()
        {
            return base.Union_dbset();
        }

        public override Task Union_simple()
        {
            return base.Union_simple();
        }

        public override Task Union_nested()
        {
            return base.Union_nested();
        }

        public override Task Union_non_entity()
        {
            return base.Union_non_entity();
        }

        public override Task Where_bitwise_or()
        {
            return base.Where_bitwise_or();
        }

        public override Task Where_bitwise_and()
        {
            return base.Where_bitwise_and();
        }

        public override Task Select_bitwise_or()
        {
            return base.Select_bitwise_or();
        }

        public override Task Select_bitwise_or_multiple()
        {
            return base.Select_bitwise_or_multiple();
        }

        public override Task Select_bitwise_and()
        {
            return base.Select_bitwise_and();
        }

        public override Task Select_bitwise_and_or()
        {
            return base.Select_bitwise_and_or();
        }

        public override Task Where_bitwise_or_with_logical_or()
        {
            return base.Where_bitwise_or_with_logical_or();
        }

        public override Task Where_bitwise_and_with_logical_and()
        {
            return base.Where_bitwise_and_with_logical_and();
        }

        public override Task Where_bitwise_or_with_logical_and()
        {
            return base.Where_bitwise_or_with_logical_and();
        }

        public override Task Where_bitwise_and_with_logical_or()
        {
            return base.Where_bitwise_and_with_logical_or();
        }

        public override Task Select_bitwise_or_with_logical_or()
        {
            return base.Select_bitwise_or_with_logical_or();
        }

        public override Task Select_bitwise_and_with_logical_and()
        {
            return base.Select_bitwise_and_with_logical_and();
        }

        public override Task Skip_CountAsync()
        {
            return base.Skip_CountAsync();
        }

        public override Task Skip_LongCountAsync()
        {
            return base.Skip_LongCountAsync();
        }

        public override Task OrderBy_Skip_CountAsync()
        {
            return base.OrderBy_Skip_CountAsync();
        }

        public override Task OrderBy_Skip_LongCountAsync()
        {
            return base.OrderBy_Skip_LongCountAsync();
        }

        public override Task Contains_with_subquery_involving_join_binds_to_correct_table()
        {
            return base.Contains_with_subquery_involving_join_binds_to_correct_table();
        }
    }
}
