using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Impatient.EFCore.Tests.Query;

public class TPHInheritanceQueryImpatientTest : TPHInheritanceQueryTestBase<TPHInheritanceQueryImpatientTest.Fixture>
{
    public TPHInheritanceQueryImpatientTest(Fixture fixture, ITestOutputHelper testOutputHelper) : base(fixture, testOutputHelper)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => base.Fixture.TestSqlLoggerFactory.AssertSql(expected);

    public new class Fixture : TPHInheritanceQueryFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    [ConditionalTheory(Skip = FromSql)]
    public override Task Can_query_all_animal_views(bool async)
    {
        return base.Can_query_all_animal_views(async);
    }

    [Fact(Skip = FromSql)]
    public override void Casting_to_base_type_joining_with_query_type_works()
    {
        base.Casting_to_base_type_joining_with_query_type_works();
    }

    [Fact(Skip = FromSql)]
    public override void FromSql_on_derived()
    {
        base.FromSql_on_derived();
    }

    [Fact(Skip = FromSql)]
    public override void FromSql_on_root()
    {
        base.FromSql_on_root();
    }

    public override async Task Is_operator_on_result_of_FirstOrDefault(bool async)
    {
        await base.Is_operator_on_result_of_FirstOrDefault(async);

        AssertSql("""
            SELECT [a].[Id] AS [Item1], [a].[CountryId] AS [Item2], [a].[Discriminator] AS [Item3], [a].[Name] AS [Item4], [a].[Species] AS [Item5], [a].[EagleId] AS [Item6], [a].[IsFlightless] AS [Item7], [a].[Group] AS [Rest.Item1], [a].[FoundOn] AS [Rest.Item2]
            FROM [Animals] AS [a]
            WHERE [a].[Discriminator] IN (N'Eagle', N'Kiwi') AND ((
                SELECT TOP (1) CAST((CASE WHEN [a1].[Discriminator] = N'Kiwi' THEN 1 ELSE 0 END) AS bit)
                FROM [Animals] AS [a1]
                WHERE [a1].[Discriminator] IN (N'Eagle', N'Kiwi') AND ([a1].[Name] = N'Great spotted kiwi')
            ) = 1)
            ORDER BY [a].[Species] ASC
            """);
    }
}
