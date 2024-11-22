using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Sdk;
using static Impatient.EFCore.Tests.Query.ComplexNavigationsCollectionsQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class ComplexNavigationsCollectionsQueryImpatientTest : ComplexNavigationsCollectionsQueryRelationalTestBase<Fixture>
{
    public ComplexNavigationsCollectionsQueryImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => base.Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(Fixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : ComplexNavigationsQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    #region special includes

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_after_different_filtered_include_different_level(bool async)
    {
        return base.Filtered_include_after_different_filtered_include_different_level(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_after_different_filtered_include_same_level(bool async)
    {
        return base.Filtered_include_after_different_filtered_include_same_level(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_after_reference_navigation(bool async)
    {
        return base.Filtered_include_after_reference_navigation(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(bool async)
    {
        return base.Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_and_non_filtered_include_on_same_navigation1(bool async)
    {
        return base.Filtered_include_and_non_filtered_include_on_same_navigation1(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_and_non_filtered_include_on_same_navigation2(bool async)
    {
        return base.Filtered_include_and_non_filtered_include_on_same_navigation2(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_basic_OrderBy_Skip(bool async)
    {
        return base.Filtered_include_basic_OrderBy_Skip(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_basic_OrderBy_Skip_Take(bool async)
    {
        return base.Filtered_include_basic_OrderBy_Skip_Take(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_basic_OrderBy_Skip_Take_EF_Property(bool async)
    {
        return base.Filtered_include_basic_OrderBy_Skip_Take_EF_Property(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_basic_OrderBy_Take(bool async)
    {
        return base.Filtered_include_basic_OrderBy_Take(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_basic_Where(bool async)
    {
        return base.Filtered_include_basic_Where(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_basic_Where_EF_Property(bool async)
    {
        return base.Filtered_include_basic_Where_EF_Property(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_calling_methods_directly_on_parameter_throws(bool async)
    {
        return base.Filtered_include_calling_methods_directly_on_parameter_throws(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_complex_three_level_with_middle_having_filter1(bool async)
    {
        return base.Filtered_include_complex_three_level_with_middle_having_filter1(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_complex_three_level_with_middle_having_filter2(bool async)
    {
        return base.Filtered_include_complex_three_level_with_middle_having_filter2(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_context_accessed_inside_filter(bool async)
    {
        return base.Filtered_include_context_accessed_inside_filter(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_context_accessed_inside_filter_correlated(bool async)
    {
        return base.Filtered_include_context_accessed_inside_filter_correlated(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_different_filter_set_on_same_navigation_twice(bool async)
    {
        return base.Filtered_include_different_filter_set_on_same_navigation_twice(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_different_filter_set_on_same_navigation_twice_multi_level(bool async)
    {
        return base.Filtered_include_different_filter_set_on_same_navigation_twice_multi_level(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_include_parameter_used_inside_filter_throws(bool async)
    {
        return base.Filtered_include_include_parameter_used_inside_filter_throws(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_is_considered_loaded(bool async)
    {
        return base.Filtered_include_is_considered_loaded(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(bool async)
    {
        return base.Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_on_ThenInclude(bool async)
    {
        return base.Filtered_include_on_ThenInclude(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_on_ThenInclude_EF_Property(bool async)
    {
        return base.Filtered_include_on_ThenInclude_EF_Property(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_OrderBy(bool async)
    {
        return base.Filtered_include_OrderBy(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_OrderBy_EF_Property(bool async)
    {
        return base.Filtered_include_OrderBy_EF_Property(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_outer_parameter_used_inside_filter(bool async)
    {
        return base.Filtered_include_outer_parameter_used_inside_filter(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_same_filter_set_on_same_navigation_twice(bool async)
    {
        return base.Filtered_include_same_filter_set_on_same_navigation_twice(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(bool async)
    {
        return base.Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_Skip_Take_with_another_Skip_Take_on_top_level(bool async)
    {
        return base.Filtered_include_Skip_Take_with_another_Skip_Take_on_top_level(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_Skip_without_OrderBy(bool async)
    {
        return base.Filtered_include_Skip_without_OrderBy(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_Take_without_OrderBy(bool async)
    {
        return base.Filtered_include_Take_without_OrderBy(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_Take_with_another_Take_on_top_level(bool async)
    {
        return base.Filtered_include_Take_with_another_Take_on_top_level(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_ThenInclude_OrderBy(bool async)
    {
        return base.Filtered_include_ThenInclude_OrderBy(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_variable_used_inside_filter(bool async)
    {
        return base.Filtered_include_variable_used_inside_filter(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_with_Distinct_throws(bool async)
    {
        return base.Filtered_include_with_Distinct_throws(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_FirstOrDefault_on_top_level(bool async)
    {
        return base.Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_FirstOrDefault_on_top_level(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_unordered_Take_on_top_level(bool async)
    {
        return base.Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_unordered_Take_on_top_level(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_ThenInclude_OrderBy(bool async)
    {
        return base.Filtered_ThenInclude_OrderBy(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Include_partially_added_before_Where_and_then_build_upon_with_filtered_include(bool async)
    {
        return base.Include_partially_added_before_Where_and_then_build_upon_with_filtered_include(async);
    }

    #endregion

    #region lifted includes

    [Theory(Skip = LiftedInclude)]
    public override Task Include_after_SelectMany_and_multiple_reference_navigations(bool async)
    {
        // The test looks like this:

        /*

        AssertQuery(
            async,
            ss => ss.Set<Level1>()
                .SelectMany(l1 => l1.OneToMany_Required1)
                .Include(l2 => l2.OneToOne_Optional_FK2.OneToOne_Required_FK3.OneToMany_Optional_Self4)
                .Select(l2 => l2.OneToOne_Optional_FK2)
                .Select(l3 => l3.OneToOne_Required_FK3),
            elementAsserter: (e, a) => AssertInclude(e, a, new ExpectedInclude<Level4>(l4 => l4.OneToMany_Optional_Self4)));

        */

        // The issue is that we are composing the include onto the materialization expression for the OneToMany_Required1,
        // but composing the navigations in the following Select calls as separately joined queries, so when the final
        // Select projects the OneToOne_Required_FK3, it is projecting from that joined query and not keeping the include.
        // This really seems like a 'caveat emptor' thing to me, but we could make it actually work if we had a pass that
        // 'lifted' the call to Include to as late in the tree as possible.

        return base.Include_after_SelectMany_and_multiple_reference_navigations(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Include_after_multiple_SelectMany_and_reference_navigation(bool async)
    {
        return base.Include_after_multiple_SelectMany_and_reference_navigation(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Include_and_ThenInclude_collections_followed_by_projecting_the_first_collection(bool async)
    {
        return base.Include_and_ThenInclude_collections_followed_by_projecting_the_first_collection(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Include_collection_and_another_navigation_chain_followed_by_projecting_the_first_collection(bool async)
    {
        return base.Include_collection_and_another_navigation_chain_followed_by_projecting_the_first_collection(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Include_collection_followed_by_complex_includes_and_projecting_the_included_collection(bool async)
    {
        return base.Include_collection_followed_by_complex_includes_and_projecting_the_included_collection(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Include_collection_ThenInclude_reference_followed_by_projection_into_anonmous_type(bool async)
    {
        return base.Include_collection_ThenInclude_reference_followed_by_projection_into_anonmous_type(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override async Task Include_ThenInclude_ThenInclude_followed_by_two_nested_selects(bool async)
    {
        await base.Include_ThenInclude_ThenInclude_followed_by_two_nested_selects(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Multiple_optional_navigation_with_Include(bool async)
    {
        return base.Multiple_optional_navigation_with_Include(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Multiple_optional_navigation_with_string_based_Include(bool async)
    {
        return base.Multiple_optional_navigation_with_string_based_Include(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Optional_navigation_with_Include_and_order(bool async)
    {
        return base.Optional_navigation_with_Include_and_order(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Optional_navigation_with_Include_ThenInclude(bool async)
    {
        return base.Optional_navigation_with_Include_ThenInclude(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Optional_navigation_with_order_by_and_Include(bool async)
    {
        return base.Optional_navigation_with_order_by_and_Include(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Required_navigation_with_Include(bool async)
    {
        return base.Required_navigation_with_Include(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Required_navigation_with_Include_ThenInclude(bool async)
    {
        return base.Required_navigation_with_Include_ThenInclude(async);
    }

    #endregion

    [TranslationExceedsEFCore]
    public override async Task Include_after_Select(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Include_after_Select(async));

        AssertSql("""
            SELECT [l].[$empty] AS [$empty], [l].[Id] AS [Id], [l].[Date] AS [Date], [l].[Level1_Optional_Id] AS [Level1_Optional_Id], [l].[Level1_Required_Id] AS [Level1_Required_Id], [l].[Name] AS [Name], (
                SELECT [l_0].[Id] AS [Id], [l_0].[Level2_Optional_Id] AS [Level2_Optional_Id], [l_0].[Level2_Required_Id] AS [Level2_Required_Id], [l_0].[Name] AS [Name]
                FROM [LevelThree] AS [l_0]
                WHERE [l].[Id] = [l_0].[OneToMany_Optional_Inverse3Id]
                FOR JSON PATH
            ) AS [OneToMany_Optional2]
            FROM [LevelOne] AS [l1]
            LEFT JOIN (
                SELECT 0 AS [$empty], [l_1].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id], [l_1].[OneToMany_Optional_Self_Inverse2Id] AS [OneToMany_Optional_Self_Inverse2Id], [l_1].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id], [l_1].[OneToMany_Required_Self_Inverse2Id] AS [OneToMany_Required_Self_Inverse2Id], [l_1].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id], [l_1].[OneToOne_Optional_Self2Id] AS [OneToOne_Optional_Self2Id], [l_1].[Id] AS [Id], [l_1].[Date] AS [Date], [l_1].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_1].[Level1_Required_Id] AS [Level1_Required_Id], [l_1].[Name] AS [Name]
                FROM [LevelTwo] AS [l_1]
            ) AS [l] ON [l1].[Id] = [l].[Level1_Optional_Id]
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Include_after_SelectMany_and_reference_navigation(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Include_after_SelectMany_and_reference_navigation(async));

        AssertSql("""
            SELECT [l].[$empty] AS [$empty], [l].[Id] AS [Id], [l].[Level2_Optional_Id] AS [Level2_Optional_Id], [l].[Level2_Required_Id] AS [Level2_Required_Id], [l].[Name] AS [Name], (
                SELECT [l_0].[Id] AS [Id], [l_0].[Level3_Optional_Id] AS [Level3_Optional_Id], [l_0].[Level3_Required_Id] AS [Level3_Required_Id], [l_0].[Name] AS [Name]
                FROM [LevelFour] AS [l_0]
                WHERE [l].[Id] = [l_0].[OneToMany_Optional_Inverse4Id]
                FOR JSON PATH
            ) AS [OneToMany_Optional3]
            FROM [LevelOne] AS [l1]
            INNER JOIN [LevelTwo] AS [l_1] ON [l1].[Id] = [l_1].[OneToMany_Required_Inverse2Id]
            LEFT JOIN (
                SELECT 0 AS [$empty], [l_2].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id], [l_2].[OneToMany_Optional_Self_Inverse3Id] AS [OneToMany_Optional_Self_Inverse3Id], [l_2].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id], [l_2].[OneToMany_Required_Self_Inverse3Id] AS [OneToMany_Required_Self_Inverse3Id], [l_2].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id], [l_2].[OneToOne_Optional_Self3Id] AS [OneToOne_Optional_Self3Id], [l_2].[Id] AS [Id], [l_2].[Level2_Optional_Id] AS [Level2_Optional_Id], [l_2].[Level2_Required_Id] AS [Level2_Required_Id], [l_2].[Name] AS [Name]
                FROM [LevelThree] AS [l_2]
            ) AS [l] ON [l_1].[Id] = [l].[Level2_Optional_Id]
            """);
    }

    [Theory(Skip = ClientEval)]
    public override Task SelectMany_over_conditional_empty_source(bool async)
    {
        return base.SelectMany_over_conditional_empty_source(async);
    }
}