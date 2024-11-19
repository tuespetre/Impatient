using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Impatient.EFCore.Tests.Query;

public class TPCInheritanceQueryImpatientTest : TPCInheritanceQueryTestBase<TPCInheritanceQueryImpatientTest.Fixture>
{
    public TPCInheritanceQueryImpatientTest(Fixture fixture, ITestOutputHelper testOutputHelper) : base(fixture, testOutputHelper)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => base.Fixture.TestSqlLoggerFactory.AssertSql(expected);

    public new class Fixture : TPCInheritanceQueryFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    public override async Task Is_operator_on_result_of_FirstOrDefault(bool async)
    {
        await base.Is_operator_on_result_of_FirstOrDefault(async);

        AssertSql("""
            SELECT [set].[Item1] AS [Item1], [set].[Item2] AS [Item2], [set].[Item3] AS [Item3], [set].[Item4] AS [Item4], [set].[Item5] AS [Item5], [set].[Item6] AS [Item6], [set].[Item7] AS [Item7], [set].[Rest.Item1] AS [Rest.Item1], [set].[Rest.Item2] AS [Rest.Item2]
            FROM (
                SELECT [e].[Id] AS [Item1], [e].[CountryId] AS [Item2], [e].[Name] AS [Item3], [e].[Species] AS [Item4], [e].[EagleId] AS [Item5], [e].[IsFlightless] AS [Item6], [e].[Group] AS [Item7], CAST(NULL AS tinyint) AS [Rest.Item1], N'Eagle' AS [Rest.Item2]
                FROM [Eagle] AS [e]
                UNION ALL
                SELECT [k].[Id] AS [Item1], [k].[CountryId] AS [Item2], [k].[Name] AS [Item3], [k].[Species] AS [Item4], [k].[EagleId] AS [Item5], [k].[IsFlightless] AS [Item6], CAST(NULL AS int) AS [Item7], [k].[FoundOn] AS [Rest.Item1], N'Kiwi' AS [Rest.Item2]
                FROM [Kiwi] AS [k]
            ) AS [set]
            WHERE (
                SELECT TOP (1) CAST((CASE WHEN [set_0].[Rest.Item2] = N'Kiwi' THEN 1 ELSE 0 END) AS bit)
                FROM (
                    SELECT [e].[Id] AS [Item1], [e].[CountryId] AS [Item2], [e].[Name] AS [Item3], [e].[Species] AS [Item4], [e].[EagleId] AS [Item5], [e].[IsFlightless] AS [Item6], [e].[Group] AS [Item7], CAST(NULL AS tinyint) AS [Rest.Item1], N'Eagle' AS [Rest.Item2]
                    FROM [Eagle] AS [e]
                    UNION ALL
                    SELECT [k].[Id] AS [Item1], [k].[CountryId] AS [Item2], [k].[Name] AS [Item3], [k].[Species] AS [Item4], [k].[EagleId] AS [Item5], [k].[IsFlightless] AS [Item6], CAST(NULL AS int) AS [Item7], [k].[FoundOn] AS [Rest.Item1], N'Kiwi' AS [Rest.Item2]
                    FROM [Kiwi] AS [k]
                ) AS [set_0]
                WHERE [set_0].[Item3] = N'Great spotted kiwi'
            ) = 1
            ORDER BY [set].[Item4] ASC
            """);
    }

    public override Task Using_OfType_on_multiple_type_with_no_result(bool async)
    {
        // TODO: allow this type of query, but just use an 'empty query expression' to avoid hitting the db.
        return base.Using_OfType_on_multiple_type_with_no_result(async);
    }
}
