using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
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

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Client_eval_followed_by_aggregate_operation(bool async)
    {
        return base.Client_eval_followed_by_aggregate_operation(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Client_member_and_unsupported_string_Equals_in_the_same_query(bool async)
    {
        return base.Client_member_and_unsupported_string_Equals_in_the_same_query(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Client_method_on_collection_navigation_in_additional_from_clause(bool async)
    {
        return base.Client_method_on_collection_navigation_in_additional_from_clause(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Client_method_on_collection_navigation_in_order_by(bool async)
    {
        return base.Client_method_on_collection_navigation_in_order_by(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Client_method_on_collection_navigation_in_predicate(bool async)
    {
        return base.Client_method_on_collection_navigation_in_predicate(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property(bool async)
    {
        return base.Client_method_on_collection_navigation_in_predicate_accessed_by_ef_property(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Client_side_equality_with_parameter_works_with_optional_navigations(bool async)
    {
        return base.Client_side_equality_with_parameter_works_with_optional_navigations(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Correlated_collection_order_by_constant_null_of_non_mapped_type(bool async)
    {
        return base.Correlated_collection_order_by_constant_null_of_non_mapped_type(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task GetValueOrDefault_on_DateTimeOffset(bool async)
    {
        return base.GetValueOrDefault_on_DateTimeOffset(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Group_by_with_aggregate_max_on_entity_type(bool async)
    {
        return base.Group_by_with_aggregate_max_on_entity_type(async);
    }

    [Fact(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Nav_rewrite_Distinct_with_convert()
    {
        return base.Nav_rewrite_Distinct_with_convert();
    }

    [Fact(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Nav_rewrite_Distinct_with_convert_anonymous()
    {
        return base.Nav_rewrite_Distinct_with_convert_anonymous();
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(bool async)
    {
        return base.Orderby_added_for_client_side_GroupJoin_composite_dependent_to_principal_LOJ_when_incomplete_key_is_used(async);
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

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Select_Where_Navigation_Client(bool async)
    {
        return base.Select_Where_Navigation_Client(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Trying_to_access_unmapped_property_inside_aggregate(bool async)
    {
        return base.Trying_to_access_unmapped_property_inside_aggregate(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Trying_to_access_unmapped_property_throws_informative_error(bool async)
    {
        return base.Trying_to_access_unmapped_property_throws_informative_error(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Where_coalesce_with_anonymous_types(bool async)
    {
        return base.Where_coalesce_with_anonymous_types(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Where_compare_anonymous_types(bool async)
    {
        return base.Where_compare_anonymous_types(async);
    }
}
