using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using static Impatient.EFCore.Tests.Query.TPTGearsOfWarQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class TPTGearsOfWarQueryImpatientTest : TPTGearsOfWarQueryRelationalTestBase<Fixture>
{
    public TPTGearsOfWarQueryImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => base.Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(Fixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : TPTGearsOfWarQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    [Theory(Skip = ClientEval)]
    public override Task Client_eval_followed_by_aggregate_operation(bool async)
    {
        return base.Client_eval_followed_by_aggregate_operation(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Client_member_and_unsupported_string_Equals_in_the_same_query(bool async)
    {
        return base.Client_member_and_unsupported_string_Equals_in_the_same_query(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Client_method_on_collection_navigation_in_additional_from_clause(bool async)
    {
        return base.Client_method_on_collection_navigation_in_additional_from_clause(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Client_method_on_collection_navigation_in_order_by(bool async)
    {
        return base.Client_method_on_collection_navigation_in_order_by(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Client_method_on_collection_navigation_in_predicate(bool async)
    {
        return base.Client_method_on_collection_navigation_in_predicate(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property(bool async)
    {
        return base.Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Client_side_equality_with_parameter_works_with_optional_navigations(bool async)
    {
        return base.Client_side_equality_with_parameter_works_with_optional_navigations(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Correlated_collection_order_by_constant_null_of_non_mapped_type(bool async)
    {
        return base.Correlated_collection_order_by_constant_null_of_non_mapped_type(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task GetValueOrDefault_on_DateTimeOffset(bool async)
    {
        return base.GetValueOrDefault_on_DateTimeOffset(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Group_by_with_aggregate_max_on_entity_type(bool async)
    {
        return base.Group_by_with_aggregate_max_on_entity_type(async);
    }

    [TranslationExceedsEFCore]
    [TestCaseRewritten]
    public override async Task Include_after_select_with_cast_throws(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Faction>().Where(f => f is LocustHorde).Select(f => (LocustHorde)f).Include(h => h.Commander),
            elementAsserter: (e, a) => QueryAsserter.AssertInclude(e, a, [new ExpectedInclude<LocustHorde>(h => h.Commander)]));

        AssertSql("""
            SELECT [l].[$empty] AS [Commander.$empty], [l].[Item1] AS [Commander.Item1], [l].[Item2] AS [Commander.Item2], [l].[Item3] AS [Commander.Item3], [l].[Item4] AS [Commander.Item4], [l].[Item5] AS [Commander.Item5], [l].[Item6] AS [Commander.Item6], [l].[Item7] AS [Commander.Item7], [l].[Rest.Item1] AS [Commander.Rest.Item1], [l].[Rest.Item2] AS [Commander.Rest.Item2], [l].[Rest.Item3] AS [Commander.Rest.Item3], [f].[Id] AS [Item1], [l_0].[Id] AS [Item2], [f].[CapitalName] AS [Item3], [f].[Name] AS [Item4], [f].[ServerAddress] AS [Item5], [l_0].[CommanderName] AS [Item6], [l_0].[Eradicated] AS [Item7], CASE
                WHEN [l_0].[Id] IS NOT NULL THEN N'LocustHorde'
            END AS [Rest.Item1]
            FROM [Factions] AS [f]
            LEFT JOIN [LocustHordes] AS [l_0] ON [f].[Id] = [l_0].[Id]
            LEFT JOIN (
                SELECT 0 AS [$empty], [l_1].[Name] AS [Item1], [l_2].[Name] AS [Item2], [l_1].[LocustHordeId] AS [Item3], [l_1].[ThreatLevel] AS [Item4], [l_1].[ThreatLevelByte] AS [Item5], [l_1].[ThreatLevelNullableByte] AS [Item6], [l_2].[DefeatedByNickname] AS [Item7], [l_2].[DefeatedBySquadId] AS [Rest.Item1], [l_2].[HighCommandId] AS [Rest.Item2], CASE
                    WHEN [l_2].[Name] IS NOT NULL THEN N'LocustCommander'
                END AS [Rest.Item3]
                FROM [LocustCommanders] AS [l_2]
                INNER JOIN [LocustLeaders] AS [l_1] ON [l_2].[Name] = [l_1].[Name]
            ) AS [l] ON [l_0].[CommanderName] = [l].[Item1]
            WHERE CASE
                WHEN [l_0].[Id] IS NOT NULL THEN N'LocustHorde'
            END = N'LocustHorde'
            """);
    }

    [TranslationExceedsEFCore]
    [TestCaseRewritten]
    public override async Task Include_after_select_with_entity_projection_throws(bool async)
    {
        await AssertQuery(
            async,
            ss => ss.Set<Faction>().Select(f => f.Capital).Include(c => c.BornGears),
            elementAsserter: (e, a) => QueryAsserter.AssertInclude(e, a, [new ExpectedInclude<City>(c => c.BornGears)]));

        AssertSql("""
            SELECT [c].[$empty] AS [$empty], [c].[Name] AS [Name], [c].[Location] AS [Location], [c].[Nation] AS [Nation], (
                SELECT [g].[Nickname] AS [Item1], [o].[Nickname] AS [Item2], [g].[SquadId] AS [Item3], [o].[SquadId] AS [Item4], [g].[AssignedCityName] AS [Item5], [g].[CityOfBirthName] AS [Item6], [g].[FullName] AS [Item7], [g].[HasSoulPatch] AS [Rest.Item1], [g].[LeaderNickname] AS [Rest.Item2], [g].[LeaderSquadId] AS [Rest.Item3], [g].[Rank] AS [Rest.Item4], CASE
                    WHEN [o].[Nickname] IS NOT NULL THEN N'Officer'
                    WHEN [g].[Nickname] IS NOT NULL THEN N'Gear'
                END AS [Rest.Item5]
                FROM [Gears] AS [g]
                LEFT JOIN [Officers] AS [o] ON ([g].[Nickname] = [o].[Nickname]) AND ([g].[SquadId] = [o].[SquadId])
                WHERE [c].[Name] = [g].[CityOfBirthName]
                FOR JSON PATH
            ) AS [BornGears]
            FROM [Factions] AS [f]
            LEFT JOIN [LocustHordes] AS [l] ON [f].[Id] = [l].[Id]
            LEFT JOIN (
                SELECT 0 AS [$empty], [c_0].[Name] AS [Name], [c_0].[Location] AS [Location], [c_0].[Nation] AS [Nation]
                FROM [Cities] AS [c_0]
            ) AS [c] ON [f].[CapitalName] = [c].[Name]
            """);
    }

    [Fact(Skip = ClientEval)]
    public override Task Nav_rewrite_Distinct_with_convert()
    {
        return base.Nav_rewrite_Distinct_with_convert();
    }

    [Fact(Skip = ClientEval)]
    public override Task Nav_rewrite_Distinct_with_convert_anonymous()
    {
        return base.Nav_rewrite_Distinct_with_convert_anonymous();
    }

    [Theory(Skip = ClientEval)]
    public override Task Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(bool async)
    {
        return base.Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(async);
    }

    [Theory(Skip = OrderByEntity)]
    public override Task Order_by_entity_qsre(bool async)
    {
        return base.Order_by_entity_qsre(async);
    }

    [Theory(Skip = OrderByEntity)]
    public override Task Order_by_entity_qsre_composite_key(bool async)
    {
        return base.Order_by_entity_qsre_composite_key(async);
    }

    [Theory(Skip = OrderByEntity)]
    public override Task Order_by_entity_qsre_with_inheritance(bool async)
    {
        return base.Order_by_entity_qsre_with_inheritance(async);
    }

    [Theory(Skip = OrderByEntity)]
    public override Task Order_by_entity_qsre_with_other_orderbys(bool async)
    {
        return base.Order_by_entity_qsre_with_other_orderbys(async);
    }

    public override async Task Project_one_value_type_with_client_projection_from_empty_collection(bool async)
    {
        await base.Project_one_value_type_with_client_projection_from_empty_collection(async);

        AssertSql("""
            SELECT [s].[Id] AS [$outer.Id], [s].[Banner] AS [$outer.Banner], [s].[Banner5] AS [$outer.Banner5], [s].[InternalNumber] AS [$outer.InternalNumber], [s].[Name] AS [$outer.Name], (
                SELECT [g].[Nickname] AS [Item1], [o].[Nickname] AS [Item2], [g].[SquadId] AS [Item3], [o].[SquadId] AS [Item4], [g].[AssignedCityName] AS [Item5], [g].[CityOfBirthName] AS [Item6], [g].[FullName] AS [Item7], [g].[HasSoulPatch] AS [Rest.Item1], [g].[LeaderNickname] AS [Rest.Item2], [g].[LeaderSquadId] AS [Rest.Item3], [g].[Rank] AS [Rest.Item4], CASE
                    WHEN [o].[Nickname] IS NOT NULL THEN N'Officer'
                    WHEN [g].[Nickname] IS NOT NULL THEN N'Gear'
                END AS [Rest.Item5]
                FROM [Gears] AS [g]
                LEFT JOIN [Officers] AS [o] ON ([g].[Nickname] = [o].[Nickname]) AND ([g].[SquadId] = [o].[SquadId])
                WHERE [s].[Id] = [g].[SquadId]
                FOR JSON PATH
            ) AS [$inner]
            FROM [Squads] AS [s]
            WHERE [s].[Name] = N'Kilo'
            """);
    }

    [Theory(Skip = ClientEval)]
    public override Task Select_Where_Navigation_Client(bool async)
    {
        return base.Select_Where_Navigation_Client(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Trying_to_access_unmapped_property_inside_aggregate(bool async)
    {
        return base.Trying_to_access_unmapped_property_inside_aggregate(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Trying_to_access_unmapped_property_throws_informative_error(bool async)
    {
        return base.Trying_to_access_unmapped_property_throws_informative_error(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Where_coalesce_with_anonymous_types(bool async)
    {
        return base.Where_coalesce_with_anonymous_types(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Where_compare_anonymous_types(bool async)
    {
        return base.Where_compare_anonymous_types(async);
    }
}
