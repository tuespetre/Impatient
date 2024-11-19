using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query;

public class TPTInheritanceQueryImpatientTest : TPTInheritanceQueryTestBase<TPTInheritanceQueryImpatientTest.Fixture>
{
    public TPTInheritanceQueryImpatientTest(Fixture fixture, Xunit.Abstractions.ITestOutputHelper helper) : base(fixture, helper)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => base.Fixture.TestSqlLoggerFactory.AssertSql(expected);

    public new class Fixture : TPTInheritanceQueryFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    public override async Task Is_operator_on_result_of_FirstOrDefault(bool async)
    {
        await base.Is_operator_on_result_of_FirstOrDefault(async);

        AssertSql("""
            SELECT [a].[Id] AS [Item1], [b].[Id] AS [Item2], [e].[Id] AS [Item3], [k].[Id] AS [Item4], [a].[CountryId] AS [Item5], [a].[Name] AS [Item6], [a].[Species] AS [Item7], [b].[EagleId] AS [Rest.Item1], [b].[IsFlightless] AS [Rest.Item2], [e].[Group] AS [Rest.Item3], [k].[FoundOn] AS [Rest.Item4], CASE
                WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
                WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
            END AS [Rest.Item5]
            FROM [Animals] AS [a]
            LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
            LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
            LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
            WHERE (
                SELECT TOP (1) CAST((CASE WHEN CASE
                    WHEN [k].[Id] IS NOT NULL THEN N'Kiwi'
                    WHEN [e].[Id] IS NOT NULL THEN N'Eagle'
                END = N'Kiwi' THEN 1 ELSE 0 END) AS bit)
                FROM [Animals] AS [a]
                LEFT JOIN [Birds] AS [b] ON [a].[Id] = [b].[Id]
                LEFT JOIN [Eagle] AS [e] ON [a].[Id] = [e].[Id]
                LEFT JOIN [Kiwi] AS [k] ON [a].[Id] = [k].[Id]
                WHERE [a].[Name] = N'Great spotted kiwi'
            ) = 1
            ORDER BY [a].[Species] ASC
            """);
    }
}
