using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
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
