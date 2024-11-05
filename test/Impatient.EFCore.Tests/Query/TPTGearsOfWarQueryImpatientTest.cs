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
}
