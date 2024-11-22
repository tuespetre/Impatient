using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Sdk;
using static Impatient.EFCore.Tests.Query.ComplexNavigationsCollectionsSharedTypeQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class ComplexNavigationsCollectionsSharedTypeQueryImpatientTest : ComplexNavigationsCollectionsSharedTypeQueryRelationalTestBase<Fixture>
{
    public ComplexNavigationsCollectionsSharedTypeQueryImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => base.Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(Fixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : ComplexNavigationsSharedTypeQueryRelationalFixtureBase
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
                SELECT [l_0].[Id] AS [Id], [l_0].[Level2_Optional_Id] AS [Level2_Optional_Id], [l_0].[Level2_Required_Id] AS [Level2_Required_Id], [l_0].[Level3_Name] AS [Name]
                FROM [Level1] AS [l_0]
                WHERE [l].[Id] = [l_0].[OneToMany_Optional_Inverse3Id]
                FOR JSON PATH
            ) AS [OneToMany_Optional2]
            FROM [Level1] AS [l1]
            LEFT JOIN (
                SELECT 0 AS [$empty], [l_1].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id], [l_1].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id], [l_1].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id], [l_1].[Id] AS [Id], [l_1].[OneToOne_Required_PK_Date] AS [Date], [l_1].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_1].[Level1_Required_Id] AS [Level1_Required_Id], [l_1].[Level2_Name] AS [Name]
                FROM [Level1] AS [l_1]
            ) AS [l] ON [l1].[Id] = [l].[Level1_Optional_Id]
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Include_after_SelectMany_and_reference_navigation(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Include_after_SelectMany_and_reference_navigation(async));

        AssertSql("""
            SELECT [l].[$empty] AS [$empty], [l].[Id] AS [Id], [l].[Level2_Optional_Id] AS [Level2_Optional_Id], [l].[Level2_Required_Id] AS [Level2_Required_Id], [l].[Name] AS [Name], (
                SELECT [l_0].[Id] AS [Id], [l_0].[Level3_Optional_Id] AS [Level3_Optional_Id], [l_0].[Level3_Required_Id] AS [Level3_Required_Id], [l_0].[Level4_Name] AS [Name]
                FROM [Level1] AS [l_0]
                WHERE [l].[Id] = [l_0].[OneToMany_Optional_Inverse4Id]
                FOR JSON PATH
            ) AS [OneToMany_Optional3]
            FROM [Level1] AS [l1]
            INNER JOIN [Level1] AS [l_1] ON [l1].[Id] = [l_1].[OneToMany_Required_Inverse2Id]
            LEFT JOIN (
                SELECT 0 AS [$empty], [l_2].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id], [l_2].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id], [l_2].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id], [l_2].[Id] AS [Id], [l_2].[Level2_Optional_Id] AS [Level2_Optional_Id], [l_2].[Level2_Required_Id] AS [Level2_Required_Id], [l_2].[Level3_Name] AS [Name]
                FROM [Level1] AS [l_2]
            ) AS [l] ON [l_1].[Id] = [l].[Level2_Optional_Id]
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Include_collection_with_multiple_orderbys_complex(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Include_collection_with_multiple_orderbys_complex(async));

        AssertSql("""
            SELECT [l].[$empty] AS [$empty], [l].[Id] AS [Id], [l].[Date] AS [Date], [l].[Level1_Optional_Id] AS [Level1_Optional_Id], [l].[Level1_Required_Id] AS [Level1_Required_Id], [l].[Name] AS [Name], (
                SELECT [l_0].[Id] AS [Id], [l_0].[Level2_Optional_Id] AS [Level2_Optional_Id], [l_0].[Level2_Required_Id] AS [Level2_Required_Id], [l_0].[Level3_Name] AS [Name]
                FROM [Level1] AS [l_0]
                WHERE [l].[Id] = [l_0].[OneToMany_Optional_Inverse3Id]
                FOR JSON PATH
            ) AS [OneToMany_Optional2]
            FROM [Level1] AS [t]
            LEFT JOIN (
                SELECT 0 AS [$empty], [l_1].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id], [l_1].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id], [l_1].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id], [l_1].[Id] AS [Id], [l_1].[OneToOne_Required_PK_Date] AS [Date], [l_1].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_1].[Level1_Required_Id] AS [Level1_Required_Id], [l_1].[Level2_Name] AS [Name]
                FROM [Level1] AS [l_1]
            ) AS [l] ON [t].[Id] = [l].[Id]
            WHERE [l].[Id] IS NOT NULL
            ORDER BY ABS([l].[Level1_Required_Id]) + 7 ASC, [l].[Name] ASC
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Include_collection_with_multiple_orderbys_complex_repeated(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Include_collection_with_multiple_orderbys_complex_repeated(async));

        AssertSql("""
            SELECT [l].[$empty] AS [$empty], [l].[Id] AS [Id], [l].[Date] AS [Date], [l].[Level1_Optional_Id] AS [Level1_Optional_Id], [l].[Level1_Required_Id] AS [Level1_Required_Id], [l].[Name] AS [Name], (
                SELECT [l_0].[Id] AS [Id], [l_0].[Level2_Optional_Id] AS [Level2_Optional_Id], [l_0].[Level2_Required_Id] AS [Level2_Required_Id], [l_0].[Level3_Name] AS [Name]
                FROM [Level1] AS [l_0]
                WHERE [l].[Id] = [l_0].[OneToMany_Optional_Inverse3Id]
                FOR JSON PATH
            ) AS [OneToMany_Optional2]
            FROM [Level1] AS [t]
            LEFT JOIN (
                SELECT 0 AS [$empty], [l_1].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id], [l_1].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id], [l_1].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id], [l_1].[Id] AS [Id], [l_1].[OneToOne_Required_PK_Date] AS [Date], [l_1].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_1].[Level1_Required_Id] AS [Level1_Required_Id], [l_1].[Level2_Name] AS [Name]
                FROM [Level1] AS [l_1]
            ) AS [l] ON [t].[Id] = [l].[Id]
            WHERE [l].[Id] IS NOT NULL
            ORDER BY -([l].[Level1_Required_Id]) ASC, (
                SELECT -([l].[Level1_Required_Id])
            ) ASC, [l].[Name] ASC
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Include_collection_with_multiple_orderbys_complex_repeated_checked(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Include_collection_with_multiple_orderbys_complex_repeated_checked(async));

        AssertSql("""
            SELECT [l].[$empty] AS [$empty], [l].[Id] AS [Id], [l].[Date] AS [Date], [l].[Level1_Optional_Id] AS [Level1_Optional_Id], [l].[Level1_Required_Id] AS [Level1_Required_Id], [l].[Name] AS [Name], (
                SELECT [l_0].[Id] AS [Id], [l_0].[Level2_Optional_Id] AS [Level2_Optional_Id], [l_0].[Level2_Required_Id] AS [Level2_Required_Id], [l_0].[Level3_Name] AS [Name]
                FROM [Level1] AS [l_0]
                WHERE [l].[Id] = [l_0].[OneToMany_Optional_Inverse3Id]
                FOR JSON PATH
            ) AS [OneToMany_Optional2]
            FROM [Level1] AS [t]
            LEFT JOIN (
                SELECT 0 AS [$empty], [l_1].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id], [l_1].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id], [l_1].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id], [l_1].[Id] AS [Id], [l_1].[OneToOne_Required_PK_Date] AS [Date], [l_1].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_1].[Level1_Required_Id] AS [Level1_Required_Id], [l_1].[Level2_Name] AS [Name]
                FROM [Level1] AS [l_1]
            ) AS [l] ON [t].[Id] = [l].[Id]
            WHERE [l].[Id] IS NOT NULL
            ORDER BY -([l].[Level1_Required_Id]) ASC, (
                SELECT -([l].[Level1_Required_Id])
            ) ASC, [l].[Name] ASC
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Include_collection_with_multiple_orderbys_member(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Include_collection_with_multiple_orderbys_member(async));

        AssertSql("""
            SELECT [l].[$empty] AS [$empty], [l].[Id] AS [Id], [l].[Date] AS [Date], [l].[Level1_Optional_Id] AS [Level1_Optional_Id], [l].[Level1_Required_Id] AS [Level1_Required_Id], [l].[Name] AS [Name], (
                SELECT [l_0].[Id] AS [Id], [l_0].[Level2_Optional_Id] AS [Level2_Optional_Id], [l_0].[Level2_Required_Id] AS [Level2_Required_Id], [l_0].[Level3_Name] AS [Name]
                FROM [Level1] AS [l_0]
                WHERE [l].[Id] = [l_0].[OneToMany_Optional_Inverse3Id]
                FOR JSON PATH
            ) AS [OneToMany_Optional2]
            FROM [Level1] AS [t]
            LEFT JOIN (
                SELECT 0 AS [$empty], [l_1].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id], [l_1].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id], [l_1].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id], [l_1].[Id] AS [Id], [l_1].[OneToOne_Required_PK_Date] AS [Date], [l_1].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_1].[Level1_Required_Id] AS [Level1_Required_Id], [l_1].[Level2_Name] AS [Name]
                FROM [Level1] AS [l_1]
            ) AS [l] ON [t].[Id] = [l].[Id]
            WHERE [l].[Id] IS NOT NULL
            ORDER BY [l].[Name] ASC, [l].[Level1_Required_Id] ASC
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Include_collection_with_multiple_orderbys_methodcall(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Include_collection_with_multiple_orderbys_methodcall(async));

        AssertSql("""
            SELECT [l].[$empty] AS [$empty], [l].[Id] AS [Id], [l].[Date] AS [Date], [l].[Level1_Optional_Id] AS [Level1_Optional_Id], [l].[Level1_Required_Id] AS [Level1_Required_Id], [l].[Name] AS [Name], (
                SELECT [l_0].[Id] AS [Id], [l_0].[Level2_Optional_Id] AS [Level2_Optional_Id], [l_0].[Level2_Required_Id] AS [Level2_Required_Id], [l_0].[Level3_Name] AS [Name]
                FROM [Level1] AS [l_0]
                WHERE [l].[Id] = [l_0].[OneToMany_Optional_Inverse3Id]
                FOR JSON PATH
            ) AS [OneToMany_Optional2]
            FROM [Level1] AS [t]
            LEFT JOIN (
                SELECT 0 AS [$empty], [l_1].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id], [l_1].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id], [l_1].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id], [l_1].[Id] AS [Id], [l_1].[OneToOne_Required_PK_Date] AS [Date], [l_1].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_1].[Level1_Required_Id] AS [Level1_Required_Id], [l_1].[Level2_Name] AS [Name]
                FROM [Level1] AS [l_1]
            ) AS [l] ON [t].[Id] = [l].[Id]
            WHERE [l].[Id] IS NOT NULL
            ORDER BY ABS([l].[Level1_Required_Id]) ASC, [l].[Name] ASC
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Include_collection_with_multiple_orderbys_property(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Include_collection_with_multiple_orderbys_property(async));

        AssertSql("""
            SELECT [l].[$empty] AS [$empty], [l].[Id] AS [Id], [l].[Date] AS [Date], [l].[Level1_Optional_Id] AS [Level1_Optional_Id], [l].[Level1_Required_Id] AS [Level1_Required_Id], [l].[Name] AS [Name], (
                SELECT [l_0].[Id] AS [Id], [l_0].[Level2_Optional_Id] AS [Level2_Optional_Id], [l_0].[Level2_Required_Id] AS [Level2_Required_Id], [l_0].[Level3_Name] AS [Name]
                FROM [Level1] AS [l_0]
                WHERE [l].[Id] = [l_0].[OneToMany_Optional_Inverse3Id]
                FOR JSON PATH
            ) AS [OneToMany_Optional2]
            FROM [Level1] AS [t]
            LEFT JOIN (
                SELECT 0 AS [$empty], [l_1].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id], [l_1].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id], [l_1].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id], [l_1].[Id] AS [Id], [l_1].[OneToOne_Required_PK_Date] AS [Date], [l_1].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_1].[Level1_Required_Id] AS [Level1_Required_Id], [l_1].[Level2_Name] AS [Name]
                FROM [Level1] AS [l_1]
            ) AS [l] ON [t].[Id] = [l].[Id]
            WHERE [l].[Id] IS NOT NULL
            ORDER BY [l].[Level1_Required_Id] ASC, [l].[Name] ASC
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Include_inside_subquery(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Include_inside_subquery(async));

        AssertSql("""
            SELECT (
                SELECT [l].[Id] AS [Id], [l].[Date] AS [Date], [l].[Level1_Optional_Id] AS [Level1_Optional_Id], [l].[Level1_Required_Id] AS [Level1_Required_Id], [l].[Name] AS [Name], (
                    SELECT [l_0].[Id] AS [Id], [l_0].[Level2_Optional_Id] AS [Level2_Optional_Id], [l_0].[Level2_Required_Id] AS [Level2_Required_Id], [l_0].[Level3_Name] AS [Name]
                    FROM [Level1] AS [l_0]
                    WHERE [l].[Id] = [l_0].[OneToMany_Optional_Inverse3Id]
                    FOR JSON PATH
                ) AS [OneToMany_Optional2]
                FROM [Level1] AS [t]
                LEFT JOIN (
                    SELECT [l_1].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id], [l_1].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id], [l_1].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id], [l_1].[Id] AS [Id], [l_1].[OneToOne_Required_PK_Date] AS [Date], [l_1].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_1].[Level1_Required_Id] AS [Level1_Required_Id], [l_1].[Level2_Name] AS [Name]
                    FROM [Level1] AS [l_1]
                ) AS [l] ON [t].[Id] = [l].[Id]
                WHERE ([l].[Id] IS NOT NULL) AND ([l].[Id] > 0)
                FOR JSON PATH
            ) AS [subquery]
            FROM [Level1] AS [l1]
            WHERE [l1].[Id] < 3
            ORDER BY [l1].[Id] ASC
            """);
    }

    [Theory(Skip = ClientEval)]
    public override Task SelectMany_over_conditional_empty_source(bool async)
    {
        return base.SelectMany_over_conditional_empty_source(async);
    }

    [TranslationExceedsEFCore]
    public override async Task SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.SelectMany_with_navigation_and_Distinct_projecting_columns_including_join_key(async));

        AssertSql("""
            SELECT [l].[Id] AS [Id], [l].[Date] AS [Date], [l].[Name] AS [Name], (
                SELECT [l_0].[Id] AS [Id], [l_0].[OneToOne_Required_PK_Date] AS [Date], [l_0].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_0].[Level1_Required_Id] AS [Level1_Required_Id], [l_0].[Level2_Name] AS [Name]
                FROM [Level1] AS [l_0]
                WHERE [l].[Id] = [l_0].[OneToMany_Optional_Inverse2Id]
                FOR JSON PATH
            ) AS [OneToMany_Optional1]
            FROM [Level1] AS [l]
            CROSS APPLY (
                SELECT DISTINCT [l_1].[Id] AS [Id], [l_1].[Level2_Name] AS [Name], [l_1].[OneToMany_Optional_Inverse2Id] AS [FK]
                FROM [Level1] AS [l_1]
                WHERE [l].[Id] = [l_1].[OneToMany_Optional_Inverse2Id]
            ) AS [l2]
            """);
    }
}