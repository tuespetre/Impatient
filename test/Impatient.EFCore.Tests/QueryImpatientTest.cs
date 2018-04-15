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

        public override void Query_when_evaluatable_queryable_method_call_with_repository()
        {
            base.Query_when_evaluatable_queryable_method_call_with_repository();
        }

        public override void Lifting_when_subquery_nested_order_by_simple()
        {
            base.Lifting_when_subquery_nested_order_by_simple();
        }

        public override void Lifting_when_subquery_nested_order_by_anonymous()
        {
            base.Lifting_when_subquery_nested_order_by_anonymous();
        }

        public override void Local_array()
        {
            base.Local_array();
        }

        public override void Method_with_constant_queryable_arg()
        {
            base.Method_with_constant_queryable_arg();
        }

        public override void Shaper_command_caching_when_parameter_names_different()
        {
            base.Shaper_command_caching_when_parameter_names_different();
        }

        public override void Entity_equality_self()
        {
            base.Entity_equality_self();
        }

        public override void Entity_equality_local()
        {
            base.Entity_equality_local();
        }

        public override void Entity_equality_local_inline()
        {
            base.Entity_equality_local_inline();
        }

        public override void Entity_equality_null()
        {
            base.Entity_equality_null();
        }

        public override void Entity_equality_not_null()
        {
            base.Entity_equality_not_null();
        }

        public override void Null_conditional_simple()
        {
            base.Null_conditional_simple();
        }

        public override void Null_conditional_deep()
        {
            base.Null_conditional_deep();
        }

        public override void Queryable_simple()
        {
            base.Queryable_simple();
        }

        public override void Queryable_simple_anonymous()
        {
            base.Queryable_simple_anonymous();
        }

        public override void Queryable_simple_anonymous_projection_subquery()
        {
            base.Queryable_simple_anonymous_projection_subquery();
        }

        public override void Queryable_reprojection()
        {
            base.Queryable_reprojection();
        }

        public override void Queryable_nested_simple()
        {
            base.Queryable_nested_simple();
        }

        public override void Take_simple()
        {
            base.Take_simple();
        }

        public override void Take_simple_parameterized()
        {
            base.Take_simple_parameterized();
        }

        public override void Take_simple_projection()
        {
            base.Take_simple_projection();
        }

        public override void Take_subquery_projection()
        {
            base.Take_subquery_projection();
        }

        public override void Skip()
        {
            base.Skip();
        }

        public override void Skip_no_orderby()
        {
            base.Skip_no_orderby();
        }

        public override void Take_Skip()
        {
            base.Take_Skip();
        }

        public override void Distinct_Skip()
        {
            base.Distinct_Skip();
        }

        public override void Skip_Take()
        {
            base.Skip_Take();
        }

        public override void Join_Customers_Orders_Skip_Take()
        {
            base.Join_Customers_Orders_Skip_Take();
        }

        public override void Join_Customers_Orders_Projection_With_String_Concat_Skip_Take()
        {
            base.Join_Customers_Orders_Projection_With_String_Concat_Skip_Take();
        }

        public override void Join_Customers_Orders_Orders_Skip_Take_Same_Properties()
        {
            base.Join_Customers_Orders_Orders_Skip_Take_Same_Properties();
        }

        public override void Distinct_Skip_Take()
        {
            base.Distinct_Skip_Take();
        }

        public override void Skip_Distinct()
        {
            base.Skip_Distinct();
        }

        public override void Skip_Take_Distinct()
        {
            base.Skip_Take_Distinct();
        }

        public override void Skip_Take_Any()
        {
            base.Skip_Take_Any();
        }

        public override void Skip_Take_All()
        {
            base.Skip_Take_All();
        }

        public override void Take_Skip_Distinct()
        {
            base.Take_Skip_Distinct();
        }

        public override void Take_Skip_Distinct_Caching()
        {
            base.Take_Skip_Distinct_Caching();
        }

        public override void Take_Distinct()
        {
            base.Take_Distinct();
        }

        public override void Distinct_Take()
        {
            base.Distinct_Take();
        }

        public override void Distinct_Take_Count()
        {
            base.Distinct_Take_Count();
        }

        public override void Take_Distinct_Count()
        {
            base.Take_Distinct_Count();
        }

        public override void Take_Where_Distinct_Count()
        {
            base.Take_Where_Distinct_Count();
        }

        public override void Any_simple()
        {
            base.Any_simple();
        }

        public override void OrderBy_Take_Count()
        {
            base.OrderBy_Take_Count();
        }

        public override void Take_OrderBy_Count()
        {
            base.Take_OrderBy_Count();
        }

        public override void Any_predicate()
        {
            base.Any_predicate();
        }

        public override void Any_nested_negated()
        {
            base.Any_nested_negated();
        }

        public override void Any_nested_negated2()
        {
            base.Any_nested_negated2();
        }

        public override void Any_nested_negated3()
        {
            base.Any_nested_negated3();
        }

        public override void Any_nested()
        {
            base.Any_nested();
        }

        public override void Any_nested2()
        {
            base.Any_nested2();
        }

        public override void Any_nested3()
        {
            base.Any_nested3();
        }

        public override void Any_with_multiple_conditions_still_uses_exists()
        {
            base.Any_with_multiple_conditions_still_uses_exists();
        }

        public override void All_top_level()
        {
            base.All_top_level();
        }

        public override void All_top_level_column()
        {
            base.All_top_level_column();
        }

        public override void All_top_level_subquery()
        {
            base.All_top_level_subquery();
        }

        public override void All_top_level_subquery_ef_property()
        {
            base.All_top_level_subquery_ef_property();
        }

        public override void All_client()
        {
            base.All_client();
        }

        public override void All_client_and_server_top_level()
        {
            base.All_client_and_server_top_level();
        }

        public override void All_client_or_server_top_level()
        {
            base.All_client_or_server_top_level();
        }

        public override void Take_with_single()
        {
            base.Take_with_single();
        }

        public override void Take_with_single_select_many()
        {
            base.Take_with_single_select_many();
        }

        public override void Cast_results_to_object()
        {
            base.Cast_results_to_object();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void First_client_predicate()
        {
            // TODO: split predicate from method call during composition
            base.First_client_predicate();
        }

        public override void Where_select_many_or()
        {
            base.Where_select_many_or();
        }

        public override void Where_select_many_or2()
        {
            base.Where_select_many_or2();
        }

        public override void Where_select_many_or3()
        {
            base.Where_select_many_or3();
        }

        public override void Where_select_many_or4()
        {
            base.Where_select_many_or4();
        }

        public override void Where_select_many_or_with_parameter()
        {
            base.Where_select_many_or_with_parameter();
        }

        public override void Where_subquery_anon()
        {
            base.Where_subquery_anon();
        }

        public override void Where_subquery_anon_nested()
        {
            base.Where_subquery_anon_nested();
        }

        public override void Where_subquery_expression()
        {
            base.Where_subquery_expression();
        }

        public override void Where_subquery_expression_same_parametername()
        {
            base.Where_subquery_expression_same_parametername();
        }

        public override void Select_DTO_distinct_translated_to_server()
        {
            base.Select_DTO_distinct_translated_to_server();
        }

        public override void Select_DTO_constructor_distinct_translated_to_server()
        {
            base.Select_DTO_constructor_distinct_translated_to_server();
        }

        public override void Select_DTO_with_member_init_distinct_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_translated_to_server();
        }

        public override void Select_DTO_with_member_init_distinct_in_subquery_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_in_subquery_translated_to_server();
        }

        public override void Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server();
        }

        public override void Select_correlated_subquery_projection()
        {
            base.Select_correlated_subquery_projection();
        }

        public override void Select_correlated_subquery_filtered()
        {
            base.Select_correlated_subquery_filtered();
        }

        public override void Select_correlated_subquery_ordered()
        {
            base.Select_correlated_subquery_ordered();
        }

        public override void Select_nested_collection_in_anonymous_type()
        {
            base.Select_nested_collection_in_anonymous_type();
        }

        public override void Select_subquery_recursive_trivial()
        {
            base.Select_subquery_recursive_trivial();
        }

        public override void Where_subquery_on_bool()
        {
            base.Where_subquery_on_bool();
        }

        public override void Where_query_composition()
        {
            base.Where_query_composition();
        }

        public override void Where_query_composition_is_null()
        {
            base.Where_query_composition_is_null();
        }

        public override void Where_query_composition_is_not_null()
        {
            base.Where_query_composition_is_not_null();
        }

        public override void Where_query_composition_entity_equality_one_element_SingleOrDefault()
        {
            base.Where_query_composition_entity_equality_one_element_SingleOrDefault();
        }

        public override void Where_query_composition_entity_equality_one_element_FirstOrDefault()
        {
            base.Where_query_composition_entity_equality_one_element_FirstOrDefault();
        }

        public override void Where_query_composition_entity_equality_no_elements_SingleOrDefault()
        {
            base.Where_query_composition_entity_equality_no_elements_SingleOrDefault();
        }

        public override void Where_query_composition_entity_equality_no_elements_Single()
        {
            base.Where_query_composition_entity_equality_no_elements_Single();
        }

        public override void Where_query_composition_entity_equality_no_elements_FirstOrDefault()
        {
            base.Where_query_composition_entity_equality_no_elements_FirstOrDefault();
        }

        public override void Where_query_composition_entity_equality_multiple_elements_SingleOrDefault()
        {
            base.Where_query_composition_entity_equality_multiple_elements_SingleOrDefault();
        }

        public override void Where_query_composition_entity_equality_multiple_elements_FirstOrDefault()
        {
            base.Where_query_composition_entity_equality_multiple_elements_FirstOrDefault();
        }

        public override void Where_query_composition2()
        {
            base.Where_query_composition2();
        }

        public override void Where_query_composition2_FirstOrDefault()
        {
            base.Where_query_composition2_FirstOrDefault();
        }

        public override void Where_query_composition2_FirstOrDefault_with_anonymous()
        {
            base.Where_query_composition2_FirstOrDefault_with_anonymous();
        }

        public override void Where_query_composition3()
        {
            base.Where_query_composition3();
        }

        public override void Where_query_composition4()
        {
            base.Where_query_composition4();
        }

        public override void Where_query_composition5()
        {
            base.Where_query_composition5();
        }

        public override void Where_query_composition6()
        {
            base.Where_query_composition6();
        }

        public override void Where_subquery_recursive_trivial()
        {
            base.Where_subquery_recursive_trivial();
        }

        public override void OrderBy_scalar_primitive()
        {
            base.OrderBy_scalar_primitive();
        }

        public override void SelectMany_mixed()
        {
            base.SelectMany_mixed();
        }

        public override void SelectMany_simple1()
        {
            base.SelectMany_simple1();
        }

        public override void SelectMany_simple_subquery()
        {
            base.SelectMany_simple_subquery();
        }

        public override void SelectMany_simple2()
        {
            base.SelectMany_simple2();
        }

        public override void SelectMany_entity_deep()
        {
            base.SelectMany_entity_deep();
        }

        public override void SelectMany_projection1()
        {
            base.SelectMany_projection1();
        }

        public override void SelectMany_projection2()
        {
            base.SelectMany_projection2();
        }

        public override void SelectMany_nested_simple()
        {
            base.SelectMany_nested_simple();
        }

        public override void SelectMany_correlated_simple()
        {
            base.SelectMany_correlated_simple();
        }

        public override void SelectMany_correlated_subquery_simple()
        {
            base.SelectMany_correlated_subquery_simple();
        }

        public override void SelectMany_correlated_subquery_hard()
        {
            base.SelectMany_correlated_subquery_hard();
        }

        public override void SelectMany_cartesian_product_with_ordering()
        {
            base.SelectMany_cartesian_product_with_ordering();
        }

        public override void SelectMany_primitive()
        {
            base.SelectMany_primitive();
        }

        public override void SelectMany_primitive_select_subquery()
        {
            base.SelectMany_primitive_select_subquery();
        }

        public override void Join_Where_Count()
        {
            base.Join_Where_Count();
        }

        public override void Multiple_joins_Where_Order_Any()
        {
            base.Multiple_joins_Where_Order_Any();
        }

        public override void Join_OrderBy_Count()
        {
            base.Join_OrderBy_Count();
        }

        public override void Where_join_select()
        {
            base.Where_join_select();
        }

        public override void Where_orderby_join_select()
        {
            base.Where_orderby_join_select();
        }

        public override void Where_join_orderby_join_select()
        {
            base.Where_join_orderby_join_select();
        }

        public override void Where_select_many()
        {
            base.Where_select_many();
        }

        public override void Where_orderby_select_many()
        {
            base.Where_orderby_select_many();
        }

        public override void Default_if_empty_top_level()
        {
            base.Default_if_empty_top_level();
        }

        public override void Default_if_empty_top_level_positive()
        {
            base.Default_if_empty_top_level_positive();
        }

        public override void Default_if_empty_top_level_projection()
        {
            base.Default_if_empty_top_level_projection();
        }

        public override void SelectMany_customer_orders()
        {
            base.SelectMany_customer_orders();
        }

        public override void SelectMany_Count()
        {
            base.SelectMany_Count();
        }

        public override void SelectMany_LongCount()
        {
            base.SelectMany_LongCount();
        }

        public override void SelectMany_OrderBy_ThenBy_Any()
        {
            base.SelectMany_OrderBy_ThenBy_Any();
        }

        public override void OrderBy()
        {
            base.OrderBy();
        }

        public override void OrderBy_true()
        {
            base.OrderBy_true();
        }

        public override void OrderBy_integer()
        {
            base.OrderBy_integer();
        }

        public override void OrderBy_parameter()
        {
            base.OrderBy_parameter();
        }

        public override void OrderBy_anon()
        {
            base.OrderBy_anon();
        }

        public override void OrderBy_anon2()
        {
            base.OrderBy_anon2();
        }

        public override void OrderBy_client_mixed()
        {
            base.OrderBy_client_mixed();
        }

        public override void OrderBy_shadow()
        {
            base.OrderBy_shadow();
        }

        public override void OrderBy_ThenBy_predicate()
        {
            base.OrderBy_ThenBy_predicate();
        }

        public override void OrderBy_correlated_subquery1()
        {
            base.OrderBy_correlated_subquery1();
        }

        public override void OrderBy_correlated_subquery2()
        {
            base.OrderBy_correlated_subquery2();
        }

        public override void OrderBy_Select()
        {
            base.OrderBy_Select();
        }

        public override void OrderBy_ThenBy()
        {
            base.OrderBy_ThenBy();
        }

        public override void OrderByDescending()
        {
            base.OrderByDescending();
        }

        public override void OrderByDescending_ThenBy()
        {
            base.OrderByDescending_ThenBy();
        }

        public override void OrderByDescending_ThenByDescending()
        {
            base.OrderByDescending_ThenByDescending();
        }

        public override void OrderBy_ThenBy_Any()
        {
            base.OrderBy_ThenBy_Any();
        }

        public override void OrderBy_Join()
        {
            base.OrderBy_Join();
        }

        public override void OrderBy_SelectMany()
        {
            base.OrderBy_SelectMany();
        }

        public override void Let_any_subquery_anonymous()
        {
            base.Let_any_subquery_anonymous();
        }

        public override void GroupBy_anonymous()
        {
            base.GroupBy_anonymous();
        }

        public override void GroupBy_anonymous_with_where()
        {
            base.GroupBy_anonymous_with_where();
        }

        public override void GroupBy_anonymous_subquery()
        {
            base.GroupBy_anonymous_subquery();
        }

        public override void GroupBy_nested_order_by_enumerable()
        {
            base.GroupBy_nested_order_by_enumerable();
        }

        public override void GroupBy_join_default_if_empty_anonymous()
        {
            base.GroupBy_join_default_if_empty_anonymous();
        }

        public override void GroupBy_SelectMany()
        {
            base.GroupBy_SelectMany();
        }

        public override void GroupBy_simple()
        {
            base.GroupBy_simple();
        }

        public override void GroupBy_simple2()
        {
            base.GroupBy_simple2();
        }

        public override void GroupBy_first()
        {
            base.GroupBy_first();
        }

        public override void GroupBy_Sum()
        {
            base.GroupBy_Sum();
        }

        public override void GroupBy_Count()
        {
            base.GroupBy_Count();
        }

        public override void GroupBy_LongCount()
        {
            base.GroupBy_LongCount();
        }

        public override void GroupBy_Shadow()
        {
            base.GroupBy_Shadow();
        }

        public override void GroupBy_Shadow3()
        {
            base.GroupBy_Shadow3();
        }

        public override void GroupBy_Sum_Min_Max_Avg()
        {
            base.GroupBy_Sum_Min_Max_Avg();
        }

        public override void GroupBy_with_result_selector()
        {
            base.GroupBy_with_result_selector();
        }

        public override void GroupBy_with_element_selector_sum()
        {
            base.GroupBy_with_element_selector_sum();
        }

        public override void GroupBy_with_element_selector()
        {
            base.GroupBy_with_element_selector();
        }

        public override void GroupBy_with_element_selector_sum_max()
        {
            base.GroupBy_with_element_selector_sum_max();
        }

        public override void GroupBy_with_anonymous_element()
        {
            base.GroupBy_with_anonymous_element();
        }

        public override void GroupBy_with_two_part_key()
        {
            base.GroupBy_with_two_part_key();
        }

        public override void GroupBy_DateTimeOffset_Property()
        {
            base.GroupBy_DateTimeOffset_Property();
        }

        public override void OrderBy_GroupBy()
        {
            base.OrderBy_GroupBy();
        }

        public override void OrderBy_GroupBy_SelectMany()
        {
            base.OrderBy_GroupBy_SelectMany();
        }

        public override void OrderBy_GroupBy_SelectMany_shadow()
        {
            base.OrderBy_GroupBy_SelectMany_shadow();
        }

        public override void GroupBy_with_orderby()
        {
            base.GroupBy_with_orderby();
        }

        public override void GroupBy_with_orderby_and_anonymous_projection()
        {
            base.GroupBy_with_orderby_and_anonymous_projection();
        }

        public override void GroupBy_with_orderby_take_skip_distinct()
        {
            base.GroupBy_with_orderby_take_skip_distinct();
        }

        public override void OrderBy_arithmetic()
        {
            base.OrderBy_arithmetic();
        }

        public override void OrderBy_condition_comparison()
        {
            base.OrderBy_condition_comparison();
        }

        public override void OrderBy_ternary_conditions()
        {
            base.OrderBy_ternary_conditions();
        }

        public override void OrderBy_any()
        {
            base.OrderBy_any();
        }

        public override void SelectMany_Joined()
        {
            base.SelectMany_Joined();
        }

        public override void SelectMany_Joined_DefaultIfEmpty()
        {
            base.SelectMany_Joined_DefaultIfEmpty();
        }

        public override void SelectMany_Joined_Take()
        {
            base.SelectMany_Joined_Take();
        }

        public override void SelectMany_Joined_DefaultIfEmpty2()
        {
            base.SelectMany_Joined_DefaultIfEmpty2();
        }

        public override void Select_many_cross_join_same_collection()
        {
            base.Select_many_cross_join_same_collection();
        }

        public override void OrderBy_null_coalesce_operator()
        {
            base.OrderBy_null_coalesce_operator();
        }

        public override void Select_null_coalesce_operator()
        {
            base.Select_null_coalesce_operator();
        }

        public override void OrderBy_conditional_operator()
        {
            base.OrderBy_conditional_operator();
        }

        public override void OrderBy_conditional_operator_where_condition_null()
        {
            base.OrderBy_conditional_operator_where_condition_null();
        }

        public override void OrderBy_comparison_operator()
        {
            base.OrderBy_comparison_operator();
        }

        public override void Projection_null_coalesce_operator()
        {
            base.Projection_null_coalesce_operator();
        }

        public override void Filter_coalesce_operator()
        {
            base.Filter_coalesce_operator();
        }

        public override void Take_skip_null_coalesce_operator()
        {
            base.Take_skip_null_coalesce_operator();
        }

        public override void Select_take_null_coalesce_operator()
        {
            base.Select_take_null_coalesce_operator();
        }

        public override void Select_take_skip_null_coalesce_operator()
        {
            base.Select_take_skip_null_coalesce_operator();
        }

        public override void Select_take_skip_null_coalesce_operator2()
        {
            base.Select_take_skip_null_coalesce_operator2();
        }

        public override void Select_take_skip_null_coalesce_operator3()
        {
            base.Select_take_skip_null_coalesce_operator3();
        }

        public override void Select_Property_when_non_shadow()
        {
            base.Select_Property_when_non_shadow();
        }

        public override void Where_Property_when_non_shadow()
        {
            base.Where_Property_when_non_shadow();
        }

        public override void Select_Property_when_shadow()
        {
            base.Select_Property_when_shadow();
        }

        public override void Where_Property_when_shadow()
        {
            base.Where_Property_when_shadow();
        }

        public override void Select_Property_when_shaow_unconstrained_generic_method()
        {
            base.Select_Property_when_shaow_unconstrained_generic_method();
        }

        public override void Where_Property_when_shaow_unconstrained_generic_method()
        {
            base.Where_Property_when_shaow_unconstrained_generic_method();
        }

        public override void Where_Property_shadow_closure()
        {
            base.Where_Property_shadow_closure();
        }

        public override void Selected_column_can_coalesce()
        {
            base.Selected_column_can_coalesce();
        }

        public override void Can_cast_CreateQuery_result_to_IQueryable_T_bug_1730()
        {
            base.Can_cast_CreateQuery_result_to_IQueryable_T_bug_1730();
        }

        public override void Can_execute_non_generic()
        {
            base.Can_execute_non_generic();
        }

        public override void Select_Subquery_Single()
        {
            base.Select_Subquery_Single();
        }

        public override void Select_Where_Subquery_Deep_Single()
        {
            base.Select_Where_Subquery_Deep_Single();
        }

        public override void Select_Where_Subquery_Deep_First()
        {
            base.Select_Where_Subquery_Deep_First();
        }

        public override void Select_Where_Subquery_Equality()
        {
            base.Select_Where_Subquery_Equality();
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

        public override void Does_not_change_ordering_of_projection_with_complex_projections()
        {
            base.Does_not_change_ordering_of_projection_with_complex_projections();
        }

        public override void DateTime_parse_is_parameterized()
        {
            base.DateTime_parse_is_parameterized();
        }

        public override void Random_next_is_not_funcletized_1()
        {
            base.Random_next_is_not_funcletized_1();
        }

        public override void Random_next_is_not_funcletized_2()
        {
            base.Random_next_is_not_funcletized_2();
        }

        public override void Random_next_is_not_funcletized_3()
        {
            base.Random_next_is_not_funcletized_3();
        }

        public override void Random_next_is_not_funcletized_4()
        {
            base.Random_next_is_not_funcletized_4();
        }

        public override void Random_next_is_not_funcletized_5()
        {
            base.Random_next_is_not_funcletized_5();
        }

        public override void Random_next_is_not_funcletized_6()
        {
            base.Random_next_is_not_funcletized_6();
        }

        public override void Environment_newline_is_funcletized()
        {
            base.Environment_newline_is_funcletized();
        }

        public override void String_concat_with_navigation1()
        {
            base.String_concat_with_navigation1();
        }

        public override void String_concat_with_navigation2()
        {
            base.String_concat_with_navigation2();
        }

        public override void Where_bitwise_or()
        {
            base.Where_bitwise_or();
        }

        public override void Where_bitwise_and()
        {
            base.Where_bitwise_and();
        }

        public override void Select_bitwise_or()
        {
            base.Select_bitwise_or();
        }

        public override void Select_bitwise_or_multiple()
        {
            base.Select_bitwise_or_multiple();
        }

        public override void Select_bitwise_and()
        {
            base.Select_bitwise_and();
        }

        public override void Select_bitwise_and_or()
        {
            base.Select_bitwise_and_or();
        }

        public override void Where_bitwise_or_with_logical_or()
        {
            base.Where_bitwise_or_with_logical_or();
        }

        public override void Where_bitwise_and_with_logical_and()
        {
            base.Where_bitwise_and_with_logical_and();
        }

        public override void Where_bitwise_or_with_logical_and()
        {
            base.Where_bitwise_or_with_logical_and();
        }

        public override void Where_bitwise_and_with_logical_or()
        {
            base.Where_bitwise_and_with_logical_or();
        }

        public override void Select_bitwise_or_with_logical_or()
        {
            base.Select_bitwise_or_with_logical_or();
        }

        public override void Select_bitwise_and_with_logical_and()
        {
            base.Select_bitwise_and_with_logical_and();
        }

        public override void Handle_materialization_properly_when_more_than_two_query_sources_are_involved()
        {
            base.Handle_materialization_properly_when_more_than_two_query_sources_are_involved();
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

        public override void Parameter_extraction_can_throw_exception_from_user_code_2()
        {
            base.Parameter_extraction_can_throw_exception_from_user_code_2();
        }

        public override void Subquery_member_pushdown_does_not_change_original_subquery_model()
        {
            base.Subquery_member_pushdown_does_not_change_original_subquery_model();
        }

        public override void Query_expression_with_to_string_and_contains()
        {
            base.Query_expression_with_to_string_and_contains();
        }

        public override void Select_expression_other_to_string()
        {
            base.Select_expression_other_to_string();
        }

        public override void Select_expression_long_to_string()
        {
            base.Select_expression_long_to_string();
        }

        public override void Select_expression_int_to_string()
        {
            base.Select_expression_int_to_string();
        }

        public override void ToString_with_formatter_is_evaluated_on_the_client()
        {
            base.ToString_with_formatter_is_evaluated_on_the_client();
        }

        public override void Select_expression_date_add_year()
        {
            base.Select_expression_date_add_year();
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

        public override void Select_expression_date_add_milliseconds_large_number_divided()
        {
            base.Select_expression_date_add_milliseconds_large_number_divided();
        }

        public override void Select_expression_references_are_updated_correctly_with_subquery()
        {
            base.Select_expression_references_are_updated_correctly_with_subquery();
        }

        public override void DefaultIfEmpty_without_group_join()
        {
            base.DefaultIfEmpty_without_group_join();
        }

        public override void DefaultIfEmpty_in_subquery()
        {
            base.DefaultIfEmpty_in_subquery();
        }

        public override void DefaultIfEmpty_in_subquery_nested()
        {
            base.DefaultIfEmpty_in_subquery_nested();
        }

        public override void OrderBy_skip_take()
        {
            base.OrderBy_skip_take();
        }

        public override void OrderBy_skip_take_take()
        {
            base.OrderBy_skip_take_take();
        }

        public override void OrderBy_skip_take_take_take_take()
        {
            base.OrderBy_skip_take_take_take_take();
        }

        public override void OrderBy_skip_take_skip_take_skip()
        {
            base.OrderBy_skip_take_skip_take_skip();
        }

        public override void OrderBy_skip_take_distinct()
        {
            base.OrderBy_skip_take_distinct();
        }

        public override void OrderBy_coalesce_take_distinct()
        {
            base.OrderBy_coalesce_take_distinct();
        }

        public override void OrderBy_coalesce_skip_take_distinct()
        {
            base.OrderBy_coalesce_skip_take_distinct();
        }

        public override void OrderBy_coalesce_skip_take_distinct_take()
        {
            base.OrderBy_coalesce_skip_take_distinct_take();
        }

        public override void OrderBy_skip_take_distinct_orderby_take()
        {
            base.OrderBy_skip_take_distinct_orderby_take();
        }

        public override void No_orderby_added_for_fully_translated_manually_constructed_LOJ()
        {
            base.No_orderby_added_for_fully_translated_manually_constructed_LOJ();
        }

        public override void Contains_with_DateTime_Date()
        {
            base.Contains_with_DateTime_Date();
        }

        public override void Contains_with_subquery_involving_join_binds_to_correct_table()
        {
            base.Contains_with_subquery_involving_join_binds_to_correct_table();
        }

        public override void Anonymous_member_distinct_where()
        {
            base.Anonymous_member_distinct_where();
        }

        public override void Anonymous_member_distinct_orderby()
        {
            base.Anonymous_member_distinct_orderby();
        }

        public override void Anonymous_member_distinct_result()
        {
            base.Anonymous_member_distinct_result();
        }

        public override void Anonymous_complex_distinct_where()
        {
            base.Anonymous_complex_distinct_where();
        }

        public override void Anonymous_complex_distinct_orderby()
        {
            base.Anonymous_complex_distinct_orderby();
        }

        public override void Anonymous_complex_distinct_result()
        {
            base.Anonymous_complex_distinct_result();
        }

        public override void Anonymous_complex_orderby()
        {
            base.Anonymous_complex_orderby();
        }

        public override void Anonymous_subquery_orderby()
        {
            base.Anonymous_subquery_orderby();
        }

        public override void DTO_member_distinct_where()
        {
            base.DTO_member_distinct_where();
        }

        public override void DTO_member_distinct_orderby()
        {
            base.DTO_member_distinct_orderby();
        }

        public override void DTO_member_distinct_result()
        {
            base.DTO_member_distinct_result();
        }

        public override void DTO_complex_distinct_where()
        {
            base.DTO_complex_distinct_where();
        }

        public override void DTO_complex_distinct_orderby()
        {
            base.DTO_complex_distinct_orderby();
        }

        public override void DTO_complex_distinct_result()
        {
            base.DTO_complex_distinct_result();
        }

        public override void DTO_complex_orderby()
        {
            base.DTO_complex_orderby();
        }

        public override void DTO_subquery_orderby()
        {
            base.DTO_subquery_orderby();
        }

        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override void Include_with_orderby_skip_preserves_ordering()
        {
            base.Include_with_orderby_skip_preserves_ordering();
        }

        public override void Complex_query_with_repeated_query_model_compiles_correctly()
        {
            base.Complex_query_with_repeated_query_model_compiles_correctly();
        }

        public override void Complex_query_with_repeated_nested_query_model_compiles_correctly()
        {
            base.Complex_query_with_repeated_nested_query_model_compiles_correctly();
        }

        public override void GroupBy_join_anonymous()
        {
            base.GroupBy_join_anonymous();
        }

        public override void Int16_parameter_can_be_used_for_int_column()
        {
            base.Int16_parameter_can_be_used_for_int_column();
        }

        public override void Subquery_is_null_translated_correctly()
        {
            base.Subquery_is_null_translated_correctly();
        }

        public override void Subquery_is_not_null_translated_correctly()
        {
            base.Subquery_is_not_null_translated_correctly();
        }

        public override void Select_take_average()
        {
            base.Select_take_average();
        }

        public override void Select_take_count()
        {
            base.Select_take_count();
        }

        public override void Select_orderBy_take_count()
        {
            base.Select_orderBy_take_count();
        }

        public override void Select_take_long_count()
        {
            base.Select_take_long_count();
        }

        public override void Select_orderBy_take_long_count()
        {
            base.Select_orderBy_take_long_count();
        }

        public override void Select_take_max()
        {
            base.Select_take_max();
        }

        public override void Select_take_min()
        {
            base.Select_take_min();
        }

        public override void Select_take_sum()
        {
            base.Select_take_sum();
        }

        public override void Select_skip_average()
        {
            base.Select_skip_average();
        }

        public override void Select_skip_count()
        {
            base.Select_skip_count();
        }

        public override void Select_orderBy_skip_count()
        {
            base.Select_orderBy_skip_count();
        }

        public override void Select_skip_long_count()
        {
            base.Select_skip_long_count();
        }

        public override void Select_orderBy_skip_long_count()
        {
            base.Select_orderBy_skip_long_count();
        }

        public override void Select_skip_max()
        {
            base.Select_skip_max();
        }

        public override void Select_skip_min()
        {
            base.Select_skip_min();
        }

        public override void Select_skip_sum()
        {
            base.Select_skip_sum();
        }

        public override void Select_distinct_average()
        {
            base.Select_distinct_average();
        }

        public override void Select_distinct_count()
        {
            base.Select_distinct_count();
        }

        public override void Select_distinct_long_count()
        {
            base.Select_distinct_long_count();
        }

        public override void Select_distinct_max()
        {
            base.Select_distinct_max();
        }

        public override void Select_distinct_min()
        {
            base.Select_distinct_min();
        }

        public override void Select_distinct_sum()
        {
            base.Select_distinct_sum();
        }

        public override void Comparing_to_fixed_string_parameter()
        {
            base.Comparing_to_fixed_string_parameter();
        }

        public override void Comparing_entities_using_Equals()
        {
            base.Comparing_entities_using_Equals();
        }

        public override void Comparing_different_entity_types_using_Equals()
        {
            base.Comparing_different_entity_types_using_Equals();
        }

        public override void Comparing_entity_to_null_using_Equals()
        {
            base.Comparing_entity_to_null_using_Equals();
        }

        public override void Comparing_navigations_using_Equals()
        {
            base.Comparing_navigations_using_Equals();
        }

        public override void Comparing_navigations_using_static_Equals()
        {
            base.Comparing_navigations_using_static_Equals();
        }

        public override void Comparing_non_matching_entities_using_Equals()
        {
            base.Comparing_non_matching_entities_using_Equals();
        }

        public override void Comparing_non_matching_collection_navigations_using_Equals()
        {
            base.Comparing_non_matching_collection_navigations_using_Equals();
        }

        public override void Comparing_collection_navigation_to_null()
        {
            base.Comparing_collection_navigation_to_null();
        }

        public override void Comparing_collection_navigation_to_null_complex()
        {
            base.Comparing_collection_navigation_to_null_complex();
        }

        public override void Compare_collection_navigation_with_itself()
        {
            base.Compare_collection_navigation_with_itself();
        }

        public override void Compare_two_collection_navigations_with_different_query_sources()
        {
            base.Compare_two_collection_navigations_with_different_query_sources();
        }

        public override void Compare_two_collection_navigations_using_equals()
        {
            base.Compare_two_collection_navigations_using_equals();
        }

        public override void Compare_two_collection_navigations_with_different_property_chains()
        {
            base.Compare_two_collection_navigations_with_different_property_chains();
        }

        public override void OrderBy_ThenBy_same_column_different_direction()
        {
            base.OrderBy_ThenBy_same_column_different_direction();
        }

        public override void OrderBy_OrderBy_same_column_different_direction()
        {
            base.OrderBy_OrderBy_same_column_different_direction();
        }

        protected override void ClearLog()
        {
            base.ClearLog();
        }

        public override void String_StartsWith_Literal()
        {
            base.String_StartsWith_Literal();
        }

        public override void String_StartsWith_Identity()
        {
            base.String_StartsWith_Identity();
        }

        public override void String_StartsWith_Column()
        {
            base.String_StartsWith_Column();
        }

        public override void String_StartsWith_MethodCall()
        {
            base.String_StartsWith_MethodCall();
        }

        public override void String_EndsWith_Literal()
        {
            base.String_EndsWith_Literal();
        }

        public override void String_EndsWith_Identity()
        {
            base.String_EndsWith_Identity();
        }

        public override void String_EndsWith_Column()
        {
            base.String_EndsWith_Column();
        }

        public override void String_EndsWith_MethodCall()
        {
            base.String_EndsWith_MethodCall();
        }

        public override void String_Contains_Literal()
        {
            // Overridden; EFCore SQL Server test does this too.
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);
        }

        public override void String_Contains_Identity()
        {
            base.String_Contains_Identity();
        }

        public override void String_Contains_Column()
        {
            base.String_Contains_Column();
        }

        public override void String_Contains_MethodCall()
        {
            base.String_Contains_MethodCall();
        }

        public override void String_Compare_simple_zero()
        {
            base.String_Compare_simple_zero();
        }

        public override void String_Compare_simple_one()
        {
            base.String_Compare_simple_one();
        }

        public override void String_compare_with_parameter()
        {
            base.String_compare_with_parameter();
        }

        public override void String_Compare_simple_client()
        {
            base.String_Compare_simple_client();
        }

        public override void String_Compare_nested()
        {
            base.String_Compare_nested();
        }

        public override void String_Compare_multi_predicate()
        {
            base.String_Compare_multi_predicate();
        }

        public override void String_Compare_to_simple_zero()
        {
            base.String_Compare_to_simple_zero();
        }

        public override void String_Compare_to_simple_one()
        {
            base.String_Compare_to_simple_one();
        }

        public override void String_compare_to_with_parameter()
        {
            base.String_compare_to_with_parameter();
        }

        public override void String_Compare_to_simple_client()
        {
            base.String_Compare_to_simple_client();
        }

        public override void String_Compare_to_nested()
        {
            base.String_Compare_to_nested();
        }

        public override void String_Compare_to_multi_predicate()
        {
            base.String_Compare_to_multi_predicate();
        }

        public override void Where_math_abs1()
        {
            base.Where_math_abs1();
        }

        public override void Where_math_abs2()
        {
            base.Where_math_abs2();
        }

        public override void Where_math_abs3()
        {
            base.Where_math_abs3();
        }

        public override void Where_math_abs_uncorrelated()
        {
            base.Where_math_abs_uncorrelated();
        }

        public override void Where_math_ceiling1()
        {
            base.Where_math_ceiling1();
        }

        public override void Where_math_ceiling2()
        {
            base.Where_math_ceiling2();
        }

        public override void Where_math_floor()
        {
            base.Where_math_floor();
        }

        public override void Where_math_power()
        {
            base.Where_math_power();
        }

        public override void Where_math_round()
        {
            base.Where_math_round();
        }

        public override void Select_math_round_int()
        {
            base.Select_math_round_int();
        }

        public override void Select_math_truncate_int()
        {
            base.Select_math_truncate_int();
        }

        public override void Where_math_round2()
        {
            base.Where_math_round2();
        }

        public override void Where_math_truncate()
        {
            base.Where_math_truncate();
        }

        public override void Where_math_exp()
        {
            base.Where_math_exp();
        }

        public override void Where_math_log10()
        {
            base.Where_math_log10();
        }

        public override void Where_math_log()
        {
            base.Where_math_log();
        }

        public override void Where_math_log_new_base()
        {
            base.Where_math_log_new_base();
        }

        public override void Where_math_sqrt()
        {
            base.Where_math_sqrt();
        }

        public override void Where_math_acos()
        {
            base.Where_math_acos();
        }

        public override void Where_math_asin()
        {
            base.Where_math_asin();
        }

        public override void Where_math_atan()
        {
            base.Where_math_atan();
        }

        public override void Where_math_atan2()
        {
            base.Where_math_atan2();
        }

        public override void Where_math_cos()
        {
            base.Where_math_cos();
        }

        public override void Where_math_sin()
        {
            base.Where_math_sin();
        }

        public override void Where_math_tan()
        {
            base.Where_math_tan();
        }

        public override void Where_math_sign()
        {
            base.Where_math_sign();
        }

        public override void Where_guid_newguid()
        {
            base.Where_guid_newguid();
        }

        public override void Where_string_to_upper()
        {
            base.Where_string_to_upper();
        }

        public override void Where_string_to_lower()
        {
            base.Where_string_to_lower();
        }

        public override void Where_functions_nested()
        {
            base.Where_functions_nested();
        }

        public override void Convert_ToByte()
        {
            base.Convert_ToByte();
        }

        public override void Convert_ToDecimal()
        {
            base.Convert_ToDecimal();
        }

        public override void Convert_ToDouble()
        {
            base.Convert_ToDouble();
        }

        public override void Convert_ToInt16()
        {
            base.Convert_ToInt16();
        }

        public override void Convert_ToInt32()
        {
            base.Convert_ToInt32();
        }

        public override void Convert_ToInt64()
        {
            base.Convert_ToInt64();
        }

        public override void Convert_ToString()
        {
            base.Convert_ToString();
        }

        public override void Substring_with_constant()
        {
            base.Substring_with_constant();
        }

        public override void Substring_with_closure()
        {
            base.Substring_with_closure();
        }

        public override void Substring_with_client_eval()
        {
            base.Substring_with_client_eval();
        }

        public override void IsNullOrEmpty_in_predicate()
        {
            base.IsNullOrEmpty_in_predicate();
        }

        public override void IsNullOrEmpty_in_projection()
        {
            base.IsNullOrEmpty_in_projection();
        }

        public override void IsNullOrEmpty_negated_in_projection()
        {
            base.IsNullOrEmpty_negated_in_projection();
        }

        public override void IsNullOrWhiteSpace_in_predicate()
        {
            base.IsNullOrWhiteSpace_in_predicate();
        }

        public override void TrimStart_without_arguments_in_predicate()
        {
            base.TrimStart_without_arguments_in_predicate();
        }

        public override void TrimStart_with_char_argument_in_predicate()
        {
            base.TrimStart_with_char_argument_in_predicate();
        }

        public override void TrimStart_with_char_array_argument_in_predicate()
        {
            base.TrimStart_with_char_array_argument_in_predicate();
        }

        public override void TrimEnd_without_arguments_in_predicate()
        {
            base.TrimEnd_without_arguments_in_predicate();
        }

        public override void TrimEnd_with_char_argument_in_predicate()
        {
            base.TrimEnd_with_char_argument_in_predicate();
        }

        public override void TrimEnd_with_char_array_argument_in_predicate()
        {
            base.TrimEnd_with_char_array_argument_in_predicate();
        }

        public override void Trim_without_argument_in_predicate()
        {
            base.Trim_without_argument_in_predicate();
        }

        public override void Trim_with_char_argument_in_predicate()
        {
            base.Trim_with_char_argument_in_predicate();
        }

        public override void Trim_with_char_array_argument_in_predicate()
        {
            base.Trim_with_char_array_argument_in_predicate();
        }

        public override void Join_customers_orders_projection()
        {
            base.Join_customers_orders_projection();
        }

        public override void Join_customers_orders_entities()
        {
            base.Join_customers_orders_entities();
        }

        public override void Join_select_many()
        {
            base.Join_select_many();
        }

        public override void Client_Join_select_many()
        {
            base.Client_Join_select_many();
        }

        public override void Join_customers_orders_select()
        {
            base.Join_customers_orders_select();
        }

        public override void Join_customers_orders_with_subquery()
        {
            base.Join_customers_orders_with_subquery();
        }

        public override void Join_customers_orders_with_subquery_with_take()
        {
            base.Join_customers_orders_with_subquery_with_take();
        }

        public override void Join_customers_orders_with_subquery_anonymous_property_method_with_take()
        {
            base.Join_customers_orders_with_subquery_anonymous_property_method_with_take();
        }

        public override void Join_customers_orders_with_subquery_predicate()
        {
            base.Join_customers_orders_with_subquery_predicate();
        }

        public override void Join_customers_orders_with_subquery_predicate_with_take()
        {
            base.Join_customers_orders_with_subquery_predicate_with_take();
        }

        public override void Join_composite_key()
        {
            base.Join_composite_key();
        }

        public override void Join_complex_condition()
        {
            base.Join_complex_condition();
        }

        public override void Join_local_collection_int_closure_is_cached_correctly()
        {
            base.Join_local_collection_int_closure_is_cached_correctly();
        }

        public override void Join_local_string_closure_is_cached_correctly()
        {
            base.Join_local_string_closure_is_cached_correctly();
        }

        public override void Join_local_bytes_closure_is_cached_correctly()
        {
            base.Join_local_bytes_closure_is_cached_correctly();
        }

        public override void Join_same_collection_multiple()
        {
            base.Join_same_collection_multiple();
        }

        public override void Join_same_collection_force_alias_uniquefication()
        {
            base.Join_same_collection_force_alias_uniquefication();
        }

        public override void GroupJoin_customers_orders_count()
        {
            base.GroupJoin_customers_orders_count();
        }

        public override void GroupJoin_customers_orders_count_preserves_ordering()
        {
            base.GroupJoin_customers_orders_count_preserves_ordering();
        }

        public override void GroupJoin_customers_employees_shadow()
        {
            base.GroupJoin_customers_employees_shadow();
        }

        public override void GroupJoin_customers_employees_subquery_shadow()
        {
            base.GroupJoin_customers_employees_subquery_shadow();
        }

        public override void GroupJoin_customers_employees_subquery_shadow_take()
        {
            base.GroupJoin_customers_employees_subquery_shadow_take();
        }

        public override void GroupJoin_simple()
        {
            base.GroupJoin_simple();
        }

        public override void GroupJoin_simple2()
        {
            base.GroupJoin_simple2();
        }

        public override void GroupJoin_simple3()
        {
            base.GroupJoin_simple3();
        }
        
        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override void GroupJoin_tracking_groups()
        {
            base.GroupJoin_tracking_groups();
        }

        public override void GroupJoin_tracking_groups2()
        {
            base.GroupJoin_tracking_groups2();
        }

        public override void GroupJoin_simple_ordering()
        {
            base.GroupJoin_simple_ordering();
        }

        public override void GroupJoin_simple_subquery()
        {
            base.GroupJoin_simple_subquery();
        }

        public override void GroupJoin_projection()
        {
            base.GroupJoin_projection();
        }

        public override void GroupJoin_outer_projection()
        {
            base.GroupJoin_outer_projection();
        }

        public override void GroupJoin_outer_projection2()
        {
            base.GroupJoin_outer_projection2();
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

        public override void GroupJoin_outer_projection_reverse()
        {
            base.GroupJoin_outer_projection_reverse();
        }

        public override void GroupJoin_outer_projection_reverse2()
        {
            base.GroupJoin_outer_projection_reverse2();
        }

        public override void GroupJoin_subquery_projection_outer_mixed()
        {
            base.GroupJoin_subquery_projection_outer_mixed();
        }

        public override void GroupJoin_DefaultIfEmpty()
        {
            base.GroupJoin_DefaultIfEmpty();
        }

        public override void GroupJoin_DefaultIfEmpty_multiple()
        {
            base.GroupJoin_DefaultIfEmpty_multiple();
        }

        public override void GroupJoin_DefaultIfEmpty2()
        {
            base.GroupJoin_DefaultIfEmpty2();
        }

        public override void GroupJoin_DefaultIfEmpty3()
        {
            base.GroupJoin_DefaultIfEmpty3();
        }

        public override void GroupJoin_Where()
        {
            base.GroupJoin_Where();
        }

        public override void GroupJoin_Where_OrderBy()
        {
            base.GroupJoin_Where_OrderBy();
        }

        public override void GroupJoin_DefaultIfEmpty_Where()
        {
            base.GroupJoin_DefaultIfEmpty_Where();
        }

        public override void Join_GroupJoin_DefaultIfEmpty_Where()
        {
            base.Join_GroupJoin_DefaultIfEmpty_Where();
        }

        public override void GroupJoin_DefaultIfEmpty_Project()
        {
            base.GroupJoin_DefaultIfEmpty_Project();
        }

        public override void GroupJoin_with_different_outer_elements_with_same_key()
        {
            base.GroupJoin_with_different_outer_elements_with_same_key();
        }

        public override void GroupJoin_with_different_outer_elements_with_same_key_with_predicate()
        {
            base.GroupJoin_with_different_outer_elements_with_same_key_with_predicate();
        }

        public override void GroupJoin_with_different_outer_elements_with_same_key_projected_from_another_entity()
        {
            base.GroupJoin_with_different_outer_elements_with_same_key_projected_from_another_entity();
        }

        public override void GroupJoin_SelectMany_subquery_with_filter()
        {
            base.GroupJoin_SelectMany_subquery_with_filter();
        }

        public override void GroupJoin_SelectMany_subquery_with_filter_orderby()
        {
            base.GroupJoin_SelectMany_subquery_with_filter_orderby();
        }

        public override void GroupJoin_SelectMany_subquery_with_filter_and_DefaultIfEmpty()
        {
            base.GroupJoin_SelectMany_subquery_with_filter_and_DefaultIfEmpty();
        }

        public override void GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty()
        {
            base.GroupJoin_SelectMany_subquery_with_filter_orderby_and_DefaultIfEmpty();
        }

        public override void GroupJoin_with_order_by_key_descending1()
        {
            base.GroupJoin_with_order_by_key_descending1();
        }

        public override void GroupJoin_with_order_by_key_descending2()
        {
            base.GroupJoin_with_order_by_key_descending2();
        }

        public override void Select_All()
        {
            base.Select_All();
        }

        public override void Select_GroupBy_All()
        {
            base.Select_GroupBy_All();
        }

        public override void Select_GroupBy()
        {
            base.Select_GroupBy();
        }

        public override void Select_GroupBy_SelectMany()
        {
            base.Select_GroupBy_SelectMany();
        }

        public override void Sum_with_no_arg()
        {
            base.Sum_with_no_arg();
        }

        public override void Sum_with_binary_expression()
        {
            base.Sum_with_binary_expression();
        }

        public override void Sum_with_no_arg_empty()
        {
            base.Sum_with_no_arg_empty();
        }

        public override void Sum_with_arg()
        {
            base.Sum_with_arg();
        }

        public override void Sum_with_arg_expression()
        {
            base.Sum_with_arg_expression();
        }

        public override void Sum_with_division_on_decimal()
        {
            base.Sum_with_division_on_decimal();
        }

        public override void Sum_with_division_on_decimal_no_significant_digits()
        {
            base.Sum_with_division_on_decimal_no_significant_digits();
        }

        public override void Sum_with_coalesce()
        {
            base.Sum_with_coalesce();
        }

        public override void Sum_over_subquery_is_client_eval()
        {
            base.Sum_over_subquery_is_client_eval();
        }

        public override void Sum_on_float_column()
        {
            base.Sum_on_float_column();
        }

        public override void Sum_on_float_column_in_subquery()
        {
            base.Sum_on_float_column_in_subquery();
        }

        public override void Average_with_no_arg()
        {
            base.Average_with_no_arg();
        }

        public override void Average_with_binary_expression()
        {
            base.Average_with_binary_expression();
        }

        public override void Average_with_arg()
        {
            base.Average_with_arg();
        }

        public override void Average_with_arg_expression()
        {
            base.Average_with_arg_expression();
        }

        public override void Average_with_division_on_decimal()
        {
            base.Average_with_division_on_decimal();
        }

        public override void Average_with_coalesce()
        {
            base.Average_with_coalesce();
        }

        public override void Average_over_subquery_is_client_eval()
        {
            base.Average_over_subquery_is_client_eval();
        }

        public override void Average_on_float_column()
        {
            base.Average_on_float_column();
        }

        public override void Average_on_float_column_in_subquery()
        {
            base.Average_on_float_column_in_subquery();
        }

        public override void Average_on_float_column_in_subquery_with_cast()
        {
            base.Average_on_float_column_in_subquery_with_cast();
        }

        public override void Min_with_no_arg()
        {
            base.Min_with_no_arg();
        }

        public override void Min_with_arg()
        {
            base.Min_with_arg();
        }

        public override void Min_with_coalesce()
        {
            base.Min_with_coalesce();
        }

        public override void Min_over_subquery_is_client_eval()
        {
            base.Min_over_subquery_is_client_eval();
        }

        public override void Max_with_no_arg()
        {
            base.Max_with_no_arg();
        }

        public override void Max_with_arg()
        {
            base.Max_with_arg();
        }

        public override void Max_with_coalesce()
        {
            base.Max_with_coalesce();
        }

        public override void Max_over_subquery_is_client_eval()
        {
            base.Max_over_subquery_is_client_eval();
        }

        public override void Count_with_no_predicate()
        {
            base.Count_with_no_predicate();
        }

        public override void Count_with_predicate()
        {
            base.Count_with_predicate();
        }

        public override void Count_with_order_by()
        {
            base.Count_with_order_by();
        }

        public override void Where_OrderBy_Count()
        {
            base.Where_OrderBy_Count();
        }

        public override void OrderBy_Where_Count()
        {
            base.OrderBy_Where_Count();
        }

        public override void OrderBy_Count_with_predicate()
        {
            base.OrderBy_Count_with_predicate();
        }

        public override void OrderBy_Where_Count_with_predicate()
        {
            base.OrderBy_Where_Count_with_predicate();
        }

        public override void Where_OrderBy_Count_client_eval()
        {
            base.Where_OrderBy_Count_client_eval();
        }

        public override void Where_OrderBy_Count_client_eval_mixed()
        {
            base.Where_OrderBy_Count_client_eval_mixed();
        }

        public override void OrderBy_Where_Count_client_eval()
        {
            base.OrderBy_Where_Count_client_eval();
        }

        public override void OrderBy_Where_Count_client_eval_mixed()
        {
            base.OrderBy_Where_Count_client_eval_mixed();
        }

        public override void OrderBy_Count_with_predicate_client_eval()
        {
            base.OrderBy_Count_with_predicate_client_eval();
        }

        public override void OrderBy_Count_with_predicate_client_eval_mixed()
        {
            base.OrderBy_Count_with_predicate_client_eval_mixed();
        }

        public override void OrderBy_Where_Count_with_predicate_client_eval()
        {
            base.OrderBy_Where_Count_with_predicate_client_eval();
        }

        public override void OrderBy_Where_Count_with_predicate_client_eval_mixed()
        {
            base.OrderBy_Where_Count_with_predicate_client_eval_mixed();
        }

        public override void OrderBy_client_Take()
        {
            base.OrderBy_client_Take();
        }

        public override void Distinct()
        {
            base.Distinct();
        }

        public override void Distinct_Scalar()
        {
            base.Distinct_Scalar();
        }

        public override void OrderBy_Distinct()
        {
            base.OrderBy_Distinct();
        }

        public override void Distinct_OrderBy()
        {
            base.Distinct_OrderBy();
        }

        public override void Distinct_OrderBy2()
        {
            base.Distinct_OrderBy2();
        }

        public override void Distinct_OrderBy3()
        {
            base.Distinct_OrderBy3();
        }

        public override void Distinct_GroupBy()
        {
            base.Distinct_GroupBy();
        }

        public override void GroupBy_Distinct()
        {
            base.GroupBy_Distinct();
        }

        public override void Distinct_Count()
        {
            base.Distinct_Count();
        }

        public override void Select_Select_Distinct_Count()
        {
            base.Select_Select_Distinct_Count();
        }

        public override void Single_Throws()
        {
            base.Single_Throws();
        }

        public override void Single_Predicate()
        {
            base.Single_Predicate();
        }

        public override void Where_Single()
        {
            base.Where_Single();
        }

        public override void SingleOrDefault_Throws()
        {
            base.SingleOrDefault_Throws();
        }

        public override void SingleOrDefault_Predicate()
        {
            base.SingleOrDefault_Predicate();
        }

        public override void Where_SingleOrDefault()
        {
            base.Where_SingleOrDefault();
        }

        public override void First()
        {
            base.First();
        }

        public override void First_Predicate()
        {
            base.First_Predicate();
        }

        public override void Where_First()
        {
            base.Where_First();
        }

        public override void FirstOrDefault()
        {
            base.FirstOrDefault();
        }

        public override void FirstOrDefault_Predicate()
        {
            base.FirstOrDefault_Predicate();
        }

        public override void Where_FirstOrDefault()
        {
            base.Where_FirstOrDefault();
        }

        public override void FirstOrDefault_inside_subquery_gets_server_evaluated()
        {
            base.FirstOrDefault_inside_subquery_gets_server_evaluated();
        }

        public override void First_inside_subquery_gets_client_evaluated()
        {
            base.First_inside_subquery_gets_client_evaluated();
        }

        public override void Last()
        {
            base.Last();
        }

        public override void Last_when_no_order_by()
        {
            base.Last_when_no_order_by();
        }

        public override void Last_Predicate()
        {
            base.Last_Predicate();
        }

        public override void Where_Last()
        {
            base.Where_Last();
        }

        public override void LastOrDefault()
        {
            base.LastOrDefault();
        }

        public override void LastOrDefault_Predicate()
        {
            base.LastOrDefault_Predicate();
        }

        public override void Where_LastOrDefault()
        {
            base.Where_LastOrDefault();
        }

        public override void Contains_with_subquery()
        {
            base.Contains_with_subquery();
        }

        public override void Contains_with_local_array_closure()
        {
            base.Contains_with_local_array_closure();
        }

        public override void Contains_with_subquery_and_local_array_closure()
        {
            base.Contains_with_subquery_and_local_array_closure();
        }

        public override void Contains_with_local_int_array_closure()
        {
            base.Contains_with_local_int_array_closure();
        }

        public override void Contains_with_local_nullable_int_array_closure()
        {
            base.Contains_with_local_nullable_int_array_closure();
        }

        public override void Contains_with_local_array_inline()
        {
            base.Contains_with_local_array_inline();
        }

        public override void Contains_with_local_list_closure()
        {
            base.Contains_with_local_list_closure();
        }

        public override void Contains_with_local_list_inline()
        {
            base.Contains_with_local_list_inline();
        }

        public override void Contains_with_local_list_inline_closure_mix()
        {
            base.Contains_with_local_list_inline_closure_mix();
        }

        public override void Contains_with_local_collection_false()
        {
            base.Contains_with_local_collection_false();
        }

        public override void Contains_with_local_collection_complex_predicate_and()
        {
            base.Contains_with_local_collection_complex_predicate_and();
        }

        public override void Contains_with_local_collection_complex_predicate_or()
        {
            base.Contains_with_local_collection_complex_predicate_or();
        }

        public override void Contains_with_local_collection_complex_predicate_not_matching_ins1()
        {
            base.Contains_with_local_collection_complex_predicate_not_matching_ins1();
        }

        public override void Contains_with_local_collection_complex_predicate_not_matching_ins2()
        {
            base.Contains_with_local_collection_complex_predicate_not_matching_ins2();
        }

        public override void Contains_with_local_collection_sql_injection()
        {
            base.Contains_with_local_collection_sql_injection();
        }

        public override void Contains_with_local_collection_empty_closure()
        {
            base.Contains_with_local_collection_empty_closure();
        }

        public override void Contains_with_local_collection_empty_inline()
        {
            base.Contains_with_local_collection_empty_inline();
        }

        public override void Contains_top_level()
        {
            base.Contains_top_level();
        }

        public override void OfType_Select()
        {
            base.OfType_Select();
        }

        public override void OfType_Select_OfType_Select()
        {
            base.OfType_Select_OfType_Select();
        }

        public override void Concat_dbset()
        {
            base.Concat_dbset();
        }

        public override void Concat_simple()
        {
            base.Concat_simple();
        }

        public override void Concat_nested()
        {
            base.Concat_nested();
        }

        public override void Concat_non_entity()
        {
            base.Concat_non_entity();
        }

        public override void Except_dbset()
        {
            base.Except_dbset();
        }

        public override void Except_simple()
        {
            base.Except_simple();
        }

        public override void Except_nested()
        {
            base.Except_nested();
        }

        public override void Except_non_entity()
        {
            base.Except_non_entity();
        }

        public override void Intersect_dbset()
        {
            base.Intersect_dbset();
        }

        public override void Intersect_simple()
        {
            base.Intersect_simple();
        }

        public override void Intersect_nested()
        {
            base.Intersect_nested();
        }

        public override void Intersect_non_entity()
        {
            base.Intersect_non_entity();
        }

        public override void Union_dbset()
        {
            base.Union_dbset();
        }

        public override void Union_simple()
        {
            base.Union_simple();
        }

        public override void Union_simple_groupby()
        {
            base.Union_simple_groupby();
        }

        public override void Union_nested()
        {
            base.Union_nested();
        }

        public override void Union_non_entity()
        {
            base.Union_non_entity();
        }

        public override void Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast()
        {
            base.Average_with_non_matching_types_in_projection_doesnt_produce_second_explicit_cast();
        }

        public override void Select_into()
        {
            base.Select_into();
        }

        public override void Projection_when_arithmetic_expression_precendence()
        {
            base.Projection_when_arithmetic_expression_precendence();
        }

        public override void Projection_when_null_value()
        {
            base.Projection_when_null_value();
        }

        public override void Projection_when_client_evald_subquery()
        {
            base.Projection_when_client_evald_subquery();
        }

        public override void Project_to_object_array()
        {
            base.Project_to_object_array();
        }

        public override void Project_to_int_array()
        {
            base.Project_to_int_array();
        }

        public override void Select_bool_closure()
        {
            base.Select_bool_closure();
        }

        public override void Select_scalar()
        {
            base.Select_scalar();
        }

        public override void Select_anonymous_one()
        {
            base.Select_anonymous_one();
        }

        public override void Select_anonymous_two()
        {
            base.Select_anonymous_two();
        }

        public override void Select_anonymous_three()
        {
            base.Select_anonymous_three();
        }

        public override void Select_anonymous_bool_constant_true()
        {
            base.Select_anonymous_bool_constant_true();
        }

        public override void Select_anonymous_constant_in_expression()
        {
            base.Select_anonymous_constant_in_expression();
        }

        public override void Select_anonymous_conditional_expression()
        {
            base.Select_anonymous_conditional_expression();
        }

        public override void Select_customer_table()
        {
            base.Select_customer_table();
        }

        public override void Select_customer_identity()
        {
            base.Select_customer_identity();
        }

        public override void Select_anonymous_with_object()
        {
            base.Select_anonymous_with_object();
        }

        public override void Select_anonymous_nested()
        {
            base.Select_anonymous_nested();
        }

        public override void Select_anonymous_empty()
        {
            base.Select_anonymous_empty();
        }

        public override void Select_anonymous_literal()
        {
            base.Select_anonymous_literal();
        }

        public override void Select_constant_int()
        {
            base.Select_constant_int();
        }

        public override void Select_constant_null_string()
        {
            base.Select_constant_null_string();
        }

        public override void Select_local()
        {
            base.Select_local();
        }

        public override void Select_scalar_primitive()
        {
            base.Select_scalar_primitive();
        }

        public override void Select_scalar_primitive_after_take()
        {
            base.Select_scalar_primitive_after_take();
        }

        public override void Select_project_filter()
        {
            base.Select_project_filter();
        }

        public override void Select_project_filter2()
        {
            base.Select_project_filter2();
        }

        public override void Select_nested_collection()
        {
            base.Select_nested_collection();
        }

        public override void Select_nested_collection_multi_level()
        {
            base.Select_nested_collection_multi_level();
        }

        public override void Select_nested_collection_multi_level2()
        {
            base.Select_nested_collection_multi_level2();
        }

        public override void Select_nested_collection_multi_level3()
        {
            base.Select_nested_collection_multi_level3();
        }

        public override void Select_nested_collection_multi_level4()
        {
            base.Select_nested_collection_multi_level4();
        }

        public override void Select_nested_collection_multi_level5()
        {
            base.Select_nested_collection_multi_level5();
        }

        public override void Select_nested_collection_multi_level6()
        {
            base.Select_nested_collection_multi_level6();
        }

        public override void Select_nested_collection_with_groupby()
        {
            base.Select_nested_collection_with_groupby();
        }

        public override void Select_nested_collection_count_using_anonymous_type()
        {
            base.Select_nested_collection_count_using_anonymous_type();
        }

        public override void Select_nested_collection_count_using_DTO()
        {
            base.Select_nested_collection_count_using_DTO();
        }

        public override void Select_nested_collection_deep()
        {
            base.Select_nested_collection_deep();
        }

        public override void New_date_time_in_anonymous_type_works()
        {
            base.New_date_time_in_anonymous_type_works();
        }

        public override void Select_non_matching_value_types_int_to_long_introduces_explicit_cast()
        {
            base.Select_non_matching_value_types_int_to_long_introduces_explicit_cast();
        }

        public override void Select_non_matching_value_types_nullable_int_to_long_introduces_explicit_cast()
        {
            base.Select_non_matching_value_types_nullable_int_to_long_introduces_explicit_cast();
        }

        public override void Select_non_matching_value_types_nullable_int_to_int_doesnt_introduces_explicit_cast()
        {
            base.Select_non_matching_value_types_nullable_int_to_int_doesnt_introduces_explicit_cast();
        }

        public override void Select_non_matching_value_types_int_to_nullable_int_doesnt_introduce_explicit_cast()
        {
            base.Select_non_matching_value_types_int_to_nullable_int_doesnt_introduce_explicit_cast();
        }

        public override void Select_non_matching_value_types_from_binary_expression_introduces_explicit_cast()
        {
            base.Select_non_matching_value_types_from_binary_expression_introduces_explicit_cast();
        }

        public override void Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast()
        {
            base.Select_non_matching_value_types_from_binary_expression_nested_introduces_top_level_explicit_cast();
        }

        public override void Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast1()
        {
            base.Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast1();
        }

        public override void Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast2()
        {
            base.Select_non_matching_value_types_from_unary_expression_introduces_explicit_cast2();
        }

        public override void Select_non_matching_value_types_from_length_introduces_explicit_cast()
        {
            base.Select_non_matching_value_types_from_length_introduces_explicit_cast();
        }

        public override void Select_non_matching_value_types_from_method_call_introduces_explicit_cast()
        {
            base.Select_non_matching_value_types_from_method_call_introduces_explicit_cast();
        }

        public override void Select_non_matching_value_types_from_anonymous_type_introduces_explicit_cast()
        {
            base.Select_non_matching_value_types_from_anonymous_type_introduces_explicit_cast();
        }

        public override void Where_simple()
        {
            base.Where_simple();
        }

        public override void Where_simple_closure()
        {
            base.Where_simple_closure();
        }

        public override void Where_indexer_closure()
        {
            base.Where_indexer_closure();
        }

        public override void Where_simple_closure_constant()
        {
            base.Where_simple_closure_constant();
        }

        public override void Where_simple_closure_via_query_cache()
        {
            base.Where_simple_closure_via_query_cache();
        }

        public override void Where_method_call_nullable_type_closure_via_query_cache()
        {
            base.Where_method_call_nullable_type_closure_via_query_cache();
        }

        public override void Where_method_call_nullable_type_reverse_closure_via_query_cache()
        {
            base.Where_method_call_nullable_type_reverse_closure_via_query_cache();
        }

        public override void Where_method_call_closure_via_query_cache()
        {
            base.Where_method_call_closure_via_query_cache();
        }

        public override void Where_field_access_closure_via_query_cache()
        {
            base.Where_field_access_closure_via_query_cache();
        }

        public override void Where_property_access_closure_via_query_cache()
        {
            base.Where_property_access_closure_via_query_cache();
        }

        public override void Where_static_field_access_closure_via_query_cache()
        {
            base.Where_static_field_access_closure_via_query_cache();
        }

        public override void Where_static_property_access_closure_via_query_cache()
        {
            base.Where_static_property_access_closure_via_query_cache();
        }

        public override void Where_nested_field_access_closure_via_query_cache()
        {
            base.Where_nested_field_access_closure_via_query_cache();
        }

        public override void Where_nested_property_access_closure_via_query_cache()
        {
            base.Where_nested_property_access_closure_via_query_cache();
        }
        
        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Where_nested_field_access_closure_via_query_cache_error_null()
        {
            base.Where_nested_field_access_closure_via_query_cache_error_null();
        }

        public override void Where_new_instance_field_access_closure_via_query_cache()
        {
            base.Where_new_instance_field_access_closure_via_query_cache();
        }

        public override void Where_simple_closure_via_query_cache_nullable_type()
        {
            base.Where_simple_closure_via_query_cache_nullable_type();
        }

        public override void Where_simple_closure_via_query_cache_nullable_type_reverse()
        {
            base.Where_simple_closure_via_query_cache_nullable_type_reverse();
        }

        public override void Where_subquery_closure_via_query_cache()
        {
            base.Where_subquery_closure_via_query_cache();
        }

        public override void Where_simple_shadow()
        {
            base.Where_simple_shadow();
        }

        public override void Where_simple_shadow_projection()
        {
            base.Where_simple_shadow_projection();
        }

        public override void Where_simple_shadow_projection_mixed()
        {
            base.Where_simple_shadow_projection_mixed();
        }

        public override void Where_simple_shadow_subquery()
        {
            base.Where_simple_shadow_subquery();
        }

        public override void Where_shadow_subquery_FirstOrDefault()
        {
            base.Where_shadow_subquery_FirstOrDefault();
        }

        public override void Where_client()
        {
            base.Where_client();
        }

        public override void Where_subquery_correlated()
        {
            base.Where_subquery_correlated();
        }

        public override void Where_subquery_correlated_client_eval()
        {
            base.Where_subquery_correlated_client_eval();
        }

        public override void Where_client_and_server_top_level()
        {
            base.Where_client_and_server_top_level();
        }

        public override void Where_client_or_server_top_level()
        {
            base.Where_client_or_server_top_level();
        }

        public override void Where_client_and_server_non_top_level()
        {
            base.Where_client_and_server_non_top_level();
        }

        public override void Where_client_deep_inside_predicate_and_server_top_level()
        {
            base.Where_client_deep_inside_predicate_and_server_top_level();
        }

        public override void Where_equals_method_string()
        {
            base.Where_equals_method_string();
        }

        public override void Where_equals_method_int()
        {
            base.Where_equals_method_int();
        }

        public override void Where_equals_using_int_overload_on_mismatched_types()
        {
            base.Where_equals_using_int_overload_on_mismatched_types();
        }

        public override void Where_equals_on_mismatched_types_int_nullable_int()
        {
            base.Where_equals_on_mismatched_types_int_nullable_int();
        }

        public override void Where_equals_on_matched_nullable_int_types()
        {
            base.Where_equals_on_matched_nullable_int_types();
        }

        public override void Where_equals_on_null_nullable_int_types()
        {
            base.Where_equals_on_null_nullable_int_types();
        }

        public override void Where_comparison_nullable_type_not_null()
        {
            base.Where_comparison_nullable_type_not_null();
        }

        public override void Where_comparison_nullable_type_null()
        {
            base.Where_comparison_nullable_type_null();
        }

        public override void Where_string_length()
        {
            base.Where_string_length();
        }

        public override void Where_datetime_now()
        {
            base.Where_datetime_now();
        }

        public override void Where_datetime_utcnow()
        {
            base.Where_datetime_utcnow();
        }

        public override void Where_datetime_date_component()
        {
            base.Where_datetime_date_component();
        }

        public override void Where_date_add_year_constant_component()
        {
            base.Where_date_add_year_constant_component();
        }

        public override void Where_datetime_year_component()
        {
            base.Where_datetime_year_component();
        }

        public override void Where_datetime_month_component()
        {
            base.Where_datetime_month_component();
        }

        public override void Where_datetime_dayOfYear_component()
        {
            base.Where_datetime_dayOfYear_component();
        }

        public override void Where_datetime_day_component()
        {
            base.Where_datetime_day_component();
        }

        public override void Where_datetime_hour_component()
        {
            base.Where_datetime_hour_component();
        }

        public override void Where_datetime_minute_component()
        {
            base.Where_datetime_minute_component();
        }

        public override void Where_datetime_second_component()
        {
            base.Where_datetime_second_component();
        }

        public override void Where_datetime_millisecond_component()
        {
            base.Where_datetime_millisecond_component();
        }

        public override void Where_simple_reversed()
        {
            base.Where_simple_reversed();
        }

        public override void Where_is_null()
        {
            base.Where_is_null();
        }

        public override void Where_null_is_null()
        {
            base.Where_null_is_null();
        }

        public override void Where_constant_is_null()
        {
            base.Where_constant_is_null();
        }

        public override void Where_is_not_null()
        {
            base.Where_is_not_null();
        }

        public override void Where_null_is_not_null()
        {
            base.Where_null_is_not_null();
        }

        public override void Where_constant_is_not_null()
        {
            base.Where_constant_is_not_null();
        }

        public override void Where_identity_comparison()
        {
            base.Where_identity_comparison();
        }

        public override void Where_in_optimization_multiple()
        {
            base.Where_in_optimization_multiple();
        }

        public override void Where_not_in_optimization1()
        {
            base.Where_not_in_optimization1();
        }

        public override void Where_not_in_optimization2()
        {
            base.Where_not_in_optimization2();
        }

        public override void Where_not_in_optimization3()
        {
            base.Where_not_in_optimization3();
        }

        public override void Where_not_in_optimization4()
        {
            base.Where_not_in_optimization4();
        }

        public override void Where_select_many_and()
        {
            base.Where_select_many_and();
        }

        public override void Where_primitive()
        {
            base.Where_primitive();
        }

        public override void Where_primitive_tracked()
        {
            base.Where_primitive_tracked();
        }

        public override void Where_primitive_tracked2()
        {
            base.Where_primitive_tracked2();
        }

        public override void Where_bool_member()
        {
            base.Where_bool_member();
        }

        public override void Where_bool_member_false()
        {
            base.Where_bool_member_false();
        }

        public override void Where_bool_client_side_negated()
        {
            base.Where_bool_client_side_negated();
        }

        public override void Where_bool_member_negated_twice()
        {
            base.Where_bool_member_negated_twice();
        }

        public override void Where_bool_member_shadow()
        {
            base.Where_bool_member_shadow();
        }

        public override void Where_bool_member_false_shadow()
        {
            base.Where_bool_member_false_shadow();
        }

        public override void Where_bool_member_equals_constant()
        {
            base.Where_bool_member_equals_constant();
        }

        public override void Where_bool_member_in_complex_predicate()
        {
            base.Where_bool_member_in_complex_predicate();
        }

        public override void Where_bool_member_compared_to_binary_expression()
        {
            base.Where_bool_member_compared_to_binary_expression();
        }

        public override void Where_not_bool_member_compared_to_not_bool_member()
        {
            base.Where_not_bool_member_compared_to_not_bool_member();
        }

        public override void Where_negated_boolean_expression_compared_to_another_negated_boolean_expression()
        {
            base.Where_negated_boolean_expression_compared_to_another_negated_boolean_expression();
        }

        public override void Where_not_bool_member_compared_to_binary_expression()
        {
            base.Where_not_bool_member_compared_to_binary_expression();
        }

        public override void Where_bool_parameter()
        {
            base.Where_bool_parameter();
        }

        public override void Where_bool_parameter_compared_to_binary_expression()
        {
            base.Where_bool_parameter_compared_to_binary_expression();
        }

        public override void Where_bool_member_and_parameter_compared_to_binary_expression_nested()
        {
            base.Where_bool_member_and_parameter_compared_to_binary_expression_nested();
        }

        public override void Where_de_morgan_or_optimizated()
        {
            base.Where_de_morgan_or_optimizated();
        }

        public override void Where_de_morgan_and_optimizated()
        {
            base.Where_de_morgan_and_optimizated();
        }

        public override void Where_complex_negated_expression_optimized()
        {
            base.Where_complex_negated_expression_optimized();
        }

        public override void Where_short_member_comparison()
        {
            base.Where_short_member_comparison();
        }

        public override void Where_comparison_to_nullable_bool()
        {
            base.Where_comparison_to_nullable_bool();
        }

        public override void Where_true()
        {
            base.Where_true();
        }

        public override void Where_false()
        {
            base.Where_false();
        }

        public override void Where_bool_closure()
        {
            base.Where_bool_closure();
        }

        public override void Where_poco_closure()
        {
            base.Where_poco_closure();
        }

        public override void Where_default()
        {
            base.Where_default();
        }

        public override void Where_expression_invoke()
        {
            base.Where_expression_invoke();
        }

        public override void Where_concat_string_int_comparison1()
        {
            base.Where_concat_string_int_comparison1();
        }

        public override void Where_concat_string_int_comparison2()
        {
            base.Where_concat_string_int_comparison2();
        }

        public override void Where_concat_string_int_comparison3()
        {
            base.Where_concat_string_int_comparison3();
        }

        public override void Where_ternary_boolean_condition_true()
        {
            base.Where_ternary_boolean_condition_true();
        }

        public override void Where_ternary_boolean_condition_false()
        {
            base.Where_ternary_boolean_condition_false();
        }

        public override void Where_ternary_boolean_condition_with_another_condition()
        {
            base.Where_ternary_boolean_condition_with_another_condition();
        }

        public override void Where_ternary_boolean_condition_with_false_as_result_true()
        {
            base.Where_ternary_boolean_condition_with_false_as_result_true();
        }

        public override void Where_ternary_boolean_condition_with_false_as_result_false()
        {
            base.Where_ternary_boolean_condition_with_false_as_result_false();
        }

        public override void Where_compare_constructed_equal()
        {
            base.Where_compare_constructed_equal();
        }

        public override void Where_compare_constructed_multi_value_equal()
        {
            base.Where_compare_constructed_multi_value_equal();
        }

        public override void Where_compare_constructed_multi_value_not_equal()
        {
            base.Where_compare_constructed_multi_value_not_equal();
        }

        public override void Where_compare_constructed()
        {
            base.Where_compare_constructed();
        }

        public override void Where_compare_null()
        {
            base.Where_compare_null();
        }

        public override void Where_projection()
        {
            base.Where_projection();
        }

        public override void Where_Is_on_same_type()
        {
            base.Where_Is_on_same_type();
        }

        public override void Where_chain()
        {
            base.Where_chain();
        }
    }
}
