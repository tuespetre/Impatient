using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Sdk;

namespace Impatient.EFCore.Tests.Query;

public class QueryNavigationsImpatientTest : NorthwindNavigationsQueryRelationalTestBase<NorthwindQueryImpatientFixture>
{
    public QueryNavigationsImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    [Theory(Skip = ClientEval)]
    public override Task Collection_select_nav_prop_all_client(bool async)
    {
        return base.Collection_select_nav_prop_all_client(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Select_Where_Navigation_Client(bool async)
    {
        return base.Select_Where_Navigation_Client(async);
    }

    [TranslationExceedsEFCore]
    public override async Task Where_subquery_on_navigation(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Where_subquery_on_navigation(async));

        AssertSql("""
            SELECT [p].[ProductID] AS [ProductID], [p].[Discontinued] AS [Discontinued], [p].[ProductName] AS [ProductName], [p].[SupplierID] AS [SupplierID], [p].[UnitPrice] AS [UnitPrice], [p].[UnitsInStock] AS [UnitsInStock]
            FROM [Products] AS [p]
            WHERE EXISTS (
                SELECT 1
                FROM [Order Details] AS [o]
                WHERE ([p].[ProductID] = [o].[ProductID]) AND (([o].[OrderID] = (
                    SELECT TOP (1) [o_0].[OrderID]
                    FROM [Order Details] AS [o_0]
                    WHERE CAST([o_0].[Quantity] AS int) = 1
                    ORDER BY [o_0].[OrderID] DESC, [o_0].[ProductID] ASC
                )) AND ([o].[ProductID] = (
                    SELECT TOP (1) [o_1].[ProductID]
                    FROM [Order Details] AS [o_1]
                    WHERE CAST([o_1].[Quantity] AS int) = 1
                    ORDER BY [o_1].[OrderID] DESC, [o_1].[ProductID] ASC
                )))
            )
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Where_subquery_on_navigation2(bool async)
    {
        await Assert.ThrowsAsync<ThrowsException>(() => base.Where_subquery_on_navigation2(async));

        AssertSql("""
            SELECT [p].[ProductID] AS [ProductID], [p].[Discontinued] AS [Discontinued], [p].[ProductName] AS [ProductName], [p].[SupplierID] AS [SupplierID], [p].[UnitPrice] AS [UnitPrice], [p].[UnitsInStock] AS [UnitsInStock]
            FROM [Products] AS [p]
            WHERE EXISTS (
                SELECT 1
                FROM [Order Details] AS [o]
                WHERE ([p].[ProductID] = [o].[ProductID]) AND (([o].[OrderID] = (
                    SELECT TOP (1) [o_0].[OrderID]
                    FROM [Order Details] AS [o_0]
                    ORDER BY [o_0].[OrderID] DESC, [o_0].[ProductID] ASC
                )) AND ([o].[ProductID] = (
                    SELECT TOP (1) [o_1].[ProductID]
                    FROM [Order Details] AS [o_1]
                    ORDER BY [o_1].[OrderID] DESC, [o_1].[ProductID] ASC
                )))
            )
            """);
    }
}
