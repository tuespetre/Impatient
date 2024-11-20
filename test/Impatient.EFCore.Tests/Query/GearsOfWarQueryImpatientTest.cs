using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Sdk;
using static Impatient.EFCore.Tests.Query.GearsOfWarQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class GearsOfWarQueryImpatientTest : GearsOfWarQueryRelationalTestBase<Fixture>
{
    public GearsOfWarQueryImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => base.Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(Fixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : GearsOfWarQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    public override Task Bitwise_operation_with_null_arguments(bool async)
    {
        return base.Bitwise_operation_with_null_arguments(async);
    }

    // we don't translate casts to derived types. it seems like a very niche use case, and not a 'proper' use case at that.
    [DisagreeWithEFCore]
    [Theory(Skip = Punt)]
    public override Task Cast_to_derived_followed_by_include_and_FirstOrDefault(bool async)
    {
        return base.Cast_to_derived_followed_by_include_and_FirstOrDefault(async);
    }

    [DisagreeWithEFCore]
    [Theory(Skip = Punt)]
    public override Task Cast_to_derived_followed_by_multiple_includes(bool async)
    {
        return base.Cast_to_derived_followed_by_multiple_includes(async);
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

    [TranslationExceedsEFCore]
    public override async Task Correlated_collection_after_distinct_3_levels_without_original_identifiers(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Correlated_collection_after_distinct_3_levels_without_original_identifiers(async));

        AssertSql("""
            SELECT [x].[Length] AS [Length], (
                SELECT [xx].[HasSoulPatch] AS [HasSoulPatch], (
                    SELECT [w].[Id] AS [Id], [x].[Length] AS [Length], [xx].[HasSoulPatch] AS [HasSoulPatch]
                    FROM [Weapons] AS [w]
                    WHERE [w].[OwnerFullName] = [xx].[CityOfBirthName]
                    FOR JSON PATH
                ) AS [Subquery2]
                FROM (
                    SELECT DISTINCT [g].[HasSoulPatch] AS [HasSoulPatch], [g].[CityOfBirthName] AS [CityOfBirthName]
                    FROM [Gears] AS [g]
                    WHERE [g].[Discriminator] IN (N'Gear', N'Officer') AND (CAST(LEN([g].[Nickname]) AS int) = [x].[Length])
                ) AS [xx]
                FOR JSON PATH
            ) AS [Subquery1]
            FROM (
                SELECT DISTINCT CAST(LEN([s].[Name]) AS int) AS [Length]
                FROM [Squads] AS [s]
            ) AS [x]
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Correlated_collection_with_distinct_not_projecting_identifier_column_also_projecting_complex_expressions(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Correlated_collection_with_distinct_not_projecting_identifier_column_also_projecting_complex_expressions(async));

        AssertSql("""
            SELECT [g].[Nickname] AS [Key], (
                SELECT DISTINCT [w].[Name] AS [Name], [w].[IsAutomatic] AS [IsAutomatic], CAST(LEN([w].[OwnerFullName]) AS int) AS [Length]
                FROM [Weapons] AS [w]
                WHERE [g].[FullName] = [w].[OwnerFullName]
                FOR JSON PATH
            ) AS [Subquery]
            FROM [Gears] AS [g]
            WHERE [g].[Discriminator] IN (N'Gear', N'Officer')
            """);
    }

    [Theory(Skip = ClientEval)]
    public override Task Correlated_collection_order_by_constant_null_of_non_mapped_type(bool async)
    {
        return base.Correlated_collection_order_by_constant_null_of_non_mapped_type(async);
    }

    [Theory(Skip = ManualLeftJoinNullabilityPropagation)]
    public override async Task Correlated_collections_deeply_nested_left_join(bool async)
    {
        await base.Correlated_collections_deeply_nested_left_join(async);

        AssertSql("""
            SELECT (
                SELECT [g].[Nickname] AS [Nickname], (
                    SELECT [w].[Id] AS [Id], [w].[AmmunitionType] AS [AmmunitionType], [w].[IsAutomatic] AS [IsAutomatic], [w].[Name] AS [Name], [w].[OwnerFullName] AS [OwnerFullName], [w].[SynergyWithId] AS [SynergyWithId]
                    FROM [Weapons] AS [w]
                    WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[IsAutomatic] = 1)
                    FOR JSON PATH
                ) AS [AutomaticWeapons]
                FROM [Gears] AS [g]
                WHERE ([g].[Discriminator] IN (N'Gear', N'Officer') AND ([s].[Id] = [g].[SquadId])) AND ([g].[HasSoulPatch] = 1)
                FOR JSON PATH
            )
            FROM [Tags] AS [t]
            LEFT JOIN (
                SELECT 0 AS [$empty], [g_0].[Nickname] AS [Item1], [g_0].[SquadId] AS [Item2], [g_0].[AssignedCityName] AS [Item3], [g_0].[CityOfBirthName] AS [Item4], [g_0].[Discriminator] AS [Item5], [g_0].[FullName] AS [Item6], [g_0].[HasSoulPatch] AS [Item7], [g_0].[LeaderNickname] AS [Rest.Item1], [g_0].[LeaderSquadId] AS [Rest.Item2], [g_0].[Rank] AS [Rest.Item3]
                FROM [Gears] AS [g_0]
                WHERE [g_0].[Discriminator] IN (N'Gear', N'Officer')
            ) AS [g_1] ON [t].[GearNickName] = [g_1].[Item1]
            LEFT JOIN (
                SELECT 0 AS [$empty], [s_0].[Id] AS [Id], [s_0].[Banner] AS [Banner], [s_0].[Banner5] AS [Banner5], [s_0].[InternalNumber] AS [InternalNumber], [s_0].[Name] AS [Name]
                FROM [Squads] AS [s_0]
            ) AS [s] ON [g_1].[Item2] = [s].[Id]
            ORDER BY [t].[Note] ASC, [g_1].[Item1] DESC
            """);
    }

    [Theory(Skip = ClientEval)]
    public override Task GetValueOrDefault_on_DateTimeOffset(bool async)
    {
        return base.GetValueOrDefault_on_DateTimeOffset(async);
    }

    [ConditionalTheory(Skip = ClientEval)]
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
            SELECT [l].[$empty] AS [Commander.$empty], [l].[Item1] AS [Commander.Item1], [l].[Item2] AS [Commander.Item2], [l].[Item3] AS [Commander.Item3], [l].[Item4] AS [Commander.Item4], [l].[Item5] AS [Commander.Item5], [l].[Item6] AS [Commander.Item6], [l].[Item7] AS [Commander.Item7], [l].[Rest.Item1] AS [Commander.Rest.Item1], [l].[Rest.Item2] AS [Commander.Rest.Item2], [f].[Id] AS [Item1], [f].[CapitalName] AS [Item2], [f].[Discriminator] AS [Item3], [f].[Name] AS [Item4], [f].[ServerAddress] AS [Item5], [f].[CommanderName] AS [Item6], [f].[Eradicated] AS [Item7]
            FROM [Factions] AS [f]
            LEFT JOIN (
                SELECT 0 AS [$empty], [l_0].[Name] AS [Item1], [l_0].[Discriminator] AS [Item2], [l_0].[LocustHordeId] AS [Item3], [l_0].[ThreatLevel] AS [Item4], [l_0].[ThreatLevelByte] AS [Item5], [l_0].[ThreatLevelNullableByte] AS [Item6], [l_0].[DefeatedByNickname] AS [Item7], [l_0].[DefeatedBySquadId] AS [Rest.Item1], [l_0].[HighCommandId] AS [Rest.Item2]
                FROM [LocustLeaders] AS [l_0]
                WHERE [l_0].[Discriminator] IN (N'LocustCommander')
            ) AS [l] ON [f].[CommanderName] = [l].[Item1]
            WHERE [f].[Discriminator] IN (N'LocustHorde') AND ([f].[Discriminator] = N'LocustHorde')
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
                SELECT [g].[Nickname] AS [Item1], [g].[SquadId] AS [Item2], [g].[AssignedCityName] AS [Item3], [g].[CityOfBirthName] AS [Item4], [g].[Discriminator] AS [Item5], [g].[FullName] AS [Item6], [g].[HasSoulPatch] AS [Item7], [g].[LeaderNickname] AS [Rest.Item1], [g].[LeaderSquadId] AS [Rest.Item2], [g].[Rank] AS [Rest.Item3]
                FROM [Gears] AS [g]
                WHERE [g].[Discriminator] IN (N'Gear', N'Officer') AND ([c].[Name] = [g].[CityOfBirthName])
                FOR JSON PATH
            ) AS [BornGears]
            FROM [Factions] AS [f]
            LEFT JOIN (
                SELECT 0 AS [$empty], [c_0].[Name] AS [Name], [c_0].[Location] AS [Location], [c_0].[Nation] AS [Nation]
                FROM [Cities] AS [c_0]
            ) AS [c] ON [f].[CapitalName] = [c].[Name]
            WHERE [f].[Discriminator] IN (N'LocustHorde')
            """);
    }

    public override async Task Join_with_complex_key_selector(bool async)
    {
        await base.Join_with_complex_key_selector(async);

        AssertSql("""
            SELECT [o].[Id] AS [Id], [i].[Id] AS [TagId]
            FROM [Squads] AS [o]
            INNER JOIN (
                SELECT [t].[Id] AS [Id], [t].[GearNickName] AS [GearNickName], [t].[GearSquadId] AS [GearSquadId], [t].[IssueDate] AS [IssueDate], [t].[Note] AS [Note]
                FROM [Tags] AS [t]
                WHERE [t].[Note] = N'Marcus'' Tag'
            ) AS [i] ON 1 = 1
            """);
    }

    public override async Task Logical_operation_with_non_null_parameter_optimizes_null_checks(bool async)
    {
        await base.Logical_operation_with_non_null_parameter_optimizes_null_checks(async);

        AssertSql("""
            @p0='True'
            @p1='True'

            SELECT [g].[Nickname] AS [Item1], [g].[SquadId] AS [Item2], [g].[AssignedCityName] AS [Item3], [g].[CityOfBirthName] AS [Item4], [g].[Discriminator] AS [Item5], [g].[FullName] AS [Item6], [g].[HasSoulPatch] AS [Item7], [g].[LeaderNickname] AS [Rest.Item1], [g].[LeaderSquadId] AS [Rest.Item2], [g].[Rank] AS [Rest.Item3]
            FROM [Gears] AS [g]
            WHERE [g].[Discriminator] IN (N'Gear', N'Officer') AND ((CASE WHEN ([g].[HasSoulPatch] = 1) AND (@p0 = 1) THEN 1 ELSE 0 END) <> @p1)

            @p0='False'
            @p1='False'

            SELECT [g].[Nickname] AS [Item1], [g].[SquadId] AS [Item2], [g].[AssignedCityName] AS [Item3], [g].[CityOfBirthName] AS [Item4], [g].[Discriminator] AS [Item5], [g].[FullName] AS [Item6], [g].[HasSoulPatch] AS [Item7], [g].[LeaderNickname] AS [Rest.Item1], [g].[LeaderSquadId] AS [Rest.Item2], [g].[Rank] AS [Rest.Item3]
            FROM [Gears] AS [g]
            WHERE [g].[Discriminator] IN (N'Gear', N'Officer') AND ((CASE WHEN ([g].[HasSoulPatch] = 1) OR (@p0 = 1) THEN 1 ELSE 0 END) <> @p1)
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

    [TranslationExceedsEFCore]
    public override async Task Optional_navigation_type_compensation_works_with_skip(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Optional_navigation_type_compensation_works_with_skip(async));

        AssertSql("""
            SELECT (
                SELECT [g].[Nickname] AS [Item1], [g].[SquadId] AS [Item2], [g].[AssignedCityName] AS [Item3], [g].[CityOfBirthName] AS [Item4], [g].[Discriminator] AS [Item5], [g].[FullName] AS [Item6], [g].[HasSoulPatch] AS [Item7], [g].[LeaderNickname] AS [Rest.Item1], [g].[LeaderSquadId] AS [Rest.Item2], [g].[Rank] AS [Rest.Item3]
                FROM [Gears] AS [g]
                WHERE [g].[Discriminator] IN (N'Gear', N'Officer')
                ORDER BY [g].[Nickname] ASC
                OFFSET [g_0].[Item2] ROWS
                FOR JSON PATH
            )
            FROM [Tags] AS [t]
            LEFT JOIN (
                SELECT 0 AS [$empty], [g_1].[Nickname] AS [Item1], [g_1].[SquadId] AS [Item2], [g_1].[AssignedCityName] AS [Item3], [g_1].[CityOfBirthName] AS [Item4], [g_1].[Discriminator] AS [Item5], [g_1].[FullName] AS [Item6], [g_1].[HasSoulPatch] AS [Item7], [g_1].[LeaderNickname] AS [Rest.Item1], [g_1].[LeaderSquadId] AS [Rest.Item2], [g_1].[Rank] AS [Rest.Item3]
                FROM [Gears] AS [g_1]
                WHERE [g_1].[Discriminator] IN (N'Gear', N'Officer')
            ) AS [g_0] ON ([t].[GearNickName] = [g_0].[Item1]) AND ([t].[GearSquadId] = [g_0].[Item2])
            WHERE ([t].[Note] IS NULL OR ([t].[Note] <> N'K.I.A.'))
            ORDER BY [t].[Note] ASC
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Optional_navigation_type_compensation_works_with_take(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Optional_navigation_type_compensation_works_with_take(async));

        AssertSql("""
            SELECT (
                SELECT TOP ([g].[Item2]) [g_0].[Nickname] AS [Item1], [g_0].[SquadId] AS [Item2], [g_0].[AssignedCityName] AS [Item3], [g_0].[CityOfBirthName] AS [Item4], [g_0].[Discriminator] AS [Item5], [g_0].[FullName] AS [Item6], [g_0].[HasSoulPatch] AS [Item7], [g_0].[LeaderNickname] AS [Rest.Item1], [g_0].[LeaderSquadId] AS [Rest.Item2], [g_0].[Rank] AS [Rest.Item3]
                FROM [Gears] AS [g_0]
                WHERE [g_0].[Discriminator] IN (N'Gear', N'Officer')
                ORDER BY [g_0].[Nickname] ASC
                FOR JSON PATH
            )
            FROM [Tags] AS [t]
            LEFT JOIN (
                SELECT 0 AS [$empty], [g_1].[Nickname] AS [Item1], [g_1].[SquadId] AS [Item2], [g_1].[AssignedCityName] AS [Item3], [g_1].[CityOfBirthName] AS [Item4], [g_1].[Discriminator] AS [Item5], [g_1].[FullName] AS [Item6], [g_1].[HasSoulPatch] AS [Item7], [g_1].[LeaderNickname] AS [Rest.Item1], [g_1].[LeaderSquadId] AS [Rest.Item2], [g_1].[Rank] AS [Rest.Item3]
                FROM [Gears] AS [g_1]
                WHERE [g_1].[Discriminator] IN (N'Gear', N'Officer')
            ) AS [g] ON ([t].[GearNickName] = [g].[Item1]) AND ([t].[GearSquadId] = [g].[Item2])
            WHERE ([t].[Note] IS NULL OR ([t].[Note] <> N'K.I.A.'))
            ORDER BY [t].[Note] ASC
            """);
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
                SELECT [g].[Nickname] AS [Item1], [g].[SquadId] AS [Item2], [g].[AssignedCityName] AS [Item3], [g].[CityOfBirthName] AS [Item4], [g].[Discriminator] AS [Item5], [g].[FullName] AS [Item6], [g].[HasSoulPatch] AS [Item7], [g].[LeaderNickname] AS [Rest.Item1], [g].[LeaderSquadId] AS [Rest.Item2], [g].[Rank] AS [Rest.Item3]
                FROM [Gears] AS [g]
                WHERE [g].[Discriminator] IN (N'Gear', N'Officer') AND ([s].[Id] = [g].[SquadId])
                FOR JSON PATH
            ) AS [$inner]
            FROM [Squads] AS [s]
            WHERE [s].[Name] = N'Kilo'
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Projecting_correlated_collection_followed_by_Distinct(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Projecting_correlated_collection_followed_by_Distinct(async));

        AssertSql("""
            SELECT DISTINCT (
                SELECT [w].[Id] AS [Id], [w].[AmmunitionType] AS [AmmunitionType], [w].[IsAutomatic] AS [IsAutomatic], [w].[Name] AS [Name], [w].[OwnerFullName] AS [OwnerFullName], [w].[SynergyWithId] AS [SynergyWithId]
                FROM [Weapons] AS [w]
                WHERE [g].[FullName] = [w].[OwnerFullName]
                FOR JSON PATH
            )
            FROM [Gears] AS [g]
            WHERE [g].[Discriminator] IN (N'Gear', N'Officer')
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Projecting_entity_as_well_as_complex_correlated_collection_followed_by_Distinct(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Projecting_entity_as_well_as_complex_correlated_collection_followed_by_Distinct(async));

        AssertSql("""
            SELECT DISTINCT [g].[Nickname] AS [g.Item1], [g].[SquadId] AS [g.Item2], [g].[AssignedCityName] AS [g.Item3], [g].[CityOfBirthName] AS [g.Item4], [g].[Discriminator] AS [g.Item5], [g].[FullName] AS [g.Item6], [g].[HasSoulPatch] AS [g.Item7], [g].[LeaderNickname] AS [g.Rest.Item1], [g].[LeaderSquadId] AS [g.Rest.Item2], [g].[Rank] AS [g.Rest.Item3], (
                SELECT [w].[Id] AS [Id], [w].[AmmunitionType] AS [AmmunitionType], [w].[IsAutomatic] AS [IsAutomatic], [w].[Name] AS [Name], [w].[OwnerFullName] AS [OwnerFullName], [w].[SynergyWithId] AS [SynergyWithId]
                FROM [Weapons] AS [w]
                WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[Id] = [g].[SquadId])
                FOR JSON PATH
            ) AS [Weapons]
            FROM [Gears] AS [g]
            WHERE [g].[Discriminator] IN (N'Gear', N'Officer')
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Projecting_entity_as_well_as_correlated_collection_followed_by_Distinct(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Projecting_entity_as_well_as_correlated_collection_followed_by_Distinct(async));

        AssertSql("""
            SELECT DISTINCT [g].[Nickname] AS [g.Item1], [g].[SquadId] AS [g.Item2], [g].[AssignedCityName] AS [g.Item3], [g].[CityOfBirthName] AS [g.Item4], [g].[Discriminator] AS [g.Item5], [g].[FullName] AS [g.Item6], [g].[HasSoulPatch] AS [g.Item7], [g].[LeaderNickname] AS [g.Rest.Item1], [g].[LeaderSquadId] AS [g.Rest.Item2], [g].[Rank] AS [g.Rest.Item3], (
                SELECT [w].[Id] AS [Id], [w].[AmmunitionType] AS [AmmunitionType], [w].[IsAutomatic] AS [IsAutomatic], [w].[Name] AS [Name], [w].[OwnerFullName] AS [OwnerFullName], [w].[SynergyWithId] AS [SynergyWithId]
                FROM [Weapons] AS [w]
                WHERE [g].[FullName] = [w].[OwnerFullName]
                FOR JSON PATH
            ) AS [Weapons]
            FROM [Gears] AS [g]
            WHERE [g].[Discriminator] IN (N'Gear', N'Officer')
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Projecting_entity_as_well_as_correlated_collection_of_scalars_followed_by_Distinct(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Projecting_entity_as_well_as_correlated_collection_of_scalars_followed_by_Distinct(async));

        AssertSql("""
            SELECT DISTINCT [g].[Nickname] AS [g.Item1], [g].[SquadId] AS [g.Item2], [g].[AssignedCityName] AS [g.Item3], [g].[CityOfBirthName] AS [g.Item4], [g].[Discriminator] AS [g.Item5], [g].[FullName] AS [g.Item6], [g].[HasSoulPatch] AS [g.Item7], [g].[LeaderNickname] AS [g.Rest.Item1], [g].[LeaderSquadId] AS [g.Rest.Item2], [g].[Rank] AS [g.Rest.Item3], (
                SELECT [w].[Id]
                FROM [Weapons] AS [w]
                WHERE [g].[FullName] = [w].[OwnerFullName]
                FOR JSON PATH
            ) AS [Ids]
            FROM [Gears] AS [g]
            WHERE [g].[Discriminator] IN (N'Gear', N'Officer')
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Projecting_some_properties_as_well_as_correlated_collection_followed_by_Distinct(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Projecting_some_properties_as_well_as_correlated_collection_followed_by_Distinct(async));

        AssertSql("""
            SELECT DISTINCT [g].[FullName] AS [FullName], [g].[HasSoulPatch] AS [HasSoulPatch], (
                SELECT [w].[Id] AS [Id], [w].[AmmunitionType] AS [AmmunitionType], [w].[IsAutomatic] AS [IsAutomatic], [w].[Name] AS [Name], [w].[OwnerFullName] AS [OwnerFullName], [w].[SynergyWithId] AS [SynergyWithId]
                FROM [Weapons] AS [w]
                WHERE [g].[FullName] = [w].[OwnerFullName]
                FOR JSON PATH
            ) AS [Weapons]
            FROM [Gears] AS [g]
            WHERE [g].[Discriminator] IN (N'Gear', N'Officer')
            """);
    }

    public override async Task Query_reusing_parameter_doesnt_declare_duplicate_parameter_complex(bool async)
    {
        await base.Query_reusing_parameter_doesnt_declare_duplicate_parameter_complex(async);

        AssertSql("""
            @p0='1'

            SELECT [g].[Item1] AS [Item1], [g].[Item2] AS [Item2], [g].[Item3] AS [Item3], [g].[Item4] AS [Item4], [g].[Item5] AS [Item5], [g].[Item6] AS [Item6], [g].[Item7] AS [Item7], [g].[Rest.Item1] AS [Rest.Item1], [g].[Rest.Item2] AS [Rest.Item2], [g].[Rest.Item3] AS [Rest.Item3]
            FROM (
                SELECT DISTINCT [g_0].[Nickname] AS [Item1], [g_0].[SquadId] AS [Item2], [g_0].[AssignedCityName] AS [Item3], [g_0].[CityOfBirthName] AS [Item4], [g_0].[Discriminator] AS [Item5], [g_0].[FullName] AS [Item6], [g_0].[HasSoulPatch] AS [Item7], [g_0].[LeaderNickname] AS [Rest.Item1], [g_0].[LeaderSquadId] AS [Rest.Item2], [g_0].[Rank] AS [Rest.Item3]
                FROM [Gears] AS [g_0]
                INNER JOIN [Squads] AS [s] ON [g_0].[SquadId] = [s].[Id]
                WHERE [g_0].[Discriminator] IN (N'Gear', N'Officer') AND ([s].[Id] = @p0)
            ) AS [g]
            INNER JOIN [Squads] AS [s_0] ON [g].[Item2] = [s_0].[Id]
            WHERE [s_0].[Id] = @p0
            ORDER BY [g].[Item6] ASC
            """);
    }

    public override Task Select_subquery_boolean_empty_with_pushdown(bool async)
    {
        return base.Select_subquery_boolean_empty_with_pushdown(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Select_Where_Navigation_Client(bool async)
    {
        return base.Select_Where_Navigation_Client(async);
    }

    [TranslationExceedsEFCore]
    public override async Task Select_correlated_filtered_collection_returning_queryable_throws(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Select_correlated_filtered_collection_returning_queryable_throws(async));

        AssertSql("""
            SELECT (
                SELECT [g].[Nickname] AS [Item1], [g].[SquadId] AS [Item2], [g].[AssignedCityName] AS [Item3], [g].[CityOfBirthName] AS [Item4], [g].[Discriminator] AS [Item5], [g].[FullName] AS [Item6], [g].[HasSoulPatch] AS [Item7], [g].[LeaderNickname] AS [Rest.Item1], [g].[LeaderSquadId] AS [Rest.Item2], [g].[Rank] AS [Rest.Item3]
                FROM [Gears] AS [g]
                WHERE [g].[Discriminator] IN (N'Gear', N'Officer') AND ([g].[Nickname] = [t].[GearNickName])
                FOR JSON PATH
            )
            FROM [Tags] AS [t]
            ORDER BY [t].[Note] ASC
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Streaming_correlated_collection_issue_11403_returning_ordered_enumerable_throws(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Streaming_correlated_collection_issue_11403_returning_ordered_enumerable_throws(async));

        AssertSql("""
            SELECT TOP (1) (
                SELECT [w].[Id] AS [Id], [w].[AmmunitionType] AS [AmmunitionType], [w].[IsAutomatic] AS [IsAutomatic], [w].[Name] AS [Name], [w].[OwnerFullName] AS [OwnerFullName], [w].[SynergyWithId] AS [SynergyWithId]
                FROM [Weapons] AS [w]
                WHERE ([g].[FullName] = [w].[OwnerFullName]) AND ([w].[IsAutomatic] = 0)
                ORDER BY [w].[Id] ASC
                FOR JSON PATH
            )
            FROM [Gears] AS [g]
            WHERE [g].[Discriminator] IN (N'Gear', N'Officer')
            ORDER BY [g].[Nickname] ASC
            """);
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

    public override async Task Where_contains_on_navigation_with_composite_keys(bool async)
    {
        await base.Where_contains_on_navigation_with_composite_keys(async);

        AssertSql("""
            SELECT [g].[Nickname] AS [Item1], [g].[SquadId] AS [Item2], [g].[AssignedCityName] AS [Item3], [g].[CityOfBirthName] AS [Item4], [g].[Discriminator] AS [Item5], [g].[FullName] AS [Item6], [g].[HasSoulPatch] AS [Item7], [g].[LeaderNickname] AS [Rest.Item1], [g].[LeaderSquadId] AS [Rest.Item2], [g].[Rank] AS [Rest.Item3]
            FROM [Gears] AS [g]
            WHERE [g].[Discriminator] IN (N'Gear', N'Officer') AND EXISTS (
                SELECT 1
                FROM [Cities] AS [c]
                WHERE EXISTS (
                    SELECT 1
                    FROM [Gears] AS [g_0]
                    WHERE ([g_0].[Discriminator] IN (N'Gear', N'Officer') AND ([c].[Name] = [g_0].[CityOfBirthName])) AND (([g_0].[Nickname] = [g].[Nickname]) AND ([g_0].[SquadId] = [g].[SquadId]))
                )
            )
            """);
    }

    [DisagreeWithEFCore]
    [Theory(Skip = Punt)]
    public override Task Where_subquery_with_ElementAt_using_column_as_index(bool async)
    {
        return base.Where_subquery_with_ElementAt_using_column_as_index(async);
    }
}
