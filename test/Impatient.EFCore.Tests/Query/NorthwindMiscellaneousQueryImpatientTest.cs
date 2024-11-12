using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindMiscellaneousQueryImpatientTest : NorthwindMiscellaneousQueryRelationalTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindMiscellaneousQueryImpatientTest(NorthwindQueryImpatientFixture fixture, ITestOutputHelper testOutputHelper) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
        fixture.TestSqlLoggerFactory.SetTestOutputHelper(testOutputHelper);
    }

    private void AssertSql(string expected) => Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    private const string WeirdSkipReason = "This test gives a DataReader exception, which for some reason potentially poisons other failing test's result messages.";

    [Theory(Skip = ClientEval)]
    public override Task All_client(bool async)
    {
        return base.All_client(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task All_client_and_server_top_level(bool async)
    {
        return base.All_client_and_server_top_level(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task All_client_or_server_top_level(bool async)
    {
        return base.All_client_or_server_top_level(async);
    }

    [DisagreeWithEFCore]
    [Theory(Skip = ClientEval)]
    public override Task Client_OrderBy_GroupBy_Group_ordering_works(bool async)
    {
        return base.Client_OrderBy_GroupBy_Group_ordering_works(async);
    }

    [Theory(Skip = Punt)]
    public override Task Collection_navigation_equal_to_null_for_subquery(bool async)
    {
        return base.Collection_navigation_equal_to_null_for_subquery(async);
    }

    [Theory(Skip = Punt)]
    public override Task Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_constant_one(bool async)
    {
        return base.Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_constant_one(async);
    }

    [Theory(Skip = Punt)]
    public override Task Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_constant_zero(bool async)
    {
        return base.Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_constant_zero(async);
    }

    [Theory(Skip = Punt)]
    public override Task Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_parameter(bool async)
    {
        return base.Collection_navigation_equal_to_null_for_subquery_using_ElementAtOrDefault_parameter(async);
    }

    [Theory(Skip = Punt)]
    public override Task Collection_navigation_equality_rewrite_for_subquery(bool async)
    {
        return base.Collection_navigation_equality_rewrite_for_subquery(async);
    }

    // I'm not sure I agree with the EF Core reasoning here.
    // It seems like an opinionated decision where I just have a different opinion.
    [DisagreeWithEFCore]
    [Theory(Skip = Punt)]
    public override Task Comparing_collection_navigation_to_null(bool async)
    {
        return base.Comparing_collection_navigation_to_null(async);
    }

    [DisagreeWithEFCore]
    [Theory(Skip = ClientEval)]
    public override Task Default_if_empty_top_level_arg(bool async)
    {
        return base.Default_if_empty_top_level_arg(async);
    }

    [DisagreeWithEFCore]
    [Theory(Skip = ClientEval)]
    public override Task Default_if_empty_top_level_arg_followed_by_projecting_constant(bool async)
    {
        return base.Default_if_empty_top_level_arg_followed_by_projecting_constant(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Dependent_to_principal_navigation_equal_to_null_for_subquery(bool async)
    {
        return base.Dependent_to_principal_navigation_equal_to_null_for_subquery(async);
    }

    [Fact(Skip = "Bad test, says so in source implementation. The query is fine.")]
    public override Task Mixed_sync_async_query()
    {
        return base.Mixed_sync_async_query();
    }

    [Theory(Skip = ClientEval)]
    public override Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2(bool async)
    {
        return base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ(bool async)
    {
        return base.Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ(async);
    }

    // Again, I just don't agree with the EF Core approach on this one.
    [DisagreeWithEFCore]
    [Theory(Skip = ClientEval)]
    public override Task OrderBy_client_mixed(bool async)
    {
        return base.OrderBy_client_mixed(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task OrderBy_correlated_subquery2(bool async)
    {
        return base.OrderBy_correlated_subquery2(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Pending_selector_in_cardinality_reducing_method_is_applied_before_expanding_collection_navigation_member(bool async)
    {
        return base.Pending_selector_in_cardinality_reducing_method_is_applied_before_expanding_collection_navigation_member(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Random_next_is_not_funcletized_2(bool async)
    {
        return base.Random_next_is_not_funcletized_2(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Random_next_is_not_funcletized_3(bool async)
    {
        return base.Random_next_is_not_funcletized_3(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Random_next_is_not_funcletized_5(bool async)
    {
        return base.Random_next_is_not_funcletized_5(async);
    }

    public override Task Select_bitwise_and(bool async)
    {
        return base.Select_bitwise_and(async);
    }

    [Theory(Skip = BadMaterialization)]
    public override Task Select_correlated_subquery_filtered_returning_queryable_throws(bool async)
    {
        return base.Select_correlated_subquery_filtered_returning_queryable_throws(async);
    }

    [Theory(Skip = BadMaterialization)]
    public override Task Select_correlated_subquery_ordered_returning_queryable_throws(bool async)
    {
        return base.Select_correlated_subquery_ordered_returning_queryable_throws(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Select_DTO_constructor_distinct_with_collection_projection_translated_to_server(bool async)
    {
        return base.Select_DTO_constructor_distinct_with_collection_projection_translated_to_server(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Select_DTO_constructor_distinct_with_collection_projection_translated_to_server_with_binding_after_client_eval(bool async)
    {
        return base.Select_DTO_constructor_distinct_with_collection_projection_translated_to_server_with_binding_after_client_eval(async);
    }

    [Theory(Skip = BadMaterialization)]
    public override Task Select_subquery_recursive_trivial_returning_queryable(bool async)
    {
        return base.Select_subquery_recursive_trivial_returning_queryable(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Select_Subquery_Single(bool async)
    {
        return base.Select_Subquery_Single(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Select_Where_Subquery_Deep_Single(bool async)
    {
        return base.Select_Where_Subquery_Deep_Single(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Select_Where_Subquery_Equality(bool async)
    {
        return base.Select_Where_Subquery_Equality(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task SelectMany_after_client_method(bool async)
    {
        return base.SelectMany_after_client_method(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task SelectMany_mixed(bool async)
    {
        return base.SelectMany_mixed(async);
    }

    [Theory(Skip = Punt)]
    public override Task Skip_0_Take_0_works_when_constant(bool async)
    {
        return base.Skip_0_Take_0_works_when_constant(async);
    }

    [Theory(Skip = Punt)]
    public override Task Skip_0_Take_0_works_when_parameter(bool async)
    {
        return base.Skip_0_Take_0_works_when_parameter(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Subquery_member_pushdown_does_not_change_original_subquery_model(bool async)
    {
        return base.Subquery_member_pushdown_does_not_change_original_subquery_model(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Subquery_member_pushdown_does_not_change_original_subquery_model2(bool async)
    {
        return base.Subquery_member_pushdown_does_not_change_original_subquery_model2(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Throws_on_concurrent_query_first(bool async)
    {
        return base.Throws_on_concurrent_query_first(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Throws_on_concurrent_query_list(bool async)
    {
        return base.Throws_on_concurrent_query_list(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition_entity_equality_multiple_elements_First(bool async)
    {
        return base.Where_query_composition_entity_equality_multiple_elements_First(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition_entity_equality_multiple_elements_FirstOrDefault(bool async)
    {
        return base.Where_query_composition_entity_equality_multiple_elements_FirstOrDefault(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition_entity_equality_multiple_elements_Single(bool async)
    {
        return base.Where_query_composition_entity_equality_multiple_elements_Single(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(bool async)
    {
        return base.Where_query_composition_entity_equality_multiple_elements_SingleOrDefault(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition_entity_equality_no_elements_First(bool async)
    {
        return base.Where_query_composition_entity_equality_no_elements_First(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition_entity_equality_no_elements_FirstOrDefault(bool async)
    {
        return base.Where_query_composition_entity_equality_no_elements_FirstOrDefault(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition_entity_equality_no_elements_Single(bool async)
    {
        return base.Where_query_composition_entity_equality_no_elements_Single(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition_entity_equality_no_elements_SingleOrDefault(bool async)
    {
        return base.Where_query_composition_entity_equality_no_elements_SingleOrDefault(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition_entity_equality_one_element_First(bool async)
    {
        return base.Where_query_composition_entity_equality_one_element_First(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition_entity_equality_one_element_FirstOrDefault(bool async)
    {
        return base.Where_query_composition_entity_equality_one_element_FirstOrDefault(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition_entity_equality_one_element_Single(bool async)
    {
        return base.Where_query_composition_entity_equality_one_element_Single(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition_entity_equality_one_element_SingleOrDefault(bool async)
    {
        return base.Where_query_composition_entity_equality_one_element_SingleOrDefault(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition_is_not_null(bool async)
    {
        return base.Where_query_composition_is_not_null(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition_is_null(bool async)
    {
        return base.Where_query_composition_is_null(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition2(bool async)
    {
        return base.Where_query_composition2(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition3(bool async)
    {
        return base.Where_query_composition3(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition4(bool async)
    {
        return base.Where_query_composition4(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition5(bool async)
    {
        return base.Where_query_composition5(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_query_composition6(bool async)
    {
        return base.Where_query_composition6(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_subquery_expression(bool async)
    {
        return base.Where_subquery_expression(async);
    }

    [Theory(Skip = WeirdSkipReason)]
    public override Task Where_subquery_expression_same_parametername(bool async)
    {
        return base.Where_subquery_expression_same_parametername(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Using_string_Equals_with_StringComparison_throws_informative_error(bool async)
    {
        return base.Using_string_Equals_with_StringComparison_throws_informative_error(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Using_static_string_Equals_with_StringComparison_throws_informative_error(bool async)
    {
        return base.Using_static_string_Equals_with_StringComparison_throws_informative_error(async);
    }

    // TODO: Translate List.Exists
    [Theory(Skip = ClientEval)]
    public override Task Where_Join_Not_Exists(bool async)
    {
        return base.Where_Join_Not_Exists(async);
    }
}
