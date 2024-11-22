using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using static Impatient.EFCore.Tests.Query.ComplexNavigationsSharedTypeQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class ComplexNavigationsSharedTypeQueryImpatientTest : ComplexNavigationsSharedTypeQueryRelationalTestBase<Fixture>
{
    public ComplexNavigationsSharedTypeQueryImpatientTest(Fixture fixture) : base(fixture)
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

    // I'm not sure I agree with the EF Core reasoning here.
    // It seems like an opinionated decision where I just have a different opinion.
    [DisagreeWithEFCore]
    [Theory(Skip = Punt)]
    public override Task Comparing_collection_navigation_on_optional_reference_to_null(bool async)
    {
        return base.Comparing_collection_navigation_on_optional_reference_to_null(async);
    }

    [Theory(Skip = BadMaterialization)]
    public override Task Complex_query_with_let_collection_SelectMany(bool async)
    {
        return base.Complex_query_with_let_collection_SelectMany(async);
    }

    // We currently put out a basic "[Column] IN ({subquery})", when it should be "EXISTS ({subquery} WHERE [Column] IS NULL AND [Other] IS NULL OR ..."
    [Trait("Translation", "Contains to EXISTS")]
    public override Task Contains_over_optional_navigation_with_null_column(bool async)
    {
        return base.Contains_over_optional_navigation_with_null_column(async);
    }

    [Trait("Translation", "Contains to EXISTS")]
    public override Task Contains_over_optional_navigation_with_null_constant(bool async)
    {
        return base.Contains_over_optional_navigation_with_null_constant(async);
    }

    [Trait("Translation", "Contains to EXISTS")]
    public override Task Contains_over_optional_navigation_with_null_entity_reference(bool async)
    {
        return base.Contains_over_optional_navigation_with_null_entity_reference(async);
    }

    [Trait("Translation", "Contains to EXISTS")]
    public override Task Contains_over_optional_navigation_with_null_parameter(bool async)
    {
        return base.Contains_over_optional_navigation_with_null_parameter(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Include12(bool async)
    {
        return base.Include12(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Include14(bool async)
    {
        return base.Include14(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Include18_3_3(bool async)
    {
        return base.Include18_3_3(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Include6(bool async)
    {
        return base.Include6(async);
    }

    [Theory(Skip = TranslationBeyondEF)]
    public override Task Include8(bool async)
    {
        return base.Include8(async);
    }

    [Theory(Skip = TranslationBeyondEF)]
    public override Task Include9(bool async)
    {
        return base.Include9(async);
    }

    [Theory(Skip = TranslationBeyondEF)]
    public override Task Level4_Include(bool async)
    {
        return base.Level4_Include(async);
    }

    [Theory(Skip = TranslationBeyondEF)]
    public override Task Multiple_required_navigation_using_multiple_selects_with_Include(bool async)
    {
        return base.Multiple_required_navigation_using_multiple_selects_with_Include(async);
    }

    [Theory(Skip = TranslationBeyondEF)]
    public override Task Multiple_required_navigation_using_multiple_selects_with_string_based_Include(bool async)
    {
        return base.Multiple_required_navigation_using_multiple_selects_with_string_based_Include(async);
    }

    [Theory(Skip = TranslationBeyondEF)]
    public override Task Multiple_required_navigation_with_string_based_Include(bool async)
    {
        return base.Multiple_required_navigation_with_string_based_Include(async);
    }

    [Theory(Skip = TranslationBeyondEF)]
    public override Task Multiple_required_navigations_with_Include(bool async)
    {
        return base.Multiple_required_navigations_with_Include(async);
    }

    public override async Task Nested_group_join_with_take(bool async)
    {
        await base.Nested_group_join_with_take(async);

        // TODO: holy shit this SQL is crazy.
        AssertSql("""
            SELECT JSON_VALUE([l2_outer].[value], N'$.Name')
            FROM (
                SELECT TOP (2) [l1_inner].[Id] AS [$outer.$outer.$outer.$outer.Id], [l1_inner].[Date] AS [$outer.$outer.$outer.$outer.Date], [l1_inner].[Name] AS [$outer.$outer.$outer.$outer.Name], [l1_inner].[Id] AS [$outer.$outer.$outer.$inner.l1_inner.Id], [l1_inner].[Date] AS [$outer.$outer.$outer.$inner.l1_inner.Date], [l1_inner].[Name] AS [$outer.$outer.$outer.$inner.l1_inner.Name], (
                    SELECT [l].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id], [l].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id], [l].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id], [l].[Id] AS [Id], [l].[Date] AS [Date], [l].[Level1_Optional_Id] AS [Level1_Optional_Id], [l].[Level1_Required_Id] AS [Level1_Required_Id], [l].[Name] AS [Name]
                    FROM [Level1] AS [t]
                    LEFT JOIN (
                        SELECT [l_0].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id], [l_0].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id], [l_0].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id], [l_0].[Id] AS [Id], [l_0].[OneToOne_Required_PK_Date] AS [Date], [l_0].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_0].[Level1_Required_Id] AS [Level1_Required_Id], [l_0].[Level2_Name] AS [Name]
                        FROM [Level1] AS [l_0]
                    ) AS [l] ON [t].[Id] = [l].[Id]
                    WHERE ([l].[Id] IS NOT NULL) AND ([l1_inner].[Id] = [l].[Level1_Optional_Id])
                    FOR JSON PATH
                ) AS [$outer.$outer.$outer.$inner.grouping_inner], [l2_inner].[$empty] AS [$outer.$outer.$inner.$empty], [l2_inner].[OneToMany_Optional_Inverse2Id] AS [$outer.$outer.$inner.OneToMany_Optional_Inverse2Id], [l2_inner].[OneToMany_Required_Inverse2Id] AS [$outer.$outer.$inner.OneToMany_Required_Inverse2Id], [l2_inner].[OneToOne_Optional_PK_Inverse2Id] AS [$outer.$outer.$inner.OneToOne_Optional_PK_Inverse2Id], [l2_inner].[Id] AS [$outer.$outer.$inner.Id], [l2_inner].[Date] AS [$outer.$outer.$inner.Date], [l2_inner].[Level1_Optional_Id] AS [$outer.$outer.$inner.Level1_Optional_Id], [l2_inner].[Level1_Required_Id] AS [$outer.$outer.$inner.Level1_Required_Id], [l2_inner].[Name] AS [$outer.$outer.$inner.Name], [l2_inner].[$empty] AS [$outer.$inner.$empty], [l2_inner].[OneToMany_Optional_Inverse2Id] AS [$outer.$inner.OneToMany_Optional_Inverse2Id], [l2_inner].[OneToMany_Required_Inverse2Id] AS [$outer.$inner.OneToMany_Required_Inverse2Id], [l2_inner].[OneToOne_Optional_PK_Inverse2Id] AS [$outer.$inner.OneToOne_Optional_PK_Inverse2Id], [l2_inner].[Id] AS [$outer.$inner.Id], [l2_inner].[Date] AS [$outer.$inner.Date], [l2_inner].[Level1_Optional_Id] AS [$outer.$inner.Level1_Optional_Id], [l2_inner].[Level1_Required_Id] AS [$outer.$inner.Level1_Required_Id], [l2_inner].[Name] AS [$outer.$inner.Name], [l2_inner].[$empty] AS [$inner.l1_outer.$empty], [l2_inner].[OneToMany_Optional_Inverse2Id] AS [$inner.l1_outer.OneToMany_Optional_Inverse2Id], [l2_inner].[OneToMany_Required_Inverse2Id] AS [$inner.l1_outer.OneToMany_Required_Inverse2Id], [l2_inner].[OneToOne_Optional_PK_Inverse2Id] AS [$inner.l1_outer.OneToOne_Optional_PK_Inverse2Id], [l2_inner].[Id] AS [$inner.l1_outer.Id], [l2_inner].[Date] AS [$inner.l1_outer.Date], [l2_inner].[Level1_Optional_Id] AS [$inner.l1_outer.Level1_Optional_Id], [l2_inner].[Level1_Required_Id] AS [$inner.l1_outer.Level1_Required_Id], [l2_inner].[Name] AS [$inner.l1_outer.Name], (
                    SELECT [l_1].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id], [l_1].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id], [l_1].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id], [l_1].[Id] AS [Id], [l_1].[Date] AS [Date], [l_1].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_1].[Level1_Required_Id] AS [Level1_Required_Id], [l_1].[Name] AS [Name]
                    FROM [Level1] AS [t_0]
                    LEFT JOIN (
                        SELECT [l_2].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id], [l_2].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id], [l_2].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id], [l_2].[Id] AS [Id], [l_2].[OneToOne_Required_PK_Date] AS [Date], [l_2].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_2].[Level1_Required_Id] AS [Level1_Required_Id], [l_2].[Level2_Name] AS [Name]
                        FROM [Level1] AS [l_2]
                    ) AS [l_1] ON [t_0].[Id] = [l_1].[Id]
                    WHERE ([l_1].[Id] IS NOT NULL) AND ([l2_inner].[Id] = [l_1].[Level1_Optional_Id])
                    FOR JSON PATH
                ) AS [$inner.grouping_outer]
                FROM [Level1] AS [l1_inner]
                OUTER APPLY (
                    SELECT [l_3].[$empty] AS [$empty], [l_3].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id], [l_3].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id], [l_3].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id], [l_3].[Id] AS [Id], [l_3].[Date] AS [Date], [l_3].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_3].[Level1_Required_Id] AS [Level1_Required_Id], [l_3].[Name] AS [Name]
                    FROM [Level1] AS [t_1]
                    LEFT JOIN (
                        SELECT 0 AS [$empty], [l_4].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id], [l_4].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id], [l_4].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id], [l_4].[Id] AS [Id], [l_4].[OneToOne_Required_PK_Date] AS [Date], [l_4].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_4].[Level1_Required_Id] AS [Level1_Required_Id], [l_4].[Level2_Name] AS [Name]
                        FROM [Level1] AS [l_4]
                    ) AS [l_3] ON [t_1].[Id] = [l_3].[Id]
                    WHERE ([l_3].[Id] IS NOT NULL) AND ([l1_inner].[Id] = [l_3].[Level1_Optional_Id])
                ) AS [l2_inner]
                ORDER BY [l1_inner].[Id] ASC
            ) AS [t_2]
            OUTER APPLY (
                SELECT 0 AS [$empty], [j].[value]
                FROM OPENJSON([t_2].[$inner.grouping_outer]) AS [j]
            ) AS [l2_outer]
            """);
    }

    [Theory(Skip = TranslationBeyondEF)]
    public override Task Optional_navigation_with_Include(bool async)
    {
        return base.Optional_navigation_with_Include(async);
    }

    public override Task Project_shadow_properties1(bool async)
    {
        return base.Project_shadow_properties1(async);
    }

    public override Task Project_shadow_properties2(bool async)
    {
        return base.Project_shadow_properties2(async);
    }

    public override Task Project_shadow_properties3(bool async)
    {
        return base.Project_shadow_properties3(async);
    }

    public override Task Project_shadow_properties4(bool async)
    {
        return base.Project_shadow_properties4(async);
    }

    public override Task Project_shadow_properties9(bool async)
    {
        return base.Project_shadow_properties9(async);
    }

    public override async Task Prune_does_not_throw_null_ref(bool async)
    {
        await base.Prune_does_not_throw_null_ref(async);

        AssertSql("""
            SELECT [l1].[Id] AS [Id], [l1].[Date] AS [Date], [l1].[Name] AS [Name]
            FROM (
                SELECT NULL AS [$empty]
            ) AS [t]
            LEFT JOIN (
                SELECT 0 AS [$empty], [l].[Level1_Required_Id]
                FROM [Level1] AS [t_0]
                LEFT JOIN (
                    SELECT 0 AS [$empty], [l_0].[OneToMany_Optional_Inverse2Id] AS [OneToMany_Optional_Inverse2Id], [l_0].[OneToMany_Required_Inverse2Id] AS [OneToMany_Required_Inverse2Id], [l_0].[OneToOne_Optional_PK_Inverse2Id] AS [OneToOne_Optional_PK_Inverse2Id], [l_0].[Id] AS [Id], [l_0].[OneToOne_Required_PK_Date] AS [Date], [l_0].[Level1_Optional_Id] AS [Level1_Optional_Id], [l_0].[Level1_Required_Id] AS [Level1_Required_Id], [l_0].[Level2_Name] AS [Name]
                    FROM [Level1] AS [l_0]
                ) AS [l] ON [t_0].[Id] = [l].[Id]
                WHERE ([l].[Id] IS NOT NULL) AND ([l].[Id] < 5)
            ) AS [t_1] ON 1 = 1
            CROSS APPLY (
                SELECT [x].[Id] AS [Id], [x].[Date] AS [Date], [x].[Name] AS [Name]
                FROM [Level1] AS [x]
                WHERE (([t_1].[$empty] IS NOT NULL) AND (([t_1].[Level1_Required_Id] IS NULL OR ([t_1].[Level1_Required_Id] <> [x].[Id])))) OR (([t_1].[$empty] IS NULL) AND (0 <> [x].[Id]))
            ) AS [l1]
            """);
    }

    [Theory(Skip = BadMaterialization)]
    public override Task Select_projecting_queryable_followed_by_Join(bool async)
    {
        return base.Select_projecting_queryable_followed_by_Join(async);
    }

    [Theory(Skip = BadMaterialization)]
    public override Task Select_projecting_queryable_followed_by_SelectMany(bool async)
    {
        return base.Select_projecting_queryable_followed_by_SelectMany(async);
    }

    [Theory(Skip = BadMaterialization)]
    public override Task Select_projecting_queryable_in_anonymous_projection_followed_by_Join(bool async)
    {
        return base.Select_projecting_queryable_in_anonymous_projection_followed_by_Join(async);
    }
}