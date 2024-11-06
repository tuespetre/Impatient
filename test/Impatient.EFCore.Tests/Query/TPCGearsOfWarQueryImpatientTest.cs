using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
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

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Client_member_and_unsupported_string_Equals_in_the_same_query(bool async)
    {
        return base.Client_member_and_unsupported_string_Equals_in_the_same_query(async);
    }

    [ConditionalTheory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Group_by_with_aggregate_max_on_entity_type(bool async)
    {
        return base.Group_by_with_aggregate_max_on_entity_type(async);
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
}
