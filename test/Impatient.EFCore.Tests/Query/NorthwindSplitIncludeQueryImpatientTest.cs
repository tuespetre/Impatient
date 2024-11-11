using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindSplitIncludeQueryImpatientTest : NorthwindSplitIncludeQueryTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindSplitIncludeQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_with_multiple_ordering(bool async)
    {
        return base.Filtered_include_with_multiple_ordering(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Include_collection_with_client_filter(bool async)
    {
        return base.Include_collection_with_client_filter(async);
    }

    [Theory(Skip = TranslationBeyondEF)]
    public override Task Include_collection_with_last_no_orderby(bool async)
    {
        return base.Include_collection_with_last_no_orderby(async);
    }

    public override async Task Include_collection_with_cross_apply_with_filter(bool async)
    {
        await base.Include_collection_with_cross_apply_with_filter(async);

        AssertSql("""
            SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region], (
                SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
                FROM [Orders] AS [o]
                WHERE [c].[CustomerID] = [o].[CustomerID]
                FOR JSON PATH
            ) AS [Orders]
            FROM [Customers] AS [c]
            CROSS APPLY (
                SELECT TOP (5) [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
                FROM [Orders] AS [o_0]
                WHERE [o_0].[CustomerID] = [c].[CustomerID]
                ORDER BY [c].[CustomerID] ASC
            ) AS [o_1]
            WHERE LEFT([c].[CustomerID], LEN(N'F')) = N'F'
            """);
    }

    public override async Task Include_collection_with_outer_apply_with_filter(bool async)
    {
        await base.Include_collection_with_outer_apply_with_filter(async);

        AssertSql("""
            SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region], (
                SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
                FROM [Orders] AS [o]
                WHERE [c].[CustomerID] = [o].[CustomerID]
                FOR JSON PATH
            ) AS [Orders]
            FROM [Customers] AS [c]
            OUTER APPLY (
                SELECT TOP (5) 0 AS [$empty], [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
                FROM [Orders] AS [o_0]
                WHERE [o_0].[CustomerID] = [c].[CustomerID]
                ORDER BY [c].[CustomerID] ASC
            ) AS [o_1]
            WHERE LEFT([c].[CustomerID], LEN(N'F')) = N'F'
            """);
    }
}
