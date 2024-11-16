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