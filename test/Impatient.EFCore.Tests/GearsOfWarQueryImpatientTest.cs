using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class GearsOfWarQueryImpatientTest : GearsOfWarQueryTestBase<ImpatientTestStore, GearsOfWarQueryImpatientFixture>
    {
        public GearsOfWarQueryImpatientTest(GearsOfWarQueryImpatientFixture fixture) : base(fixture)
        {
        }

        public override void All_with_optional_navigation_is_translated_to_sql()
        {
            base.All_with_optional_navigation_is_translated_to_sql();
        }

        public override void Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql()
        {
            base.Any_with_optional_navigation_as_subquery_predicate_is_translated_to_sql();
        }

        public override void Bitwise_projects_values_in_select()
        {
            base.Bitwise_projects_values_in_select();
        }

        public override void Cast_result_operator_on_subquery_is_properly_lifted_to_a_convert()
        {
            base.Cast_result_operator_on_subquery_is_properly_lifted_to_a_convert();
        }

        public override void Client_method_on_collection_navigation_in_additional_from_clause()
        {
            base.Client_method_on_collection_navigation_in_additional_from_clause();
        }

        public override void Client_method_on_collection_navigation_in_order_by()
        {
            base.Client_method_on_collection_navigation_in_order_by();
        }

        public override void Client_method_on_collection_navigation_in_outer_join_key()
        {
            base.Client_method_on_collection_navigation_in_outer_join_key();
        }

        public override void Client_method_on_collection_navigation_in_predicate()
        {
            base.Client_method_on_collection_navigation_in_predicate();
        }

        public override void Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property()
        {
            base.Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property();
        }

        public override void Client_side_equality_with_parameter_works_with_optional_navigations()
        {
            base.Client_side_equality_with_parameter_works_with_optional_navigations();
        }

        public override void Coalesce_operator_in_predicate()
        {
            base.Coalesce_operator_in_predicate();
        }

        public override void Coalesce_operator_in_predicate_with_other_conditions()
        {
            base.Coalesce_operator_in_predicate_with_other_conditions();
        }

        public override void Coalesce_operator_in_projection_with_other_conditions()
        {
            base.Coalesce_operator_in_projection_with_other_conditions();
        }

        public override void Collection_navigation_access_on_derived_entity_using_cast()
        {
            base.Collection_navigation_access_on_derived_entity_using_cast();
        }

        public override void Collection_navigation_access_on_derived_entity_using_cast_in_SelectMany()
        {
            base.Collection_navigation_access_on_derived_entity_using_cast_in_SelectMany();
        }

        public override void Collection_with_inheritance_and_join_include_joined()
        {
            base.Collection_with_inheritance_and_join_include_joined();
        }

        public override void Collection_with_inheritance_and_join_include_source()
        {
            base.Collection_with_inheritance_and_join_include_source();
        }

        public override void Comparing_entities_using_Equals_inheritance()
        {
            base.Comparing_entities_using_Equals_inheritance();
        }

        public override void Comparing_two_collection_navigations_composite_key()
        {
            base.Comparing_two_collection_navigations_composite_key();
        }

        public override void Comparing_two_collection_navigations_inheritance()
        {
            base.Comparing_two_collection_navigations_inheritance();
        }

        public override void Complex_predicate_with_AndAlso_and_nullable_bool_property()
        {
            base.Complex_predicate_with_AndAlso_and_nullable_bool_property();
        }

        public override void Concat_anonymous_with_count()
        {
            base.Concat_anonymous_with_count();
        }

        public override void Concat_scalars_with_count()
        {
            base.Concat_scalars_with_count();
        }

        public override void Concat_with_collection_navigations()
        {
            base.Concat_with_collection_navigations();
        }

        public override void Concat_with_count()
        {
            base.Concat_with_count();
        }

        public override void Concat_with_groupings()
        {
            base.Concat_with_groupings();
        }

        public override void Concat_with_scalar_projection()
        {
            base.Concat_with_scalar_projection();
        }

        public override void Contains_on_nullable_array_produces_correct_sql()
        {
            base.Contains_on_nullable_array_produces_correct_sql();
        }

        public override void Contains_with_local_nullable_guid_list_closure()
        {
            base.Contains_with_local_nullable_guid_list_closure();
        }

        public override void Count_with_optional_navigation_is_translated_to_sql()
        {
            base.Count_with_optional_navigation_is_translated_to_sql();
        }

        public override void Count_with_unflattened_groupjoin_is_evaluated_on_client()
        {
            base.Count_with_unflattened_groupjoin_is_evaluated_on_client();
        }

        public override void DateTimeOffset_DateAdd_AddDays()
        {
            base.DateTimeOffset_DateAdd_AddDays();
        }

        public override void DateTimeOffset_DateAdd_AddHours()
        {
            base.DateTimeOffset_DateAdd_AddHours();
        }

        public override void DateTimeOffset_DateAdd_AddMilliseconds()
        {
            base.DateTimeOffset_DateAdd_AddMilliseconds();
        }

        public override void DateTimeOffset_DateAdd_AddMinutes()
        {
            base.DateTimeOffset_DateAdd_AddMinutes();
        }

        public override void DateTimeOffset_DateAdd_AddMonths()
        {
            base.DateTimeOffset_DateAdd_AddMonths();
        }

        public override void DateTimeOffset_DateAdd_AddSeconds()
        {
            base.DateTimeOffset_DateAdd_AddSeconds();
        }

        public override void DateTimeOffset_DateAdd_AddYears()
        {
            base.DateTimeOffset_DateAdd_AddYears();
        }

        public override void DateTimeOffset_Datepart_works()
        {
            base.DateTimeOffset_Datepart_works();
        }

        public override void DateTimeOffset_Date_works()
        {
            base.DateTimeOffset_Date_works();
        }

        public override void Distinct_on_subquery_doesnt_get_lifted()
        {
            base.Distinct_on_subquery_doesnt_get_lifted();
        }

        public override void Distinct_with_optional_navigation_is_translated_to_sql()
        {
            base.Distinct_with_optional_navigation_is_translated_to_sql();
        }

        public override void Distinct_with_unflattened_groupjoin_is_evaluated_on_client()
        {
            base.Distinct_with_unflattened_groupjoin_is_evaluated_on_client();
        }

        public override void Entity_equality_empty()
        {
            base.Entity_equality_empty();
        }

        public override void Enum_ToString_is_client_eval()
        {
            base.Enum_ToString_is_client_eval();
        }

        public override void FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql()
        {
            base.FirstOrDefault_with_manually_created_groupjoin_is_translated_to_sql();
        }

        public override void GroupJoin_Composite_Key()
        {
            base.GroupJoin_Composite_Key();
        }

        public override void Include_multiple_circular()
        {
            base.Include_multiple_circular();
        }

        public override void Include_multiple_circular_with_filter()
        {
            base.Include_multiple_circular_with_filter();
        }

        public override void Include_multiple_include_then_include()
        {
            base.Include_multiple_include_then_include();
        }

        public override void Include_multiple_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_many();
        }

        public override void Include_multiple_one_to_one_and_one_to_many_self_reference()
        {
            base.Include_multiple_one_to_one_and_one_to_many_self_reference();
        }

        public override void Include_multiple_one_to_one_and_one_to_one_and_one_to_many()
        {
            base.Include_multiple_one_to_one_and_one_to_one_and_one_to_many();
        }

        public override void Include_multiple_one_to_one_optional_and_one_to_one_required()
        {
            base.Include_multiple_one_to_one_optional_and_one_to_one_required();
        }

        public override void Include_navigation_on_derived_type()
        {
            base.Include_navigation_on_derived_type();
        }

        public override void Include_on_derived_entity_using_OfType()
        {
            base.Include_on_derived_entity_using_OfType();
        }

        public override void Include_on_derived_entity_using_subquery_with_cast()
        {
            base.Include_on_derived_entity_using_subquery_with_cast();
        }

        public override void Include_on_derived_entity_using_subquery_with_cast_AsNoTracking()
        {
            base.Include_on_derived_entity_using_subquery_with_cast_AsNoTracking();
        }

        public override void Include_on_derived_entity_using_subquery_with_cast_cross_product_base_entity()
        {
            base.Include_on_derived_entity_using_subquery_with_cast_cross_product_base_entity();
        }

        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1();
        }

        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result3();
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result();
        }

        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_conditional_result();
        }

        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_inheritance_and_coalesce_result();
        }

        public override void Include_using_alternate_key()
        {
            base.Include_using_alternate_key();
        }

        public override void Include_where_list_contains_navigation()
        {
            base.Include_where_list_contains_navigation();
        }

        public override void Include_where_list_contains_navigation2()
        {
            base.Include_where_list_contains_navigation2();
        }

        public override void Include_with_join_and_inheritance1()
        {
            base.Include_with_join_and_inheritance1();
        }

        public override void Include_with_join_and_inheritance2()
        {
            base.Include_with_join_and_inheritance2();
        }

        public override void Include_with_join_and_inheritance3()
        {
            base.Include_with_join_and_inheritance3();
        }

        public override void Include_with_join_collection1()
        {
            base.Include_with_join_collection1();
        }

        public override void Include_with_join_collection2()
        {
            base.Include_with_join_collection2();
        }

        public override void Include_with_join_multi_level()
        {
            base.Include_with_join_multi_level();
        }

        public override void Include_with_join_reference1()
        {
            base.Include_with_join_reference1();
        }

        public override void Include_with_join_reference2()
        {
            base.Include_with_join_reference2();
        }

        public override void Include_with_nested_navigation_in_order_by()
        {
            base.Include_with_nested_navigation_in_order_by();
        }

        public override void Join_navigation_translated_to_subquery_composite_key()
        {
            base.Join_navigation_translated_to_subquery_composite_key();
        }

        public override void Join_predicate_condition_equals_condition()
        {
            base.Join_predicate_condition_equals_condition();
        }

        public override void Join_predicate_value()
        {
            base.Join_predicate_value();
        }

        public override void Join_predicate_value_equals_condition()
        {
            base.Join_predicate_value_equals_condition();
        }

        public override void Left_join_predicate_condition_equals_condition()
        {
            base.Left_join_predicate_condition_equals_condition();
        }

        public override void Left_join_predicate_value()
        {
            base.Left_join_predicate_value();
        }

        public override void Left_join_predicate_value_equals_condition()
        {
            base.Left_join_predicate_value_equals_condition();
        }

        public override void Member_access_on_derived_entity_using_cast()
        {
            base.Member_access_on_derived_entity_using_cast();
        }

        public override void Member_access_on_derived_entity_using_cast_and_let()
        {
            base.Member_access_on_derived_entity_using_cast_and_let();
        }

        public override void Member_access_on_derived_materialized_entity_using_cast()
        {
            base.Member_access_on_derived_materialized_entity_using_cast();
        }

        public override void Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include()
        {
            base.Multiple_order_bys_are_properly_lifted_from_subquery_created_by_include();
        }

        public override void Navigation_accessed_twice_outside_and_inside_subquery()
        {
            base.Navigation_accessed_twice_outside_and_inside_subquery();
        }

        public override void Navigation_access_fk_on_derived_entity_using_cast()
        {
            base.Navigation_access_fk_on_derived_entity_using_cast();
        }

        public override void Navigation_access_on_derived_entity_using_cast()
        {
            base.Navigation_access_on_derived_entity_using_cast();
        }

        public override void Navigation_access_on_derived_materialized_entity_using_cast()
        {
            base.Navigation_access_on_derived_materialized_entity_using_cast();
        }

        public override void Navigation_access_via_EFProperty_on_derived_entity_using_cast()
        {
            base.Navigation_access_via_EFProperty_on_derived_entity_using_cast();
        }

        public override void Non_flattened_GroupJoin_with_result_operator_evaluates_on_the_client()
        {
            base.Non_flattened_GroupJoin_with_result_operator_evaluates_on_the_client();
        }

        public override void Non_unicode_parameter_is_used_for_non_unicode_column()
        {
            base.Non_unicode_parameter_is_used_for_non_unicode_column();
        }

        public override void Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column()
        {
            base.Non_unicode_string_literals_in_contains_is_used_for_non_unicode_column();
        }

        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_in_subquery();
        }

        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat();
        }

        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_contains();
        }

        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_subquery();
        }

        public override void Non_unicode_string_literal_is_used_for_non_unicode_column()
        {
            base.Non_unicode_string_literal_is_used_for_non_unicode_column();
        }

        public override void Non_unicode_string_literal_is_used_for_non_unicode_column_right()
        {
            base.Non_unicode_string_literal_is_used_for_non_unicode_column_right();
        }

        public override void Null_propagation_optimization1()
        {
            base.Null_propagation_optimization1();
        }

        public override void Null_propagation_optimization2()
        {
            base.Null_propagation_optimization2();
        }

        public override void Null_propagation_optimization3()
        {
            base.Null_propagation_optimization3();
        }

        public override void Null_propagation_optimization4()
        {
            base.Null_propagation_optimization4();
        }

        public override void Null_propagation_optimization5()
        {
            base.Null_propagation_optimization5();
        }

        public override void Null_propagation_optimization6()
        {
            base.Null_propagation_optimization6();
        }

        public override void Optional_Navigation_Null_Coalesce_To_Clr_Type()
        {
            base.Optional_Navigation_Null_Coalesce_To_Clr_Type();
        }

        public override void Optional_navigation_type_compensation_works_with_all()
        {
            base.Optional_navigation_type_compensation_works_with_all();
        }

        public override void Optional_navigation_type_compensation_works_with_array_initializers()
        {
            base.Optional_navigation_type_compensation_works_with_array_initializers();
        }

        public override void Optional_navigation_type_compensation_works_with_binary_expression()
        {
            base.Optional_navigation_type_compensation_works_with_binary_expression();
        }

        public override void Optional_navigation_type_compensation_works_with_conditional_expression()
        {
            base.Optional_navigation_type_compensation_works_with_conditional_expression();
        }

        public override void Optional_navigation_type_compensation_works_with_contains()
        {
            base.Optional_navigation_type_compensation_works_with_contains();
        }

        public override void Optional_navigation_type_compensation_works_with_DTOs()
        {
            base.Optional_navigation_type_compensation_works_with_DTOs();
        }

        public override void Optional_navigation_type_compensation_works_with_groupby()
        {
            base.Optional_navigation_type_compensation_works_with_groupby();
        }

        public override void Optional_navigation_type_compensation_works_with_list_initializers()
        {
            base.Optional_navigation_type_compensation_works_with_list_initializers();
        }

        public override void Optional_navigation_type_compensation_works_with_orderby()
        {
            base.Optional_navigation_type_compensation_works_with_orderby();
        }

        public override void Optional_navigation_type_compensation_works_with_predicate()
        {
            base.Optional_navigation_type_compensation_works_with_predicate();
        }

        public override void Optional_navigation_type_compensation_works_with_predicate2()
        {
            base.Optional_navigation_type_compensation_works_with_predicate2();
        }

        public override void Optional_navigation_type_compensation_works_with_predicate_negated()
        {
            base.Optional_navigation_type_compensation_works_with_predicate_negated();
        }

        public override void Optional_navigation_type_compensation_works_with_predicate_negated_complex1()
        {
            base.Optional_navigation_type_compensation_works_with_predicate_negated_complex1();
        }

        public override void Optional_navigation_type_compensation_works_with_predicate_negated_complex2()
        {
            base.Optional_navigation_type_compensation_works_with_predicate_negated_complex2();
        }

        public override void Optional_navigation_type_compensation_works_with_projection()
        {
            base.Optional_navigation_type_compensation_works_with_projection();
        }

        public override void Optional_navigation_type_compensation_works_with_projection_into_anonymous_type()
        {
            base.Optional_navigation_type_compensation_works_with_projection_into_anonymous_type();
        }

        public override void Optional_navigation_type_compensation_works_with_skip()
        {
            base.Optional_navigation_type_compensation_works_with_skip();
        }

        public override void Optional_navigation_type_compensation_works_with_take()
        {
            base.Optional_navigation_type_compensation_works_with_take();
        }

        public override void Optional_navigation_with_collection_composite_key()
        {
            base.Optional_navigation_with_collection_composite_key();
        }

        public override void Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used()
        {
            base.Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used();
        }

        public override void Order_by_is_properly_lifted_from_subquery_created_by_include()
        {
            base.Order_by_is_properly_lifted_from_subquery_created_by_include();
        }

        public override void Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query()
        {
            base.Order_by_is_properly_lifted_from_subquery_with_same_order_by_in_the_outer_query();
        }

        public override void Order_by_then_by_is_properly_lifted_from_subquery_created_by_include()
        {
            base.Order_by_then_by_is_properly_lifted_from_subquery_created_by_include();
        }

        public override void Project_collection_navigation_with_inheritance1()
        {
            base.Project_collection_navigation_with_inheritance1();
        }

        public override void Project_collection_navigation_with_inheritance2()
        {
            base.Project_collection_navigation_with_inheritance2();
        }

        public override void Project_collection_navigation_with_inheritance3()
        {
            base.Project_collection_navigation_with_inheritance3();
        }

        public override void Property_access_on_derived_entity_using_cast()
        {
            base.Property_access_on_derived_entity_using_cast();
        }

        public override void Select_coalesce_with_anonymous_types()
        {
            base.Select_coalesce_with_anonymous_types();
        }

        public override void Select_comparison_with_null()
        {
            base.Select_comparison_with_null();
        }

        public override void Select_conditional_with_anonymous_types()
        {
            base.Select_conditional_with_anonymous_types();
        }

        public override void Select_conditional_with_anonymous_type_and_null_constant()
        {
            base.Select_conditional_with_anonymous_type_and_null_constant();
        }

        public override void Select_correlated_filtered_collection()
        {
            base.Select_correlated_filtered_collection();
        }

        public override void Select_correlated_filtered_collection_with_composite_key()
        {
            base.Select_correlated_filtered_collection_with_composite_key();
        }

        public override void Select_correlated_filtered_collection_works_with_caching()
        {
            base.Select_correlated_filtered_collection_works_with_caching();
        }

        public override void Select_enum_has_flag()
        {
            base.Select_enum_has_flag();
        }

        public override void Select_inverted_boolean()
        {
            base.Select_inverted_boolean();
        }

        public override void Select_length_of_string_property()
        {
            base.Select_length_of_string_property();
        }

        public override void Select_multiple_conditions()
        {
            base.Select_multiple_conditions();
        }

        public override void Select_navigation_with_concat_and_count()
        {
            base.Select_navigation_with_concat_and_count();
        }

        public override void Select_nested_ternary_operations()
        {
            base.Select_nested_ternary_operations();
        }

        public override void Select_null_conditional_with_inheritance()
        {
            base.Select_null_conditional_with_inheritance();
        }

        public override void Select_null_conditional_with_inheritance_negative()
        {
            base.Select_null_conditional_with_inheritance_negative();
        }

        public override void Select_null_propagation_negative1()
        {
            base.Select_null_propagation_negative1();
        }

        public override void Select_null_propagation_negative2()
        {
            base.Select_null_propagation_negative2();
        }

        public override void Select_null_propagation_negative3()
        {
            base.Select_null_propagation_negative3();
        }

        public override void Select_null_propagation_negative4()
        {
            base.Select_null_propagation_negative4();
        }

        public override void Select_null_propagation_negative5()
        {
            base.Select_null_propagation_negative5();
        }

        public override void Select_null_propagation_negative6()
        {
            base.Select_null_propagation_negative6();
        }

        public override void Select_null_propagation_negative7()
        {
            base.Select_null_propagation_negative7();
        }

        public override void Select_null_propagation_negative8()
        {
            base.Select_null_propagation_negative8();
        }

        public override void Select_null_propagation_optimization7()
        {
            base.Select_null_propagation_optimization7();
        }

        public override void Select_null_propagation_optimization8()
        {
            base.Select_null_propagation_optimization8();
        }

        public override void Select_null_propagation_optimization9()
        {
            base.Select_null_propagation_optimization9();
        }

        public override void Select_null_propagation_works_for_multiple_navigations_with_composite_keys()
        {
            base.Select_null_propagation_works_for_multiple_navigations_with_composite_keys();
        }

        public override void Select_null_propagation_works_for_navigations_with_composite_keys()
        {
            base.Select_null_propagation_works_for_navigations_with_composite_keys();
        }

        public override void Select_Singleton_Navigation_With_Member_Access()
        {
            base.Select_Singleton_Navigation_With_Member_Access();
        }

        public override void Select_subquery_distinct_firstordefault()
        {
            base.Select_subquery_distinct_firstordefault();
        }

        public override void Select_ternary_operation_multiple_conditions()
        {
            base.Select_ternary_operation_multiple_conditions();
        }

        public override void Select_ternary_operation_multiple_conditions_2()
        {
            base.Select_ternary_operation_multiple_conditions_2();
        }

        public override void Select_ternary_operation_with_boolean()
        {
            base.Select_ternary_operation_with_boolean();
        }

        public override void Select_ternary_operation_with_has_value_not_null()
        {
            base.Select_ternary_operation_with_has_value_not_null();
        }

        public override void Select_ternary_operation_with_inverted_boolean()
        {
            base.Select_ternary_operation_with_inverted_boolean();
        }

        public override void Select_Where_Navigation()
        {
            base.Select_Where_Navigation();
        }

        public override void Select_Where_Navigation_Client()
        {
            base.Select_Where_Navigation_Client();
        }

        public override void Select_Where_Navigation_Equals_Navigation()
        {
            base.Select_Where_Navigation_Equals_Navigation();
        }

        public override void Select_Where_Navigation_Included()
        {
            base.Select_Where_Navigation_Included();
        }

        public override void Select_Where_Navigation_Null()
        {
            base.Select_Where_Navigation_Null();
        }

        public override void Select_Where_Navigation_Null_Reverse()
        {
            base.Select_Where_Navigation_Null_Reverse();
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar();
        }

        public override void Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected()
        {
            base.Select_Where_Navigation_Scalar_Equals_Navigation_Scalar_Projected();
        }

        public override void Singleton_Navigation_With_Member_Access()
        {
            base.Singleton_Navigation_With_Member_Access();
        }

        public override void String_based_Include_navigation_on_derived_type()
        {
            base.String_based_Include_navigation_on_derived_type();
        }

        public override void Subquery_containing_join_gets_lifted_clashing_names()
        {
            base.Subquery_containing_join_gets_lifted_clashing_names();
        }

        public override void Subquery_containing_join_projecting_main_from_clause_gets_lifted()
        {
            base.Subquery_containing_join_projecting_main_from_clause_gets_lifted();
        }

        public override void Subquery_containing_left_join_projecting_main_from_clause_gets_lifted()
        {
            base.Subquery_containing_left_join_projecting_main_from_clause_gets_lifted();
        }

        public override void Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted()
        {
            base.Subquery_containing_SelectMany_projecting_main_from_clause_gets_lifted();
        }

        public override void Subquery_created_by_include_gets_lifted_nested()
        {
            base.Subquery_created_by_include_gets_lifted_nested();
        }

        public override void Subquery_is_lifted_from_main_from_clause_of_SelectMany()
        {
            base.Subquery_is_lifted_from_main_from_clause_of_SelectMany();
        }

        public override void Subquery_is_not_lifted_from_additional_from_clause()
        {
            base.Subquery_is_not_lifted_from_additional_from_clause();
        }

        public override void Subquery_with_result_operator_is_not_lifted()
        {
            base.Subquery_with_result_operator_is_not_lifted();
        }

        public override void Sum_with_optional_navigation_is_translated_to_sql()
        {
            base.Sum_with_optional_navigation_is_translated_to_sql();
        }

        public override void ToString_guid_property_projection()
        {
            base.ToString_guid_property_projection();
        }

        public override void Union_with_collection_navigations()
        {
            base.Union_with_collection_navigations();
        }

        public override void Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property()
        {
            base.Unnecessary_include_doesnt_get_added_complex_when_projecting_EF_Property();
        }

        public override void Where_and_order_by_are_properly_lifted_from_subquery_created_by_tracking()
        {
            base.Where_and_order_by_are_properly_lifted_from_subquery_created_by_tracking();
        }

        public override void Where_any_subquery_without_collision()
        {
            base.Where_any_subquery_without_collision();
        }

        public override void Where_bitwise_and_enum()
        {
            base.Where_bitwise_and_enum();
        }

        public override void Where_bitwise_and_integral()
        {
            base.Where_bitwise_and_integral();
        }

        public override void Where_bitwise_and_nullable_enum_with_constant()
        {
            base.Where_bitwise_and_nullable_enum_with_constant();
        }

        public override void Where_bitwise_and_nullable_enum_with_non_nullable_parameter()
        {
            base.Where_bitwise_and_nullable_enum_with_non_nullable_parameter();
        }

        public override void Where_bitwise_and_nullable_enum_with_nullable_parameter()
        {
            base.Where_bitwise_and_nullable_enum_with_nullable_parameter();
        }

        public override void Where_bitwise_and_nullable_enum_with_null_constant()
        {
            base.Where_bitwise_and_nullable_enum_with_null_constant();
        }

        public override void Where_bitwise_or_enum()
        {
            base.Where_bitwise_or_enum();
        }

        public override void Where_coalesce_with_anonymous_types()
        {
            base.Where_coalesce_with_anonymous_types();
        }

        public override void Where_compare_anonymous_types()
        {
            base.Where_compare_anonymous_types();
        }

        public override void Where_compare_anonymous_types_with_uncorrelated_members()
        {
            base.Where_compare_anonymous_types_with_uncorrelated_members();
        }

        public override void Where_conditional_with_anonymous_type()
        {
            base.Where_conditional_with_anonymous_type();
        }

        public override void Where_count_subquery_without_collision()
        {
            base.Where_count_subquery_without_collision();
        }

        public override void Where_enum()
        {
            base.Where_enum();
        }

        public override void Where_enum_has_flag()
        {
            base.Where_enum_has_flag();
        }

        public override void Where_enum_has_flag_subquery()
        {
            base.Where_enum_has_flag_subquery();
        }

        public override void Where_enum_has_flag_subquery_client_eval()
        {
            base.Where_enum_has_flag_subquery_client_eval();
        }

        public override void Where_enum_has_flag_with_non_nullable_parameter()
        {
            base.Where_enum_has_flag_with_non_nullable_parameter();
        }

        public override void Where_has_flag_with_nullable_parameter()
        {
            base.Where_has_flag_with_nullable_parameter();
        }

        public override void Where_is_properly_lifted_from_subquery_created_by_include()
        {
            base.Where_is_properly_lifted_from_subquery_created_by_include();
        }

        public override void Where_member_access_on_anonymous_type()
        {
            base.Where_member_access_on_anonymous_type();
        }

        public override void Where_nullable_enum_with_constant()
        {
            base.Where_nullable_enum_with_constant();
        }

        public override void Where_nullable_enum_with_non_nullable_parameter()
        {
            base.Where_nullable_enum_with_non_nullable_parameter();
        }

        public override void Where_nullable_enum_with_nullable_parameter()
        {
            base.Where_nullable_enum_with_nullable_parameter();
        }

        public override void Where_nullable_enum_with_null_constant()
        {
            base.Where_nullable_enum_with_null_constant();
        }

        public override void Where_subquery_boolean()
        {
            base.Where_subquery_boolean();
        }

        public override void Where_subquery_concat_firstordefault_boolean()
        {
            base.Where_subquery_concat_firstordefault_boolean();
        }

        public override void Where_subquery_concat_order_by_firstordefault_boolean()
        {
            base.Where_subquery_concat_order_by_firstordefault_boolean();
        }

        public override void Where_subquery_distinct_firstordefault_boolean()
        {
            base.Where_subquery_distinct_firstordefault_boolean();
        }

        public override void Where_subquery_distinct_first_boolean()
        {
            base.Where_subquery_distinct_first_boolean();
        }

        public override void Where_subquery_distinct_lastordefault_boolean()
        {
            base.Where_subquery_distinct_lastordefault_boolean();
        }

        public override void Where_subquery_distinct_last_boolean()
        {
            base.Where_subquery_distinct_last_boolean();
        }

        public override void Where_subquery_distinct_orderby_firstordefault_boolean()
        {
            base.Where_subquery_distinct_orderby_firstordefault_boolean();
        }

        public override void Where_subquery_distinct_singleordefault_boolean()
        {
            base.Where_subquery_distinct_singleordefault_boolean();
        }

        public override void Where_subquery_union_firstordefault_boolean()
        {
            base.Where_subquery_union_firstordefault_boolean();
        }
    }
}
