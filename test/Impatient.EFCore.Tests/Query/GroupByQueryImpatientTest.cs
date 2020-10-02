using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace Impatient.EFCore.Tests.Query
{
    public class GroupByQueryImpatientTest : NorthwindGroupByQueryRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public GroupByQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            ClearLog();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Anonymous_projection_Distinct_GroupBy_Aggregate(bool async)
        {
            await base.Anonymous_projection_Distinct_GroupBy_Aggregate(async);

            AssertSql(@"
SELECT [o].[EmployeeID] AS [Key], COUNT(*) AS [c]
FROM (
    SELECT DISTINCT [o_0].[OrderID] AS [OrderID], [o_0].[EmployeeID] AS [EmployeeID]
    FROM [Orders] AS [o_0]
) AS [o]
GROUP BY [o].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Distinct_GroupBy_Aggregate(bool async)
        {
            await base.Distinct_GroupBy_Aggregate(async);

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [c]
FROM (
    SELECT DISTINCT [g].[OrderID] AS [OrderID], [g].[CustomerID] AS [CustomerID], [g].[EmployeeID] AS [EmployeeID], [g].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [g]
) AS [o]
GROUP BY [o].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Distinct_GroupBy_OrderBy_key(bool async)
        {
            await base.Distinct_GroupBy_OrderBy_key(async);

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [c]
FROM (
    SELECT DISTINCT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_0]
) AS [o]
GROUP BY [o].[CustomerID]
ORDER BY [o].[CustomerID] ASC
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_aggregate_Contains(bool async)
        {
            await base.GroupBy_aggregate_Contains(async);

            Fixture.AssertSql(@"
SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] IN (
    SELECT [g].[Key]
    FROM (
        SELECT [g_0].[CustomerID] AS [Key]
        FROM [Orders] AS [g_0]
        GROUP BY [g_0].[CustomerID]
    ) AS [g]
    WHERE (
        SELECT COUNT(*)
        FROM [Orders] AS [g_1]
        WHERE (([g].[Key] IS NULL AND [g_1].[CustomerID] IS NULL) OR ([g].[Key] = [g_1].[CustomerID]))
    ) > 30
)");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Aggregate_Join(bool async)
        {
            await base.GroupBy_Aggregate_Join(async);

            AssertSql(@"
SELECT [c].[CustomerID] AS [c.CustomerID], [c].[Address] AS [c.Address], [c].[City] AS [c.City], [c].[CompanyName] AS [c.CompanyName], [c].[ContactName] AS [c.ContactName], [c].[ContactTitle] AS [c.ContactTitle], [c].[Country] AS [c.Country], [c].[Fax] AS [c.Fax], [c].[Phone] AS [c.Phone], [c].[PostalCode] AS [c.PostalCode], [c].[Region] AS [c.Region], [o].[OrderID] AS [o.OrderID], [o].[CustomerID] AS [o.CustomerID], [o].[EmployeeID] AS [o.EmployeeID], [o].[OrderDate] AS [o.OrderDate]
FROM (
    SELECT [g].[CustomerID] AS [Key]
    FROM [Orders] AS [g]
    GROUP BY [g].[CustomerID]
) AS [g_0]
INNER JOIN [Customers] AS [c] ON [g_0].[Key] = [c].[CustomerID]
INNER JOIN [Orders] AS [o] ON (
    SELECT MAX([g_1].[OrderID])
    FROM [Orders] AS [g_1]
    WHERE (([g_0].[Key] IS NULL AND [g_1].[CustomerID] IS NULL) OR ([g_0].[Key] = [g_1].[CustomerID]))
) = [o].[OrderID]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [g_1]
    WHERE (([g_0].[Key] IS NULL AND [g_1].[CustomerID] IS NULL) OR ([g_0].[Key] = [g_1].[CustomerID]))
) > 5
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_aggregate_Pushdown(bool async)
        {
            await base.GroupBy_aggregate_Pushdown(async);

            AssertSql(@"
SELECT [t].[Key]
FROM (
    SELECT TOP (20) [g].[Key]
    FROM (
        SELECT [g_0].[CustomerID] AS [Key]
        FROM [Orders] AS [g_0]
        GROUP BY [g_0].[CustomerID]
    ) AS [g]
    WHERE (
        SELECT COUNT(*)
        FROM [Orders] AS [g_1]
        WHERE (([g].[Key] IS NULL AND [g_1].[CustomerID] IS NULL) OR ([g].[Key] = [g_1].[CustomerID]))
    ) > 10
    ORDER BY [g].[Key] ASC
) AS [t]
ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) ASC
OFFSET 4 ROWS
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_anonymous_Select_Average(bool async)
        {
            await base.GroupBy_anonymous_Select_Average(async);

            AssertSql(@"
SELECT AVG(CAST([g].[OrderID] AS float))
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_anonymous_Select_Count(bool async)
        {
            await base.GroupBy_anonymous_Select_Count(async);

            AssertSql(@"
SELECT COUNT(*)
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_anonymous_Select_LongCount(bool async)
        {
            await base.GroupBy_anonymous_Select_LongCount(async);

            AssertSql(@"
SELECT COUNT_BIG(*)
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_anonymous_Select_Max(bool async)
        {
            await base.GroupBy_anonymous_Select_Max(async);

            AssertSql(@"
SELECT MAX([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_anonymous_Select_Min(bool async)
        {
            await base.GroupBy_anonymous_Select_Min(async);

            AssertSql(@"
SELECT MIN([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_anonymous_Select_Sum(bool async)
        {
            await base.GroupBy_anonymous_Select_Sum(async);

            AssertSql(@"
SELECT SUM([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_anonymous_Select_Sum_Min_Max_Avg(bool async)
        {
            await base.GroupBy_anonymous_Select_Sum_Min_Max_Avg(async);

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_anonymous_with_alias_Select_Key_Sum(bool async)
        {
            await base.GroupBy_anonymous_with_alias_Select_Key_Sum(async);

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], SUM([g].[OrderID]) AS [Sum]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Average(bool async)
        {
            await base.GroupBy_Composite_Select_Average(async);

            AssertSql(@"
SELECT AVG(CAST([g].[OrderID] AS float))
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Count(bool async)
        {
            await base.GroupBy_Composite_Select_Count(async);

            AssertSql(@"
SELECT COUNT(*)
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Dto_Sum_Min_Key_flattened_Max_Avg(bool async)
        {
            await base.GroupBy_Composite_Select_Dto_Sum_Min_Key_flattened_Max_Avg(async);

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], [g].[CustomerID] AS [CustomerId], [g].[EmployeeID] AS [EmployeeId], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Key_Average(bool async)
        {
            await base.GroupBy_Composite_Select_Key_Average(async);

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID], AVG(CAST([g].[OrderID] AS float)) AS [Average]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Key_Count(bool async)
        {
            await base.GroupBy_Composite_Select_Key_Count(async);

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID], COUNT(*) AS [Count]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Key_LongCount(bool async)
        {
            await base.GroupBy_Composite_Select_Key_LongCount(async);

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID], COUNT_BIG(*) AS [LongCount]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Key_Max(bool async)
        {
            await base.GroupBy_Composite_Select_Key_Max(async);

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID], MAX([g].[OrderID]) AS [Max]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Key_Min(bool async)
        {
            await base.GroupBy_Composite_Select_Key_Min(async);

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID], MIN([g].[OrderID]) AS [Min]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Key_Sum(bool async)
        {
            await base.GroupBy_Composite_Select_Key_Sum(async);

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID], SUM([g].[OrderID]) AS [Sum]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Key_Sum_Min_Max_Avg(bool async)
        {
            await base.GroupBy_Composite_Select_Key_Sum_Min_Max_Avg(async);

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID], SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_LongCount(bool async)
        {
            await base.GroupBy_Composite_Select_LongCount(async);

            AssertSql(@"
SELECT COUNT_BIG(*)
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Max(bool async)
        {
            await base.GroupBy_Composite_Select_Max(async);

            AssertSql(@"
SELECT MAX([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Min(bool async)
        {
            await base.GroupBy_Composite_Select_Min(async);

            AssertSql(@"
SELECT MIN([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Sum(bool async)
        {
            await base.GroupBy_Composite_Select_Sum(async);

            AssertSql(@"
SELECT SUM([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg(bool async)
        {
            await base.GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg(async);

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], [g].[CustomerID] AS [CustomerID], [g].[EmployeeID] AS [EmployeeID], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Sum_Min_Key_Max_Avg(bool async)
        {
            await base.GroupBy_Composite_Select_Sum_Min_Key_Max_Avg(async);

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Sum_Min_Max_Avg(bool async)
        {
            await base.GroupBy_Composite_Select_Sum_Min_Max_Avg(async);

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg(bool async)
        {
            await base.GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg(async);

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], [g].[CustomerID] AS [CustomerID], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Distinct(bool async)
        {
            await base.GroupBy_Distinct(async);

            AssertSql(@"
SELECT [g].[Key]
FROM (
    SELECT DISTINCT [g_0].[CustomerID] AS [Key]
    FROM [Orders] AS [g_0]
    GROUP BY [g_0].[CustomerID]
) AS [g]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Dto_as_element_selector_Select_Sum(bool async)
        {
            await base.GroupBy_Dto_as_element_selector_Select_Sum(async);

            AssertSql(@"
SELECT SUM(CAST([g].[EmployeeID] AS bigint)) AS [Sum], [g].[CustomerID] AS [Key]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Dto_as_key_Select_Sum(bool async)
        {
            await base.GroupBy_Dto_as_key_Select_Sum(async);

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_empty_key_Aggregate(bool async)
        {
            await base.GroupBy_empty_key_Aggregate(async);

            Fixture.AssertSql(@"SELECT [g].[OrderID] AS [OrderID], [g].[CustomerID] AS [CustomerID], [g].[EmployeeID] AS [EmployeeID], [g].[OrderDate] AS [OrderDate]
FROM [Orders] AS [g]");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_filter_count(bool async)
        {
            await base.GroupBy_filter_count(async);

            AssertSql(@"
SELECT [o].[Key] AS [Key], (
    SELECT COUNT(*)
    FROM [Orders] AS [o_0]
    WHERE (([o].[Key] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[Key] = [o_0].[CustomerID]))
) AS [Count]
FROM (
    SELECT [o_1].[CustomerID] AS [Key]
    FROM [Orders] AS [o_1]
    GROUP BY [o_1].[CustomerID]
) AS [o]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o_0]
    WHERE (([o].[Key] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[Key] = [o_0].[CustomerID]))
) > 4
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_filter_count_OrderBy_count_Select_sum(bool async)
        {
            await base.GroupBy_filter_count_OrderBy_count_Select_sum(async);

            AssertSql(@"
SELECT [o].[Key] AS [Key], (
    SELECT COUNT(*)
    FROM [Orders] AS [o_0]
    WHERE (([o].[Key] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[Key] = [o_0].[CustomerID]))
) AS [Count], (
    SELECT SUM([o_0].[OrderID])
    FROM [Orders] AS [o_0]
    WHERE (([o].[Key] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[Key] = [o_0].[CustomerID]))
) AS [Sum]
FROM (
    SELECT [o_1].[CustomerID] AS [Key]
    FROM [Orders] AS [o_1]
    GROUP BY [o_1].[CustomerID]
) AS [o]
WHERE (
    SELECT COUNT(*)
    FROM [Orders] AS [o_0]
    WHERE (([o].[Key] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[Key] = [o_0].[CustomerID]))
) > 4
ORDER BY (
    SELECT COUNT(*)
    FROM [Orders] AS [o_0]
    WHERE (([o].[Key] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[Key] = [o_0].[CustomerID]))
) ASC, [o].[Key] ASC
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_filter_key(bool async)
        {
            await base.GroupBy_filter_key(async);

            AssertSql(@"
SELECT [o].[Key] AS [Key], (
    SELECT COUNT(*)
    FROM [Orders] AS [o_0]
    WHERE (([o].[Key] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[Key] = [o_0].[CustomerID]))
) AS [c]
FROM (
    SELECT [o_1].[CustomerID] AS [Key]
    FROM [Orders] AS [o_1]
    GROUP BY [o_1].[CustomerID]
) AS [o]
WHERE [o].[Key] = N'ALFKI'
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_multi_navigation_members_Aggregate(bool async)
        {
            await base.GroupBy_multi_navigation_members_Aggregate(async);

            AssertSql(@"
SELECT [o].[CustomerID] AS [CompositeKey.CustomerID], [p].[ProductName] AS [CompositeKey.ProductName], COUNT(*) AS [Count]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [o] ON [od].[OrderID] = [o].[OrderID]
INNER JOIN [Products] AS [p] ON [od].[ProductID] = [p].[ProductID]
GROUP BY [o].[CustomerID], [p].[ProductName]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_optional_navigation_member_Aggregate(bool async)
        {
            await base.GroupBy_optional_navigation_member_Aggregate(async);

            AssertSql(@"
SELECT [c].[Country] AS [Country], COUNT(*) AS [Count]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT 0 AS [$empty], [c_0].[CustomerID] AS [CustomerID], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Country] AS [Country], [c_0].[Fax] AS [Fax], [c_0].[Phone] AS [Phone], [c_0].[PostalCode] AS [PostalCode], [c_0].[Region] AS [Region]
    FROM [Customers] AS [c_0]
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[Country]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_OrderBy_count(bool async)
        {
            await base.GroupBy_OrderBy_count(async);

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
ORDER BY COUNT(*) ASC, [o].[CustomerID] ASC
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_OrderBy_count_Select_sum(bool async)
        {
            await base.GroupBy_OrderBy_count_Select_sum(async);

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], SUM([o].[OrderID]) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
ORDER BY COUNT(*) ASC, [o].[CustomerID] ASC
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_OrderBy_key(bool async)
        {
            await base.GroupBy_OrderBy_key(async);

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [c]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
ORDER BY [o].[CustomerID] ASC
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_anonymous_element_selector_Average(bool async)
        {
            await base.GroupBy_Property_anonymous_element_selector_Average(async);

            AssertSql(@"
SELECT AVG(CAST([g].[OrderID] AS float))
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_anonymous_element_selector_Count(bool async)
        {
            await base.GroupBy_Property_anonymous_element_selector_Count(async);

            AssertSql(@"
SELECT COUNT(*)
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_anonymous_element_selector_LongCount(bool async)
        {
            await base.GroupBy_Property_anonymous_element_selector_LongCount(async);

            AssertSql(@"
SELECT COUNT_BIG(*)
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_anonymous_element_selector_Max(bool async)
        {
            await base.GroupBy_Property_anonymous_element_selector_Max(async);

            AssertSql(@"
SELECT MAX([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_anonymous_element_selector_Min(bool async)
        {
            await base.GroupBy_Property_anonymous_element_selector_Min(async);

            AssertSql(@"
SELECT MIN([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_anonymous_element_selector_Sum(bool async)
        {
            await base.GroupBy_Property_anonymous_element_selector_Sum(async);

            AssertSql(@"
SELECT SUM([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg(bool async)
        {
            await base.GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg(async);

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[EmployeeID]) AS [Min], MAX([g].[EmployeeID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_scalar_element_selector_Average(bool async)
        {
            await base.GroupBy_Property_scalar_element_selector_Average(async);

            AssertSql(@"
SELECT AVG(CAST([g].[OrderID] AS float))
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_scalar_element_selector_Count(bool async)
        {
            await base.GroupBy_Property_scalar_element_selector_Count(async);

            AssertSql(@"
SELECT COUNT([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_scalar_element_selector_LongCount(bool async)
        {
            await base.GroupBy_Property_scalar_element_selector_LongCount(async);

            AssertSql(@"
SELECT COUNT_BIG([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_scalar_element_selector_Max(bool async)
        {
            await base.GroupBy_Property_scalar_element_selector_Max(async);

            AssertSql(@"
SELECT MAX([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_scalar_element_selector_Min(bool async)
        {
            await base.GroupBy_Property_scalar_element_selector_Min(async);

            AssertSql(@"
SELECT MIN([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_scalar_element_selector_Sum(bool async)
        {
            await base.GroupBy_Property_scalar_element_selector_Sum(async);

            AssertSql(@"
SELECT SUM([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg(bool async)
        {
            await base.GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg(async);

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_Select_Average(bool async)
        {
            await base.GroupBy_Property_Select_Average(async);

            AssertSql(@"
SELECT AVG(CAST([g].[OrderID] AS float))
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_Select_Count(bool async)
        {
            await base.GroupBy_Property_Select_Count(async);

            AssertSql(@"
SELECT COUNT(*)
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_Select_Key_Average(bool async)
        {
            await base.GroupBy_Property_Select_Key_Average(async);

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], AVG(CAST([g].[OrderID] AS float)) AS [Average]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_Select_Key_Count(bool async)
        {
            await base.GroupBy_Property_Select_Key_Count(async);

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_Select_Key_LongCount(bool async)
        {
            await base.GroupBy_Property_Select_Key_LongCount(async);

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], COUNT_BIG(*) AS [LongCount]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_Select_Key_Max(bool async)
        {
            await base.GroupBy_Property_Select_Key_Max(async);

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], MAX([g].[OrderID]) AS [Max]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_Select_Key_Min(bool async)
        {
            await base.GroupBy_Property_Select_Key_Min(async);

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], MIN([g].[OrderID]) AS [Min]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_Select_Key_Sum(bool async)
        {
            await base.GroupBy_Property_Select_Key_Sum(async);

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], SUM([g].[OrderID]) AS [Sum]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_Select_Key_Sum_Min_Max_Avg(bool async)
        {
            await base.GroupBy_Property_Select_Key_Sum_Min_Max_Avg(async);

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_Select_LongCount(bool async)
        {
            await base.GroupBy_Property_Select_LongCount(async);

            AssertSql(@"
SELECT COUNT_BIG(*)
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_Select_Max(bool async)
        {
            await base.GroupBy_Property_Select_Max(async);

            AssertSql(@"
SELECT MAX([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_Select_Min(bool async)
        {
            await base.GroupBy_Property_Select_Min(async);

            AssertSql(@"
SELECT MIN([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_Select_Sum(bool async)
        {
            await base.GroupBy_Property_Select_Sum(async);

            AssertSql(@"
SELECT SUM([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_Select_Sum_Min_Key_Max_Avg(bool async)
        {
            await base.GroupBy_Property_Select_Sum_Min_Key_Max_Avg(async);

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], [g].[CustomerID] AS [Key], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Property_Select_Sum_Min_Max_Avg(bool async)
        {
            await base.GroupBy_Property_Select_Sum_Min_Max_Avg(async);

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_required_navigation_member_Aggregate(bool async)
        {
            await base.GroupBy_required_navigation_member_Aggregate(async);

            AssertSql(@"
SELECT [o].[CustomerID] AS [CustomerId], COUNT(*) AS [Count]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [o] ON [od].[OrderID] = [o].[OrderID]
GROUP BY [o].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_SelectMany(bool async)
        {
            await base.GroupBy_SelectMany(async);

            AssertSql(@"
SELECT [g].[CustomerID] AS [CustomerID], [g].[Address] AS [Address], [g].[City] AS [City], [g].[CompanyName] AS [CompanyName], [g].[ContactName] AS [ContactName], [g].[ContactTitle] AS [ContactTitle], [g].[Country] AS [Country], [g].[Fax] AS [Fax], [g].[Phone] AS [Phone], [g].[PostalCode] AS [PostalCode], [g].[Region] AS [Region]
FROM (
    SELECT [g_0].[City] AS [Key]
    FROM [Customers] AS [g_0]
    GROUP BY [g_0].[City]
) AS [g_1]
INNER JOIN [Customers] AS [g] ON [g_1].[Key] = [g].[City]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Shadow(bool async)
        {
            await base.GroupBy_Shadow(async);

            AssertSql(@"
@p0='1'

SELECT [e].[Title] AS [Key], (
    SELECT [e_0].[EmployeeID] AS [EmployeeID], [e_0].[City] AS [City], [e_0].[Country] AS [Country], [e_0].[FirstName] AS [FirstName], [e_0].[ReportsTo] AS [ReportsTo], [e_0].[Title] AS [Title]
    FROM [Employees] AS [e_0]
    WHERE (([e_0].[Title] = N'Sales Representative') AND ([e_0].[EmployeeID] = @p0)) AND ((([e].[Title] IS NULL AND [e_0].[Title] IS NULL) OR ([e].[Title] = [e_0].[Title])))
    FOR JSON PATH
) AS [Elements]
FROM [Employees] AS [e]
WHERE ([e].[Title] = N'Sales Representative') AND ([e].[EmployeeID] = @p0)
GROUP BY [e].[Title]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Shadow3(bool async)
        {
            await base.GroupBy_Shadow3(async);

            AssertSql(@"
@p0='1'

SELECT [e].[EmployeeID] AS [Key], (
    SELECT [e_0].[EmployeeID] AS [EmployeeID], [e_0].[City] AS [City], [e_0].[Country] AS [Country], [e_0].[FirstName] AS [FirstName], [e_0].[ReportsTo] AS [ReportsTo], [e_0].[Title] AS [Title]
    FROM [Employees] AS [e_0]
    WHERE ([e_0].[EmployeeID] = @p0) AND ([e].[EmployeeID] = [e_0].[EmployeeID])
    FOR JSON PATH
) AS [Elements]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] = @p0
GROUP BY [e].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Sum_constant(bool async)
        {
            await base.GroupBy_Sum_constant(async);

            AssertSql(@"
SELECT SUM(CAST(1 AS int))
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_Sum_constant_cast(bool async)
        {
            await base.GroupBy_Sum_constant_cast(async);

            AssertSql(@"
SELECT SUM(CAST(1 AS bigint))
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_with_aggregate_through_navigation_property(bool async)
        {
            await base.GroupBy_with_aggregate_through_navigation_property(async);

            AssertSql(@"
SELECT (
    SELECT MAX([c].[Region])
    FROM [Orders] AS [g]
    LEFT JOIN (
        SELECT 0 AS [$empty], [c_0].[CustomerID] AS [CustomerID], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Country] AS [Country], [c_0].[Fax] AS [Fax], [c_0].[Phone] AS [Phone], [c_0].[PostalCode] AS [PostalCode], [c_0].[Region] AS [Region]
        FROM [Customers] AS [c_0]
    ) AS [c] ON [g].[CustomerID] = [c].[CustomerID]
    WHERE (([g_0].[EmployeeID] IS NULL AND [g].[EmployeeID] IS NULL) OR ([g_0].[EmployeeID] = [g].[EmployeeID]))
) AS [max]
FROM [Orders] AS [g_0]
GROUP BY [g_0].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupBy_with_result_selector(bool async)
        {
            await base.GroupBy_with_result_selector(async);

            AssertSql(@"
SELECT SUM([o].[OrderID]) AS [Sum], MIN([o].[OrderID]) AS [Min], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupJoin_complex_GroupBy_Aggregate(bool async)
        {
            await base.GroupJoin_complex_GroupBy_Aggregate(async);

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], AVG(CAST([o].[OrderID] AS float)) AS [Count]
FROM (
    SELECT [c].[CustomerID] AS [c.CustomerID], [c].[Address] AS [c.Address], [c].[City] AS [c.City], [c].[CompanyName] AS [c.CompanyName], [c].[ContactName] AS [c.ContactName], [c].[ContactTitle] AS [c.ContactTitle], [c].[Country] AS [c.Country], [c].[Fax] AS [c.Fax], [c].[Phone] AS [c.Phone], [c].[PostalCode] AS [c.PostalCode], [c].[Region] AS [c.Region], [c].[CustomerID] AS [grouping.Key]
    FROM [Customers] AS [c]
    WHERE ([c].[CustomerID] <> N'DRACD') AND ([c].[CustomerID] <> N'FOLKO')
    ORDER BY [c].[City] ASC
    OFFSET 10 ROWS FETCH NEXT 50 ROWS ONLY
) AS [t]
INNER JOIN (
    SELECT TOP (100) [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_0]
    WHERE [o_0].[OrderID] < 10400
    ORDER BY [o_0].[OrderDate] ASC
) AS [o] ON [t].[grouping.Key] = [o].[CustomerID]
WHERE [o].[OrderID] > 10300
GROUP BY [o].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupJoin_GroupBy_Aggregate(bool async)
        {
            await base.GroupJoin_GroupBy_Aggregate(async);

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], AVG(CAST([o].[OrderID] AS float)) AS [Average]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT 0 AS [$empty], [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_0]
) AS [o] ON [c].[CustomerID] = [o].[CustomerID]
WHERE [o].[OrderID] IS NOT NULL
GROUP BY [o].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupJoin_GroupBy_Aggregate_2(bool async)
        {
            await base.GroupJoin_GroupBy_Aggregate_2(async);

            AssertSql(@"
SELECT [c].[CustomerID] AS [Key], MAX([c].[City]) AS [Max]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT 0 AS [$empty], [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o]
) AS [o_0] ON [c].[CustomerID] = [o_0].[CustomerID]
GROUP BY [c].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupJoin_GroupBy_Aggregate_3(bool async)
        {
            await base.GroupJoin_GroupBy_Aggregate_3(async);

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], AVG(CAST([o].[OrderID] AS float)) AS [Average]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT 0 AS [$empty], [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
) AS [c_0] ON [o].[CustomerID] = [c_0].[CustomerID]
GROUP BY [o].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupJoin_GroupBy_Aggregate_4(bool async)
        {
            await base.GroupJoin_GroupBy_Aggregate_4(async);

            AssertSql(@"
SELECT [c].[CustomerID] AS [Value], MAX([c].[City]) AS [Max]
FROM [Customers] AS [c]
LEFT JOIN (
    SELECT 0 AS [$empty], [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o]
) AS [o_0] ON [c].[CustomerID] = [o_0].[CustomerID]
GROUP BY [c].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task GroupJoin_GroupBy_Aggregate_5(bool async)
        {
            await base.GroupJoin_GroupBy_Aggregate_5(async);

            AssertSql(@"
SELECT [o].[OrderID] AS [Value], AVG(CAST([o].[OrderID] AS float)) AS [Average]
FROM [Orders] AS [o]
LEFT JOIN (
    SELECT 0 AS [$empty], [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
) AS [c_0] ON [o].[CustomerID] = [c_0].[CustomerID]
GROUP BY [o].[OrderID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Join_complex_GroupBy_Aggregate(bool async)
        {
            await base.Join_complex_GroupBy_Aggregate(async);

            AssertSql(@"
SELECT [c].[CustomerID] AS [Key], AVG(CAST([o].[OrderID] AS float)) AS [Count]
FROM (
    SELECT TOP (100) [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_0]
    WHERE [o_0].[OrderID] < 10400
    ORDER BY [o_0].[OrderDate] ASC
) AS [o]
INNER JOIN (
    SELECT [c_0].[CustomerID] AS [CustomerID], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Country] AS [Country], [c_0].[Fax] AS [Fax], [c_0].[Phone] AS [Phone], [c_0].[PostalCode] AS [PostalCode], [c_0].[Region] AS [Region]
    FROM [Customers] AS [c_0]
    WHERE ([c_0].[CustomerID] <> N'DRACD') AND ([c_0].[CustomerID] <> N'FOLKO')
    ORDER BY [c_0].[City] ASC
    OFFSET 10 ROWS FETCH NEXT 50 ROWS ONLY
) AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Join_GroupBy_Aggregate(bool async)
        {
            await base.Join_GroupBy_Aggregate(async);

            AssertSql(@"
SELECT [c].[CustomerID] AS [Key], AVG(CAST([o].[OrderID] AS float)) AS [Count]
FROM [Orders] AS [o]
INNER JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Join_GroupBy_Aggregate_multijoins(bool async)
        {
            await base.Join_GroupBy_Aggregate_multijoins(async);

            AssertSql(@"
SELECT [c].[CustomerID] AS [c.CustomerID], [c].[Address] AS [c.Address], [c].[City] AS [c.City], [c].[CompanyName] AS [c.CompanyName], [c].[ContactName] AS [c.ContactName], [c].[ContactTitle] AS [c.ContactTitle], [c].[Country] AS [c.Country], [c].[Fax] AS [c.Fax], [c].[Phone] AS [c.Phone], [c].[PostalCode] AS [c.PostalCode], [c].[Region] AS [c.Region], [o].[OrderID] AS [o.OrderID], [o].[CustomerID] AS [o.CustomerID], [o].[EmployeeID] AS [o.EmployeeID], [o].[OrderDate] AS [o.OrderDate]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [g].[Key] AS [CustomerID], (
        SELECT MAX([g_0].[OrderID])
        FROM [Orders] AS [g_0]
        WHERE (([g].[Key] IS NULL AND [g_0].[CustomerID] IS NULL) OR ([g].[Key] = [g_0].[CustomerID]))
    ) AS [LastOrderID]
    FROM (
        SELECT [g_1].[CustomerID] AS [Key]
        FROM [Orders] AS [g_1]
        GROUP BY [g_1].[CustomerID]
    ) AS [g]
    WHERE (
        SELECT COUNT(*)
        FROM [Orders] AS [g_0]
        WHERE (([g].[Key] IS NULL AND [g_0].[CustomerID] IS NULL) OR ([g].[Key] = [g_0].[CustomerID]))
    ) > 5
) AS [a] ON [c].[CustomerID] = [a].[CustomerID]
INNER JOIN [Orders] AS [o] ON [a].[LastOrderID] = [o].[OrderID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Join_GroupBy_Aggregate_on_key(bool async)
        {
            await base.Join_GroupBy_Aggregate_on_key(async);

            Fixture.AssertSql(@"SELECT [c].[CustomerID] AS [c.CustomerID], [c].[Address] AS [c.Address], [c].[City] AS [c.City], [c].[CompanyName] AS [c.CompanyName], [c].[ContactName] AS [c.ContactName], [c].[ContactTitle] AS [c.ContactTitle], [c].[Country] AS [c.Country], [c].[Fax] AS [c.Fax], [c].[Phone] AS [c.Phone], [c].[PostalCode] AS [c.PostalCode], [c].[Region] AS [c.Region], [a].[LastOrderID] AS [LastOrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [g].[Key] AS [Key], (
        SELECT MAX([g_0].[OrderID])
        FROM [Orders] AS [g_0]
        WHERE (([g].[Key] IS NULL AND [g_0].[CustomerID] IS NULL) OR ([g].[Key] = [g_0].[CustomerID]))
    ) AS [LastOrderID]
    FROM (
        SELECT [g_1].[CustomerID] AS [Key]
        FROM [Orders] AS [g_1]
        GROUP BY [g_1].[CustomerID]
    ) AS [g]
    WHERE (
        SELECT COUNT(*)
        FROM [Orders] AS [g_0]
        WHERE (([g].[Key] IS NULL AND [g_0].[CustomerID] IS NULL) OR ([g].[Key] = [g_0].[CustomerID]))
    ) > 5
) AS [a] ON [c].[CustomerID] = [a].[Key]");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Join_GroupBy_Aggregate_single_join(bool async)
        {
            await base.Join_GroupBy_Aggregate_single_join(async);

            AssertSql(@"
SELECT [c].[CustomerID] AS [c.CustomerID], [c].[Address] AS [c.Address], [c].[City] AS [c.City], [c].[CompanyName] AS [c.CompanyName], [c].[ContactName] AS [c.ContactName], [c].[ContactTitle] AS [c.ContactTitle], [c].[Country] AS [c.Country], [c].[Fax] AS [c.Fax], [c].[Phone] AS [c.Phone], [c].[PostalCode] AS [c.PostalCode], [c].[Region] AS [c.Region], [a].[LastOrderID] AS [LastOrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [g].[Key] AS [CustomerID], (
        SELECT MAX([g_0].[OrderID])
        FROM [Orders] AS [g_0]
        WHERE (([g].[Key] IS NULL AND [g_0].[CustomerID] IS NULL) OR ([g].[Key] = [g_0].[CustomerID]))
    ) AS [LastOrderID]
    FROM (
        SELECT [g_1].[CustomerID] AS [Key]
        FROM [Orders] AS [g_1]
        GROUP BY [g_1].[CustomerID]
    ) AS [g]
    WHERE (
        SELECT COUNT(*)
        FROM [Orders] AS [g_0]
        WHERE (([g].[Key] IS NULL AND [g_0].[CustomerID] IS NULL) OR ([g].[Key] = [g_0].[CustomerID]))
    ) > 5
) AS [a] ON [c].[CustomerID] = [a].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Join_GroupBy_Aggregate_with_another_join(bool async)
        {
            await base.Join_GroupBy_Aggregate_with_another_join(async);

            AssertSql(@"
SELECT [c].[CustomerID] AS [c.CustomerID], [c].[Address] AS [c.Address], [c].[City] AS [c.City], [c].[CompanyName] AS [c.CompanyName], [c].[ContactName] AS [c.ContactName], [c].[ContactTitle] AS [c.ContactTitle], [c].[Country] AS [c.Country], [c].[Fax] AS [c.Fax], [c].[Phone] AS [c.Phone], [c].[PostalCode] AS [c.PostalCode], [c].[Region] AS [c.Region], [a].[LastOrderID] AS [LastOrderID], [o].[OrderID] AS [OrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT [g].[Key] AS [CustomerID], (
        SELECT MAX([g_0].[OrderID])
        FROM [Orders] AS [g_0]
        WHERE (([g].[Key] IS NULL AND [g_0].[CustomerID] IS NULL) OR ([g].[Key] = [g_0].[CustomerID]))
    ) AS [LastOrderID]
    FROM (
        SELECT [g_1].[CustomerID] AS [Key]
        FROM [Orders] AS [g_1]
        GROUP BY [g_1].[CustomerID]
    ) AS [g]
    WHERE (
        SELECT COUNT(*)
        FROM [Orders] AS [g_0]
        WHERE (([g].[Key] IS NULL AND [g_0].[CustomerID] IS NULL) OR ([g].[Key] = [g_0].[CustomerID]))
    ) > 5
) AS [a] ON [c].[CustomerID] = [a].[CustomerID]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task OrderBy_GroupBy_Aggregate(bool async)
        {
            await base.OrderBy_GroupBy_Aggregate(async);

            AssertSql(@"
SELECT SUM([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task OrderBy_GroupBy_SelectMany(bool async)
        {
            await base.OrderBy_GroupBy_SelectMany(async);

            AssertSql(@"
SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
FROM (
    SELECT [o_0].[CustomerID] AS [Key]
    FROM [Orders] AS [o_0]
    GROUP BY [o_0].[CustomerID]
) AS [g]
INNER JOIN [Orders] AS [o] ON [g].[Key] = [o].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task OrderBy_GroupBy_SelectMany_shadow(bool async)
        {
            await base.OrderBy_GroupBy_SelectMany_shadow(async);

            AssertSql(@"
SELECT [e].[Title]
FROM (
    SELECT [e_0].[EmployeeID] AS [Key]
    FROM [Employees] AS [e_0]
    GROUP BY [e_0].[EmployeeID]
) AS [g]
INNER JOIN [Employees] AS [e] ON [g].[Key] = [e].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task OrderBy_Skip_GroupBy_Aggregate(bool async)
        {
            await base.OrderBy_Skip_GroupBy_Aggregate(async);

            AssertSql(@"
SELECT AVG(CAST([o].[OrderID] AS float))
FROM (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_0]
    ORDER BY [o_0].[OrderID] ASC
    OFFSET 80 ROWS
) AS [o]
GROUP BY [o].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task OrderBy_Skip_Take_GroupBy_Aggregate(bool async)
        {
            await base.OrderBy_Skip_Take_GroupBy_Aggregate(async);

            AssertSql(@"
SELECT MAX([o].[OrderID])
FROM (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_0]
    ORDER BY [o_0].[OrderID] ASC
    OFFSET 80 ROWS FETCH NEXT 500 ROWS ONLY
) AS [o]
GROUP BY [o].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task OrderBy_Take_GroupBy_Aggregate(bool async)
        {
            await base.OrderBy_Take_GroupBy_Aggregate(async);

            AssertSql(@"
SELECT MIN([o].[OrderID])
FROM (
    SELECT TOP (500) [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_0]
    ORDER BY [o_0].[OrderID] ASC
) AS [o]
GROUP BY [o].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task SelectMany_GroupBy_Aggregate(bool async)
        {
            await base.SelectMany_GroupBy_Aggregate(async);

            AssertSql(@"
SELECT [o].[EmployeeID] AS [Key], COUNT(*) AS [c]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
GROUP BY [o].[EmployeeID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Select_anonymous_GroupBy_Aggregate(bool async)
        {
            await base.Select_anonymous_GroupBy_Aggregate(async);

            AssertSql(@"
SELECT MIN([o].[OrderDate]) AS [Min], MAX([o].[OrderDate]) AS [Max], SUM([o].[OrderID]) AS [Sum], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
GROUP BY [o].[CustomerID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Select_GroupBy_All(bool async)
        {
            await base.Select_GroupBy_All(async);

            AssertSql(@"
SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM (
        SELECT [o].[CustomerID] AS [Key]
        FROM [Orders] AS [o]
        GROUP BY [o].[CustomerID]
    ) AS [a]
    WHERE ([a].[Key] IS NULL OR ([a].[Key] <> N'ALFKI'))
) THEN 0 ELSE 1 END) AS bit)
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Select_GroupBy_SelectMany(bool async)
        {
            await base.Select_GroupBy_SelectMany(async);

            AssertSql(@"
SELECT [o].[OrderID] AS [Order], [o].[CustomerID] AS [Customer]
FROM (
    SELECT [o_0].[OrderID] AS [Key]
    FROM [Orders] AS [o_0]
    GROUP BY [o_0].[OrderID]
) AS [g]
INNER JOIN [Orders] AS [o] ON [g].[Key] = [o].[OrderID]
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Select_nested_collection_with_groupby(bool async)
        {
            await base.Select_nested_collection_with_groupby(async);

            AssertSql(@"
SELECT [c].[CustomerID] AS [$outer.CustomerID], [c].[Address] AS [$outer.Address], [c].[City] AS [$outer.City], [c].[CompanyName] AS [$outer.CompanyName], [c].[ContactName] AS [$outer.ContactName], [c].[ContactTitle] AS [$outer.ContactTitle], [c].[Country] AS [$outer.Country], [c].[Fax] AS [$outer.Fax], [c].[Phone] AS [$outer.Phone], [c].[PostalCode] AS [$outer.PostalCode], [c].[Region] AS [$outer.Region], (
    SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    FOR JSON PATH
) AS [$inner]
FROM [Customers] AS [c]
WHERE LEFT([c].[CustomerID], LEN(N'A')) = N'A'
");
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override async Task Self_join_GroupBy_Aggregate(bool async)
        {
            await base.Self_join_GroupBy_Aggregate(async);

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], AVG(CAST([o2].[OrderID] AS float)) AS [Count]
FROM [Orders] AS [o]
INNER JOIN [Orders] AS [o2] ON [o].[OrderID] = [o2].[OrderID]
WHERE [o].[OrderID] < 10400
GROUP BY [o].[CustomerID]
");
        }

        #region utilities

        protected override void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

        private void AssertSql(string sql) => Fixture.AssertSql(sql);

        private void AssertSqlStartsWith(string sql) => Fixture.AssertSqlStartsWith(sql);

        protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture) =>
            new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

        #endregion
    }
}
