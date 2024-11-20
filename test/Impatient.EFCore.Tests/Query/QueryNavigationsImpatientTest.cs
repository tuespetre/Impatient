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

        // TODO: figure out a way to avoid the QueryComposingExpressionVisitor doing.... that
        AssertSql("""
            SELECT [p].[ProductID] AS [ProductID], [p].[Discontinued] AS [Discontinued], [p].[ProductName] AS [ProductName], [p].[SupplierID] AS [SupplierID], [p].[UnitPrice] AS [UnitPrice], [p].[UnitsInStock] AS [UnitsInStock]
            FROM [Products] AS [p]
            WHERE EXISTS (
                SELECT 1
                FROM [Order Details] AS [o]
                WHERE ([p].[ProductID] = [o].[ProductID]) AND (([o].[OrderID] = (
                    SELECT TOP (1) [t].[OrderID]
                    FROM (
                        SELECT TOP (1) [t_0].[OrderID]
                        FROM (
                            SELECT TOP (1) [t_1].[OrderID] AS [OrderID], [t_1].[ProductID] AS [ProductID], [t_1].[Discount] AS [Discount], [t_1].[Quantity] AS [Quantity], [t_1].[UnitPrice] AS [UnitPrice]
                            FROM (
                                SELECT TOP (1) [t_2].[OrderID] AS [OrderID], [t_2].[ProductID] AS [ProductID], [t_2].[Discount] AS [Discount], [t_2].[Quantity] AS [Quantity], [t_2].[UnitPrice] AS [UnitPrice]
                                FROM (
                                    SELECT TOP (1) [t_3].[OrderID] AS [OrderID], [t_3].[ProductID] AS [ProductID], [t_3].[Discount] AS [Discount], [t_3].[Quantity] AS [Quantity], [t_3].[UnitPrice] AS [UnitPrice]
                                    FROM (
                                        SELECT TOP (1) [t_4].[OrderID] AS [OrderID], [t_4].[ProductID] AS [ProductID], [t_4].[Discount] AS [Discount], [t_4].[Quantity] AS [Quantity], [t_4].[UnitPrice] AS [UnitPrice]
                                        FROM (
                                            SELECT TOP (1) [t_5].[OrderID] AS [OrderID], [t_5].[ProductID] AS [ProductID], [t_5].[Discount] AS [Discount], [t_5].[Quantity] AS [Quantity], [t_5].[UnitPrice] AS [UnitPrice]
                                            FROM (
                                                SELECT TOP (1) [t_6].[OrderID] AS [OrderID], [t_6].[ProductID] AS [ProductID], [t_6].[Discount] AS [Discount], [t_6].[Quantity] AS [Quantity], [t_6].[UnitPrice] AS [UnitPrice]
                                                FROM (
                                                    SELECT TOP (1) [t_7].[OrderID] AS [OrderID], [t_7].[ProductID] AS [ProductID], [t_7].[Discount] AS [Discount], [t_7].[Quantity] AS [Quantity], [t_7].[UnitPrice] AS [UnitPrice]
                                                    FROM (
                                                        SELECT TOP (1) [t_8].[OrderID] AS [OrderID], [t_8].[ProductID] AS [ProductID], [t_8].[Discount] AS [Discount], [t_8].[Quantity] AS [Quantity], [t_8].[UnitPrice] AS [UnitPrice]
                                                        FROM (
                                                            SELECT TOP (1) [o_0].[OrderID] AS [OrderID], [o_0].[ProductID] AS [ProductID], [o_0].[Discount] AS [Discount], [o_0].[Quantity] AS [Quantity], [o_0].[UnitPrice] AS [UnitPrice]
                                                            FROM [Order Details] AS [o_0]
                                                            WHERE CAST([o_0].[Quantity] AS int) = 1
                                                            ORDER BY [o_0].[OrderID] DESC, [o_0].[ProductID] ASC
                                                        ) AS [t_8]
                                                    ) AS [t_7]
                                                ) AS [t_6]
                                            ) AS [t_5]
                                        ) AS [t_4]
                                    ) AS [t_3]
                                ) AS [t_2]
                            ) AS [t_1]
                        ) AS [t_0]
                    ) AS [t]
                )) AND ([o].[ProductID] = (
                    SELECT TOP (1) [t_9].[ProductID]
                    FROM (
                        SELECT TOP (1) [t_10].[ProductID]
                        FROM (
                            SELECT TOP (1) [t_11].[OrderID] AS [OrderID], [t_11].[ProductID] AS [ProductID], [t_11].[Discount] AS [Discount], [t_11].[Quantity] AS [Quantity], [t_11].[UnitPrice] AS [UnitPrice]
                            FROM (
                                SELECT TOP (1) [t_12].[OrderID] AS [OrderID], [t_12].[ProductID] AS [ProductID], [t_12].[Discount] AS [Discount], [t_12].[Quantity] AS [Quantity], [t_12].[UnitPrice] AS [UnitPrice]
                                FROM (
                                    SELECT TOP (1) [t_13].[OrderID] AS [OrderID], [t_13].[ProductID] AS [ProductID], [t_13].[Discount] AS [Discount], [t_13].[Quantity] AS [Quantity], [t_13].[UnitPrice] AS [UnitPrice]
                                    FROM (
                                        SELECT TOP (1) [t_14].[OrderID] AS [OrderID], [t_14].[ProductID] AS [ProductID], [t_14].[Discount] AS [Discount], [t_14].[Quantity] AS [Quantity], [t_14].[UnitPrice] AS [UnitPrice]
                                        FROM (
                                            SELECT TOP (1) [t_15].[OrderID] AS [OrderID], [t_15].[ProductID] AS [ProductID], [t_15].[Discount] AS [Discount], [t_15].[Quantity] AS [Quantity], [t_15].[UnitPrice] AS [UnitPrice]
                                            FROM (
                                                SELECT TOP (1) [t_16].[OrderID] AS [OrderID], [t_16].[ProductID] AS [ProductID], [t_16].[Discount] AS [Discount], [t_16].[Quantity] AS [Quantity], [t_16].[UnitPrice] AS [UnitPrice]
                                                FROM (
                                                    SELECT TOP (1) [t_17].[OrderID] AS [OrderID], [t_17].[ProductID] AS [ProductID], [t_17].[Discount] AS [Discount], [t_17].[Quantity] AS [Quantity], [t_17].[UnitPrice] AS [UnitPrice]
                                                    FROM (
                                                        SELECT TOP (1) [t_18].[OrderID] AS [OrderID], [t_18].[ProductID] AS [ProductID], [t_18].[Discount] AS [Discount], [t_18].[Quantity] AS [Quantity], [t_18].[UnitPrice] AS [UnitPrice]
                                                        FROM (
                                                            SELECT TOP (1) [o_0].[OrderID] AS [OrderID], [o_0].[ProductID] AS [ProductID], [o_0].[Discount] AS [Discount], [o_0].[Quantity] AS [Quantity], [o_0].[UnitPrice] AS [UnitPrice]
                                                            FROM [Order Details] AS [o_0]
                                                            WHERE CAST([o_0].[Quantity] AS int) = 1
                                                            ORDER BY [o_0].[OrderID] DESC, [o_0].[ProductID] ASC
                                                        ) AS [t_18]
                                                    ) AS [t_17]
                                                ) AS [t_16]
                                            ) AS [t_15]
                                        ) AS [t_14]
                                    ) AS [t_13]
                                ) AS [t_12]
                            ) AS [t_11]
                        ) AS [t_10]
                    ) AS [t_9]
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
                    SELECT TOP (1) [t].[OrderID]
                    FROM (
                        SELECT TOP (1) [t_0].[OrderID]
                        FROM (
                            SELECT TOP (1) [t_1].[OrderID] AS [OrderID], [t_1].[ProductID] AS [ProductID], [t_1].[Discount] AS [Discount], [t_1].[Quantity] AS [Quantity], [t_1].[UnitPrice] AS [UnitPrice]
                            FROM (
                                SELECT TOP (1) [t_2].[OrderID] AS [OrderID], [t_2].[ProductID] AS [ProductID], [t_2].[Discount] AS [Discount], [t_2].[Quantity] AS [Quantity], [t_2].[UnitPrice] AS [UnitPrice]
                                FROM (
                                    SELECT TOP (1) [t_3].[OrderID] AS [OrderID], [t_3].[ProductID] AS [ProductID], [t_3].[Discount] AS [Discount], [t_3].[Quantity] AS [Quantity], [t_3].[UnitPrice] AS [UnitPrice]
                                    FROM (
                                        SELECT TOP (1) [t_4].[OrderID] AS [OrderID], [t_4].[ProductID] AS [ProductID], [t_4].[Discount] AS [Discount], [t_4].[Quantity] AS [Quantity], [t_4].[UnitPrice] AS [UnitPrice]
                                        FROM (
                                            SELECT TOP (1) [t_5].[OrderID] AS [OrderID], [t_5].[ProductID] AS [ProductID], [t_5].[Discount] AS [Discount], [t_5].[Quantity] AS [Quantity], [t_5].[UnitPrice] AS [UnitPrice]
                                            FROM (
                                                SELECT TOP (1) [t_6].[OrderID] AS [OrderID], [t_6].[ProductID] AS [ProductID], [t_6].[Discount] AS [Discount], [t_6].[Quantity] AS [Quantity], [t_6].[UnitPrice] AS [UnitPrice]
                                                FROM (
                                                    SELECT TOP (1) [t_7].[OrderID] AS [OrderID], [t_7].[ProductID] AS [ProductID], [t_7].[Discount] AS [Discount], [t_7].[Quantity] AS [Quantity], [t_7].[UnitPrice] AS [UnitPrice]
                                                    FROM (
                                                        SELECT TOP (1) [t_8].[OrderID] AS [OrderID], [t_8].[ProductID] AS [ProductID], [t_8].[Discount] AS [Discount], [t_8].[Quantity] AS [Quantity], [t_8].[UnitPrice] AS [UnitPrice]
                                                        FROM (
                                                            SELECT TOP (1) [o_0].[OrderID] AS [OrderID], [o_0].[ProductID] AS [ProductID], [o_0].[Discount] AS [Discount], [o_0].[Quantity] AS [Quantity], [o_0].[UnitPrice] AS [UnitPrice]
                                                            FROM [Order Details] AS [o_0]
                                                            ORDER BY [o_0].[OrderID] DESC, [o_0].[ProductID] ASC
                                                        ) AS [t_8]
                                                    ) AS [t_7]
                                                ) AS [t_6]
                                            ) AS [t_5]
                                        ) AS [t_4]
                                    ) AS [t_3]
                                ) AS [t_2]
                            ) AS [t_1]
                        ) AS [t_0]
                    ) AS [t]
                )) AND ([o].[ProductID] = (
                    SELECT TOP (1) [t_9].[ProductID]
                    FROM (
                        SELECT TOP (1) [t_10].[ProductID]
                        FROM (
                            SELECT TOP (1) [t_11].[OrderID] AS [OrderID], [t_11].[ProductID] AS [ProductID], [t_11].[Discount] AS [Discount], [t_11].[Quantity] AS [Quantity], [t_11].[UnitPrice] AS [UnitPrice]
                            FROM (
                                SELECT TOP (1) [t_2].[OrderID] AS [OrderID], [t_2].[ProductID] AS [ProductID], [t_2].[Discount] AS [Discount], [t_2].[Quantity] AS [Quantity], [t_2].[UnitPrice] AS [UnitPrice]
                                FROM (
                                    SELECT TOP (1) [t_3].[OrderID] AS [OrderID], [t_3].[ProductID] AS [ProductID], [t_3].[Discount] AS [Discount], [t_3].[Quantity] AS [Quantity], [t_3].[UnitPrice] AS [UnitPrice]
                                    FROM (
                                        SELECT TOP (1) [t_4].[OrderID] AS [OrderID], [t_4].[ProductID] AS [ProductID], [t_4].[Discount] AS [Discount], [t_4].[Quantity] AS [Quantity], [t_4].[UnitPrice] AS [UnitPrice]
                                        FROM (
                                            SELECT TOP (1) [t_5].[OrderID] AS [OrderID], [t_5].[ProductID] AS [ProductID], [t_5].[Discount] AS [Discount], [t_5].[Quantity] AS [Quantity], [t_5].[UnitPrice] AS [UnitPrice]
                                            FROM (
                                                SELECT TOP (1) [t_6].[OrderID] AS [OrderID], [t_6].[ProductID] AS [ProductID], [t_6].[Discount] AS [Discount], [t_6].[Quantity] AS [Quantity], [t_6].[UnitPrice] AS [UnitPrice]
                                                FROM (
                                                    SELECT TOP (1) [t_7].[OrderID] AS [OrderID], [t_7].[ProductID] AS [ProductID], [t_7].[Discount] AS [Discount], [t_7].[Quantity] AS [Quantity], [t_7].[UnitPrice] AS [UnitPrice]
                                                    FROM (
                                                        SELECT TOP (1) [t_8].[OrderID] AS [OrderID], [t_8].[ProductID] AS [ProductID], [t_8].[Discount] AS [Discount], [t_8].[Quantity] AS [Quantity], [t_8].[UnitPrice] AS [UnitPrice]
                                                        FROM (
                                                            SELECT TOP (1) [o_0].[OrderID] AS [OrderID], [o_0].[ProductID] AS [ProductID], [o_0].[Discount] AS [Discount], [o_0].[Quantity] AS [Quantity], [o_0].[UnitPrice] AS [UnitPrice]
                                                            FROM [Order Details] AS [o_0]
                                                            ORDER BY [o_0].[OrderID] DESC, [o_0].[ProductID] ASC
                                                        ) AS [t_8]
                                                    ) AS [t_7]
                                                ) AS [t_6]
                                            ) AS [t_5]
                                        ) AS [t_4]
                                    ) AS [t_3]
                                ) AS [t_2]
                            ) AS [t_11]
                        ) AS [t_10]
                    ) AS [t_9]
                )))
            )
            """);
    }
}
