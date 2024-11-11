using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindSetOperationsQueryImpatientTest : NorthwindSetOperationsQueryRelationalTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindSetOperationsQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    [TranslationExceedsEFCore]
    public override async Task Collection_projection_after_set_operation_fails_if_distinct(bool async)
    {
        //await base.Collection_projection_after_set_operation_fails_if_distinct(async);

        await AssertQuery(
            async,
            ss => ss.Set<Customer>().Where(c => c.City == "Seatte")
                .Concat(ss.Set<Customer>().Where(c => c.CustomerID.StartsWith("F")))
                .Select(c => new { c.CustomerID, c.Orders }),
            elementSorter: c => c.CustomerID,
            elementAsserter: (e, a) =>
            {
                AssertEqual(e.CustomerID, a.CustomerID);
                AssertCollection(e.Orders, a.Orders);
            });

        AssertSql("""
            SELECT [set].[CustomerID] AS [CustomerID], (
                SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
                FROM [Orders] AS [o]
                WHERE [set].[CustomerID] = [o].[CustomerID]
                FOR JSON PATH
            ) AS [Orders]
            FROM (
                SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
                FROM [Customers] AS [c]
                WHERE [c].[City] = N'Seatte'
                UNION ALL
                SELECT [c_0].[CustomerID] AS [CustomerID], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Country] AS [Country], [c_0].[Fax] AS [Fax], [c_0].[Phone] AS [Phone], [c_0].[PostalCode] AS [PostalCode], [c_0].[Region] AS [Region]
                FROM [Customers] AS [c_0]
                WHERE LEFT([c_0].[CustomerID], LEN(N'F')) = N'F'
            ) AS [set]
            """);
    }

    [TranslationExceedsEFCore]
    public override async Task Collection_projection_before_set_operation_fails(bool async)
    {
        //await base.Collection_projection_before_set_operation_fails(async);

        await AssertQuery(
            async,
            ss => ss.Set<Customer>()
                .Where(c => c.City == "Seatte")
                .Select(c => new { c.Orders })
                .Union(
                    ss.Set<Customer>()
                        .Where(c => c.CustomerID.StartsWith("F"))
                        .Select(c => new { c.Orders })),
            elementSorter: a => a.Orders.FirstOrDefault().Maybe(e => e.CustomerID),
            elementAsserter: (e, a) =>
            {
                AssertCollection(e.Orders, a.Orders);
            });

        AssertSql("""
            SELECT [set].[Orders] AS [Orders]
            FROM (
                SELECT (
                    SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
                    FROM [Orders] AS [o]
                    WHERE [c].[CustomerID] = [o].[CustomerID]
                    FOR JSON PATH
                ) AS [Orders]
                FROM [Customers] AS [c]
                WHERE [c].[City] = N'Seatte'
                UNION
                SELECT (
                    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
                    FROM [Orders] AS [o_0]
                    WHERE [c_0].[CustomerID] = [o_0].[CustomerID]
                    FOR JSON PATH
                ) AS [Orders]
                FROM [Customers] AS [c_0]
                WHERE LEFT([c_0].[CustomerID], LEN(N'F')) = N'F'
            ) AS [set]
            """);
    }

    const string MismatchedIncludeSkipReason = "Unions of different includes - Punt";

    [Theory(Skip = MismatchedIncludeSkipReason)]
    public override async Task Include_Union_different_includes_throws(bool async)
    {
        await base.Include_Union_different_includes_throws(async);
    }

    [Theory(Skip = MismatchedIncludeSkipReason)]
    public override async Task Include_Union_only_on_one_side_throws(bool async)
    {
        await base.Include_Union_only_on_one_side_throws(async);
    }

    public override async Task OrderBy_Take_Union(bool async)
    {
        await base.OrderBy_Take_Union(async);

        AssertSql("""
            SELECT [set].[CustomerID] AS [CustomerID], [set].[Address] AS [Address], [set].[City] AS [City], [set].[CompanyName] AS [CompanyName], [set].[ContactName] AS [ContactName], [set].[ContactTitle] AS [ContactTitle], [set].[Country] AS [Country], [set].[Fax] AS [Fax], [set].[Phone] AS [Phone], [set].[PostalCode] AS [PostalCode], [set].[Region] AS [Region]
            FROM (
                SELECT TOP (1) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
                FROM [Customers] AS [c]
                ORDER BY [c].[ContactName] ASC
                UNION
                SELECT TOP (1) [c_0].[CustomerID] AS [CustomerID], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Country] AS [Country], [c_0].[Fax] AS [Fax], [c_0].[Phone] AS [Phone], [c_0].[PostalCode] AS [PostalCode], [c_0].[Region] AS [Region]
                FROM [Customers] AS [c_0]
                ORDER BY [c_0].[ContactName] ASC
            ) AS [set]
            """);
    }
}
