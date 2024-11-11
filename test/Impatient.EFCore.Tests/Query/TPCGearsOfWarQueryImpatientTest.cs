using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using static Impatient.EFCore.Tests.Query.TPCGearsOfWarQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class TPCGearsOfWarQueryImpatientTest : TPCGearsOfWarQueryRelationalTestBase<Fixture>
{
    public TPCGearsOfWarQueryImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => base.Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(Fixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : TPCGearsOfWarQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    public override Task Accessing_property_of_optional_navigation_in_child_projection_works(bool async)
    {
        // failing because TranslatabilityAnalyzingExpressionVisitor.VisitConditional not currently allowing non-scalar types

        return base.Accessing_property_of_optional_navigation_in_child_projection_works(async);
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

    [ConditionalTheory(Skip = ClientEval)]
    public override Task Group_by_with_aggregate_max_on_entity_type(bool async)
    {
        return base.Group_by_with_aggregate_max_on_entity_type(async);
    }

    public override async Task Nav_rewrite_with_convert1(bool async)
    {
        await base.Nav_rewrite_with_convert1(async);

        AssertSql("""
            SELECT [l].[$empty] AS [$empty], [l].[Name] AS [Name], [l].[ThreatLevel] AS [ThreatLevel], [l].[ThreatLevelByte] AS [ThreatLevelByte], [l].[ThreatLevelNullableByte] AS [ThreatLevelNullableByte], [l].[DefeatedByNickname] AS [DefeatedByNickname], [l].[DefeatedBySquadId] AS [DefeatedBySquadId], [l].[HighCommandId] AS [HighCommandId]
            FROM [LocustHordes] AS [f]
            LEFT JOIN (
                SELECT 0 AS [$empty], [c].[Name] AS [Name], [c].[Location] AS [Location], [c].[Nation] AS [Nation]
                FROM [Cities] AS [c]
            ) AS [c_0] ON [f].[CapitalName] = [c_0].[Name]
            LEFT JOIN (
                SELECT 0 AS [$empty], [l_0].[LocustHordeId] AS [LocustHordeId], [l_0].[Name] AS [Name], [l_0].[ThreatLevel] AS [ThreatLevel], [l_0].[ThreatLevelByte] AS [ThreatLevelByte], [l_0].[ThreatLevelNullableByte] AS [ThreatLevelNullableByte], [l_0].[DefeatedByNickname] AS [DefeatedByNickname], [l_0].[DefeatedBySquadId] AS [DefeatedBySquadId], [l_0].[HighCommandId] AS [HighCommandId]
                FROM [LocustCommanders] AS [l_0]
            ) AS [l] ON [f].[CommanderName] = [l].[Name]
            WHERE ([c_0].[Name] IS NULL OR ([c_0].[Name] <> N'Foo'))
            """);
    }

    [Theory(Skip = ClientEval)]
    public override Task Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(bool async)
    {
        return base.Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(async);
    }

    public override async Task Project_one_value_type_with_client_projection_from_empty_collection(bool async)
    {
        await base.Project_one_value_type_with_client_projection_from_empty_collection(async);

        AssertSql("""
            SELECT [s].[Id] AS [$outer.Id], [s].[Banner] AS [$outer.Banner], [s].[Banner5] AS [$outer.Banner5], [s].[InternalNumber] AS [$outer.InternalNumber], [s].[Name] AS [$outer.Name], (
                SELECT [set].[Item1] AS [Item1], [set].[Item2] AS [Item2], [set].[Item3] AS [Item3], [set].[Item4] AS [Item4], [set].[Item5] AS [Item5], [set].[Item6] AS [Item6], [set].[Item7] AS [Item7], [set].[Rest.Item1] AS [Rest.Item1], [set].[Rest.Item2] AS [Rest.Item2], [set].[Rest.Item3] AS [Rest.Item3]
                FROM (
                    SELECT [g].[Nickname] AS [Item1], [g].[SquadId] AS [Item2], [g].[AssignedCityName] AS [Item3], [g].[CityOfBirthName] AS [Item4], [g].[FullName] AS [Item5], [g].[HasSoulPatch] AS [Item6], [g].[LeaderNickname] AS [Item7], [g].[LeaderSquadId] AS [Rest.Item1], [g].[Rank] AS [Rest.Item2], N'Gear' AS [Rest.Item3]
                    FROM [Gears] AS [g]
                    UNION ALL
                    SELECT [o].[Nickname] AS [Item1], [o].[SquadId] AS [Item2], [o].[AssignedCityName] AS [Item3], [o].[CityOfBirthName] AS [Item4], [o].[FullName] AS [Item5], [o].[HasSoulPatch] AS [Item6], [o].[LeaderNickname] AS [Item7], [o].[LeaderSquadId] AS [Rest.Item1], [o].[Rank] AS [Rest.Item2], N'Officer' AS [Rest.Item3]
                    FROM [Officers] AS [o]
                ) AS [set]
                WHERE [s].[Id] = [set].[Item2]
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
