using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class OwnedQueryImpatientTest : OwnedQueryRelationalTestBase<OwnedQueryImpatientTest.Fixture>
{
    public OwnedQueryImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => base.Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(Fixture fixture) =>
        new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : RelationalOwnedQueryFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    [ConditionalTheory(Skip = FromSql)]
    public override Task Using_from_sql_on_owner_generates_join_with_table_for_owned_shared_dependents(bool async)
    {
        return base.Using_from_sql_on_owner_generates_join_with_table_for_owned_shared_dependents(async);
    }

    public override async Task Can_OrderBy_indexer_properties(bool async)
    {
        // it's failing because the query is NoTracking, and we aren't doing navigation fixup on the owned order, setting its Client to the OwnedPerson instance.
        await base.Can_OrderBy_indexer_properties(async);

        AssertSql("""
            SELECT (
                SELECT [o].[Id] AS [Id], (
                    SELECT [o_0].[Detail] AS [Detail]
                    FROM [OrderDetail] AS [o_0]
                    WHERE ([o].[ClientId] = [o_0].[OrderClientId]) AND ([o].[Id] = [o_0].[OrderId])
                    FOR JSON PATH
                ) AS [Details], [o].[OrderDate] AS [OrderDate]
                FROM [Order] AS [o]
                WHERE [o_1].[Id] = [o].[ClientId]
                FOR JSON PATH
            ) AS [Orders], [o_1].[Id] AS [Item1], [o_1].[Discriminator] AS [Item2], [o_1].[Name] AS [Item3], [o_1].[PersonAddress_AddressLine] AS [Item4], [o_1].[PersonAddress_PlaceType] AS [Item5], [o_1].[PersonAddress_ZipCode] AS [Item6], [o_1].[PersonAddress_Country_Name] AS [Item7], [o_1].[PersonAddress_Country_PlanetId] AS [Rest.Item1], [o_1].[BranchAddress_BranchName] AS [Rest.Item2], [o_1].[BranchAddress_PlaceType] AS [Rest.Item3], [o_1].[BranchAddress_Country_Name] AS [Rest.Item4], [o_1].[BranchAddress_Country_PlanetId] AS [Rest.Item5], [o_1].[LeafAAddress_LeafType] AS [Rest.Item6], [o_1].[LeafAAddress_PlaceType] AS [Rest.Item7], [o_1].[LeafAAddress_Country_Name] AS [Rest.Rest.Item1], [o_1].[LeafAAddress_Country_PlanetId] AS [Rest.Rest.Item2], [o_1].[LeafBAddress_LeafBType] AS [Rest.Rest.Item3], [o_1].[LeafBAddress_PlaceType] AS [Rest.Rest.Item4], [o_1].[LeafBAddress_Country_Name] AS [Rest.Rest.Item5], [o_1].[LeafBAddress_Country_PlanetId] AS [Rest.Rest.Item6]
            FROM [OwnedPerson] AS [o_1]
            WHERE [o_1].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA')
            ORDER BY [o_1].[Name] ASC, [o_1].[Id] ASC
            """);
    }

    public override async Task Indexer_property_is_pushdown_into_subquery(bool async)
    {
        await base.Indexer_property_is_pushdown_into_subquery(async);

        AssertSql("""
            SELECT [o].[Name]
            FROM [OwnedPerson] AS [o]
            WHERE [o].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ((
                SELECT TOP (1) [o_0].[Name]
                FROM [OwnedPerson] AS [o_0]
                WHERE [o_0].[Discriminator] IN (N'OwnedPerson', N'Branch', N'LeafB', N'LeafA') AND ([o_0].[Id] = [o].[Id])
            ) = N'Mona Cy')
            """);
    }
}
