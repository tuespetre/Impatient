using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using static Impatient.EFCore.Tests.Query.ComplexNavigationsQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class ComplexNavigationsQueryImpatientTest : ComplexNavigationsQueryRelationalTestBase<Fixture>
{
    public ComplexNavigationsQueryImpatientTest(Fixture fixture) : base(fixture)
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

    public override async Task Prune_does_not_throw_null_ref(bool async)
    {
        await base.Prune_does_not_throw_null_ref(async);

        AssertSql("""
            SELECT [l1].[Id] AS [Id], [l1].[Date] AS [Date], [l1].[Name] AS [Name]
            FROM (
                SELECT NULL AS [$empty]
            ) AS [t]
            LEFT JOIN (
                SELECT 0 AS [$empty], [i].[Level1_Required_Id]
                FROM [LevelTwo] AS [i]
                WHERE [i].[Id] < 5
            ) AS [t_0] ON 1 = 1
            CROSS APPLY (
                SELECT [x].[OneToMany_Optional_Self_Inverse1Id] AS [OneToMany_Optional_Self_Inverse1Id], [x].[OneToMany_Required_Self_Inverse1Id] AS [OneToMany_Required_Self_Inverse1Id], [x].[OneToOne_Optional_Self1Id] AS [OneToOne_Optional_Self1Id], [x].[Id] AS [Id], [x].[Date] AS [Date], [x].[Name] AS [Name]
                FROM [LevelOne] AS [x]
                WHERE (([t_0].[$empty] IS NOT NULL) AND ([t_0].[Level1_Required_Id] <> [x].[Id])) OR (([t_0].[$empty] IS NULL) AND (0 <> [x].[Id]))
            ) AS [l1]
            """);
    }

    public override async Task Query_source_materialization_bug_4547(bool async)
    {
        await base.Query_source_materialization_bug_4547(async);

        // WHERE 1 = 1
        // is incorrect.

        AssertSql("""
            SELECT [e1].[Id]
            FROM [LevelThree] AS [e3]
            INNER JOIN [LevelOne] AS [e1] ON [e3].[Id] = (
                SELECT TOP (1) (CASE WHEN [subQuery3].[Id] IS NOT NULL THEN [subQuery3].[Id] ELSE NULL END)
                FROM [LevelTwo] AS [subQuery2]
                LEFT JOIN (
                    SELECT 0 AS [$empty], [subQuery3_0].[OneToMany_Optional_Inverse3Id] AS [OneToMany_Optional_Inverse3Id], [subQuery3_0].[OneToMany_Optional_Self_Inverse3Id] AS [OneToMany_Optional_Self_Inverse3Id], [subQuery3_0].[OneToMany_Required_Inverse3Id] AS [OneToMany_Required_Inverse3Id], [subQuery3_0].[OneToMany_Required_Self_Inverse3Id] AS [OneToMany_Required_Self_Inverse3Id], [subQuery3_0].[OneToOne_Optional_PK_Inverse3Id] AS [OneToOne_Optional_PK_Inverse3Id], [subQuery3_0].[OneToOne_Optional_Self3Id] AS [OneToOne_Optional_Self3Id], [subQuery3_0].[Id] AS [Id], [subQuery3_0].[Level2_Optional_Id] AS [Level2_Optional_Id], [subQuery3_0].[Level2_Required_Id] AS [Level2_Required_Id], [subQuery3_0].[Name] AS [Name]
                    FROM [LevelThree] AS [subQuery3_0]
                ) AS [subQuery3] ON [subQuery2].[Id] = [subQuery3].[Level2_Optional_Id]
                WHERE 1 = 1
                ORDER BY (CASE WHEN [subQuery3].[Id] IS NOT NULL THEN [subQuery3].[Id] ELSE NULL END) ASC
            )
            """);
    }

    public override Task Required_navigation_on_a_subquery_with_complex_projection_and_First(bool async)
    {
        // Fails because I still need to implement projection splitting

        return base.Required_navigation_on_a_subquery_with_complex_projection_and_First(async);
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

    public override Task Select_subquery_with_client_eval_and_navigation1(bool async)
    {
        // Fails because I still need to implement projection splitting

        return base.Select_subquery_with_client_eval_and_navigation1(async);
    }
}