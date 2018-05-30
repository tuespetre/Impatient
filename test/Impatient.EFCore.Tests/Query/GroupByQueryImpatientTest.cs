using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using System.Linq;
using Xunit;
using Xunit.Sdk;

namespace Impatient.EFCore.Tests.Query
{
    public class GroupByQueryImpatientTest : GroupByQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public GroupByQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            ClearLog();
        }

        [Fact]
        public override void Anonymous_projection_Distinct_GroupBy_Aggregate()
        {
            base.Anonymous_projection_Distinct_GroupBy_Aggregate();

            AssertSql(@"
SELECT [o].[EmployeeID] AS [Key], COUNT(*) AS [c]
FROM (
    SELECT DISTINCT [o_0].[OrderID] AS [OrderID], [o_0].[EmployeeID] AS [EmployeeID]
    FROM [Orders] AS [o_0]
) AS [o]
GROUP BY [o].[EmployeeID]
");
        }

        [Fact]
        public override void Distinct_GroupBy_Aggregate()
        {
            base.Distinct_GroupBy_Aggregate();

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [c]
FROM (
    SELECT DISTINCT [g].[OrderID] AS [OrderID], [g].[CustomerID] AS [CustomerID], [g].[EmployeeID] AS [EmployeeID], [g].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [g]
) AS [o]
GROUP BY [o].[CustomerID]
");
        }

        [Fact]
        public override void Distinct_GroupBy_OrderBy_key()
        {
            base.Distinct_GroupBy_OrderBy_key();

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

        [Fact]
        public override void Double_GroupBy_with_aggregate()
        {
            base.Double_GroupBy_with_aggregate();

            Fixture.AssertSql(@"SELECT [g].[Key.OrderDate] AS [Key], (
    SELECT [g_0].[Key.OrderID] AS [Key.OrderID], [g_0].[Key.OrderDate] AS [Key.OrderDate], (
        SELECT [g_1].[OrderID] AS [OrderID], [g_1].[CustomerID] AS [CustomerID], [g_1].[EmployeeID] AS [EmployeeID], [g_1].[OrderDate] AS [OrderDate]
        FROM [Orders] AS [g_1]
        WHERE ([g_0].[Key.OrderID] = [g_1].[OrderID]) AND ((([g_0].[Key.OrderDate] IS NULL AND [g_1].[OrderDate] IS NULL) OR ([g_0].[Key.OrderDate] = [g_1].[OrderDate])))
        FOR JSON PATH
    ) AS [Elements]
    FROM (
        SELECT [g_2].[OrderID] AS [Key.OrderID], [g_2].[OrderDate] AS [Key.OrderDate]
        FROM [Orders] AS [g_2]
        GROUP BY [g_2].[OrderID], [g_2].[OrderDate]
    ) AS [g_0]
    WHERE (([g].[Key.OrderDate] IS NULL AND [g_0].[Key.OrderDate] IS NULL) OR ([g].[Key.OrderDate] = [g_0].[Key.OrderDate]))
    FOR JSON PATH
) AS [Elements]
FROM (
    SELECT [g_3].[OrderID] AS [Key.OrderID], [g_3].[OrderDate] AS [Key.OrderDate]
    FROM [Orders] AS [g_3]
    GROUP BY [g_3].[OrderID], [g_3].[OrderDate]
) AS [g]
GROUP BY [g].[Key.OrderDate]");
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg()
        {
            var ex = Assert.Throws<EqualException>(() => base.GroupBy_after_predicate_Constant_Select_Sum_Min_Key_Max_Avg());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("577", ex.Actual);
        }

        [Fact]
        public override void GroupBy_aggregate_Contains()
        {
            base.GroupBy_aggregate_Contains();

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

        [Fact]
        public override void GroupBy_Aggregate_Join()
        {
            base.GroupBy_Aggregate_Join();

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

        [Fact]
        public override void GroupBy_aggregate_Pushdown()
        {
            base.GroupBy_aggregate_Pushdown();

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

        [Fact]
        public override void GroupBy_anonymous()
        {
            base.GroupBy_anonymous();

            AssertSql(@"
SELECT [c].[City] AS [Key], (
    SELECT [c_0].[City] AS [City], [c_0].[CustomerID] AS [CustomerID]
    FROM [Customers] AS [c_0]
    WHERE (([c].[City] IS NULL AND [c_0].[City] IS NULL) OR ([c].[City] = [c_0].[City]))
    FOR JSON PATH
) AS [Elements]
FROM [Customers] AS [c]
GROUP BY [c].[City]
");
        }

        [Fact]
        public override void GroupBy_anonymous_key_without_aggregate()
        {
            base.GroupBy_anonymous_key_without_aggregate();

            Fixture.AssertSql(@"SELECT [g].[CustomerID] AS [Key.CustomerID], [g].[OrderDate] AS [Key.OrderDate], [g].[CustomerID] AS [g.Key.CustomerID], [g].[OrderDate] AS [g.Key.OrderDate], (
    SELECT [g_0].[OrderID] AS [OrderID], [g_0].[CustomerID] AS [CustomerID], [g_0].[EmployeeID] AS [EmployeeID], [g_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [g_0]
    WHERE ((([g].[CustomerID] IS NULL AND [g_0].[CustomerID] IS NULL) OR ([g].[CustomerID] = [g_0].[CustomerID]))) AND ((([g].[OrderDate] IS NULL AND [g_0].[OrderDate] IS NULL) OR ([g].[OrderDate] = [g_0].[OrderDate])))
    FOR JSON PATH
) AS [g.Elements]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[OrderDate]");
        }

        [Fact]
        public override void GroupBy_anonymous_Select_Average()
        {
            base.GroupBy_anonymous_Select_Average();

            AssertSql(@"
SELECT AVG(CAST([g].[OrderID] AS float))
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_anonymous_Select_Count()
        {
            base.GroupBy_anonymous_Select_Count();

            AssertSql(@"
SELECT COUNT(*)
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_anonymous_Select_LongCount()
        {
            base.GroupBy_anonymous_Select_LongCount();

            AssertSql(@"
SELECT COUNT_BIG(*)
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_anonymous_Select_Max()
        {
            base.GroupBy_anonymous_Select_Max();

            AssertSql(@"
SELECT MAX([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_anonymous_Select_Min()
        {
            base.GroupBy_anonymous_Select_Min();

            AssertSql(@"
SELECT MIN([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_anonymous_Select_Sum()
        {
            base.GroupBy_anonymous_Select_Sum();

            AssertSql(@"
SELECT SUM([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_anonymous_Select_Sum_Min_Max_Avg()
        {
            base.GroupBy_anonymous_Select_Sum_Min_Max_Avg();

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void GroupBy_anonymous_subquery()
        {
            AssertQuery<Customer>(
                cs => cs
                    .Select(c => new { c.City, c.CustomerID })
                    .GroupBy(a => from c2 in cs select c2),
                elementAsserter: (a, b) =>
                {
                    var ca = (IGrouping<IQueryable<Customer>, dynamic>)a;
                    var cb = (IGrouping<IQueryable<Customer>, dynamic>)b;

                    Assert.Equal(ca.Key.AsEnumerable(), cb.Key.AsEnumerable());
                    Assert.Equal(ca.AsEnumerable().OrderBy(d => d.CustomerID), cb.AsEnumerable().OrderBy(d => d.CustomerID));
                },
                elementSorter: o =>
                {
                    var co = (IGrouping<IQueryable<Customer>, dynamic>)o;

                    return co.First().CustomerID;
                },
                assertOrder: false,
                entryCount: 91);

            AssertSqlStartsWith(@"
SELECT [c].[City] AS [City], [c].[CustomerID] AS [CustomerID]
FROM [Customers] AS [c]

SELECT [c2].[CustomerID] AS [CustomerID], [c2].[Address] AS [Address], [c2].[City] AS [City], [c2].[CompanyName] AS [CompanyName], [c2].[ContactName] AS [ContactName], [c2].[ContactTitle] AS [ContactTitle], [c2].[Country] AS [Country], [c2].[Fax] AS [Fax], [c2].[Phone] AS [Phone], [c2].[PostalCode] AS [PostalCode], [c2].[Region] AS [Region]
FROM [Customers] AS [c2]
");
        }

        [Fact]
        public override void GroupBy_anonymous_with_alias_Select_Key_Sum()
        {
            base.GroupBy_anonymous_with_alias_Select_Key_Sum();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], SUM([g].[OrderID]) AS [Sum]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_anonymous_with_where()
        {
            base.GroupBy_anonymous_with_where();

            AssertSql(@"
@p0_0='Argentina' (Nullable = false) (Size = 9)
@p0_1='Austria' (Nullable = false) (Size = 7)
@p0_2='Brazil' (Nullable = false) (Size = 6)
@p0_3='France' (Nullable = false) (Size = 6)
@p0_4='Germany' (Nullable = false) (Size = 7)
@p0_5='USA' (Nullable = false) (Size = 3)
@p1_0='Argentina' (Nullable = false) (Size = 9)
@p1_1='Austria' (Nullable = false) (Size = 7)
@p1_2='Brazil' (Nullable = false) (Size = 6)
@p1_3='France' (Nullable = false) (Size = 6)
@p1_4='Germany' (Nullable = false) (Size = 7)
@p1_5='USA' (Nullable = false) (Size = 3)

SELECT [c].[City] AS [Key], (
    SELECT [c_0].[City] AS [City], [c_0].[CustomerID] AS [CustomerID]
    FROM [Customers] AS [c_0]
    WHERE [c_0].[Country] IN (@p0_0, @p0_1, @p0_2, @p0_3, @p0_4, @p0_5) AND ((([c].[City] IS NULL AND [c_0].[City] IS NULL) OR ([c].[City] = [c_0].[City])))
    FOR JSON PATH
) AS [Elements]
FROM [Customers] AS [c]
WHERE [c].[Country] IN (@p1_0, @p1_1, @p1_2, @p1_3, @p1_4, @p1_5)
GROUP BY [c].[City]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Average()
        {
            base.GroupBy_Composite_Select_Average();

            AssertSql(@"
SELECT AVG(CAST([g].[OrderID] AS float))
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Count()
        {
            base.GroupBy_Composite_Select_Count();

            AssertSql(@"
SELECT COUNT(*)
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Dto_Sum_Min_Key_flattened_Max_Avg()
        {
            base.GroupBy_Composite_Select_Dto_Sum_Min_Key_flattened_Max_Avg();

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], [g].[CustomerID] AS [CustomerId], [g].[EmployeeID] AS [EmployeeId], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Key_Average()
        {
            base.GroupBy_Composite_Select_Key_Average();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID], AVG(CAST([g].[OrderID] AS float)) AS [Average]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Key_Count()
        {
            base.GroupBy_Composite_Select_Key_Count();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID], COUNT(*) AS [Count]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Key_LongCount()
        {
            base.GroupBy_Composite_Select_Key_LongCount();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID], COUNT_BIG(*) AS [LongCount]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Key_Max()
        {
            base.GroupBy_Composite_Select_Key_Max();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID], MAX([g].[OrderID]) AS [Max]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Key_Min()
        {
            base.GroupBy_Composite_Select_Key_Min();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID], MIN([g].[OrderID]) AS [Min]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Key_Sum()
        {
            base.GroupBy_Composite_Select_Key_Sum();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID], SUM([g].[OrderID]) AS [Sum]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Key_Sum_Min_Max_Avg()
        {
            base.GroupBy_Composite_Select_Key_Sum_Min_Max_Avg();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID], SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_LongCount()
        {
            base.GroupBy_Composite_Select_LongCount();

            AssertSql(@"
SELECT COUNT_BIG(*)
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Max()
        {
            base.GroupBy_Composite_Select_Max();

            AssertSql(@"
SELECT MAX([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Min()
        {
            base.GroupBy_Composite_Select_Min();

            AssertSql(@"
SELECT MIN([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Sum()
        {
            base.GroupBy_Composite_Select_Sum();

            AssertSql(@"
SELECT SUM([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg()
        {
            base.GroupBy_Composite_Select_Sum_Min_Key_flattened_Max_Avg();

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], [g].[CustomerID] AS [CustomerID], [g].[EmployeeID] AS [EmployeeID], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Sum_Min_Key_Max_Avg()
        {
            base.GroupBy_Composite_Select_Sum_Min_Key_Max_Avg();

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Sum_Min_Max_Avg()
        {
            base.GroupBy_Composite_Select_Sum_Min_Max_Avg();

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg()
        {
            base.GroupBy_Composite_Select_Sum_Min_part_Key_flattened_Max_Avg();

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], [g].[CustomerID] AS [CustomerID], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void GroupBy_Constant_Select_Sum_Min_Key_Max_Avg()
        {
            var ex = Assert.Throws<EqualException>(() => base.GroupBy_Constant_Select_Sum_Min_Key_Max_Avg());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("830", ex.Actual);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg()
        {
            var ex = Assert.Throws<EqualException>(() => base.GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("830", ex.Actual);
        }

        [Fact]
        public override void GroupBy_DateTimeOffset_Property()
        {
            base.GroupBy_DateTimeOffset_Property();

            AssertSql(@"
SELECT DATEPART(month, [o].[OrderDate]) AS [Key], (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_0]
    WHERE ([o_0].[OrderDate] IS NOT NULL) AND (DATEPART(month, [o].[OrderDate]) = DATEPART(month, [o_0].[OrderDate]))
    FOR JSON PATH
) AS [Elements]
FROM [Orders] AS [o]
WHERE [o].[OrderDate] IS NOT NULL
GROUP BY DATEPART(month, [o].[OrderDate])
");
        }

        [Fact]
        public override void GroupBy_Distinct()
        {
            base.GroupBy_Distinct();

            AssertSql(@"
SELECT [g].[Key]
FROM (
    SELECT DISTINCT [g_0].[CustomerID] AS [Key]
    FROM [Orders] AS [g_0]
    GROUP BY [g_0].[CustomerID]
) AS [g]
");
        }

        [Fact]
        public override void GroupBy_Dto_as_element_selector_Select_Sum()
        {
            base.GroupBy_Dto_as_element_selector_Select_Sum();

            AssertSql(@"
SELECT SUM(CAST([g].[EmployeeID] AS bigint)) AS [Sum], [g].[CustomerID] AS [Key]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Dto_as_key_Select_Sum()
        {
            base.GroupBy_Dto_as_key_Select_Sum();

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], [g].[CustomerID] AS [Key.CustomerID], [g].[EmployeeID] AS [Key.EmployeeID]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID], [g].[EmployeeID]
");
        }

        [Fact]
        public override void GroupBy_empty_key_Aggregate()
        {
            base.GroupBy_empty_key_Aggregate();

            Fixture.AssertSql(@"SELECT [g].[OrderID] AS [OrderID], [g].[CustomerID] AS [CustomerID], [g].[EmployeeID] AS [EmployeeID], [g].[OrderDate] AS [OrderDate]
FROM [Orders] AS [g]");
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void GroupBy_empty_key_Aggregate_Key()
        {
            var ex = Assert.Throws<EqualException>(() => base.GroupBy_empty_key_Aggregate_Key());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("830", ex.Actual);
        }

        [Fact]
        public override void GroupBy_filter_count()
        {
            base.GroupBy_filter_count();

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

        [Fact]
        public override void GroupBy_filter_count_OrderBy_count_Select_sum()
        {
            base.GroupBy_filter_count_OrderBy_count_Select_sum();

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

        [Fact]
        public override void GroupBy_filter_key()
        {
            base.GroupBy_filter_key();

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

        [Fact]
        public override void GroupBy_first()
        {
            base.GroupBy_first();

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_0]
    WHERE ([o_0].[CustomerID] = N'ALFKI') AND ((([o].[CustomerID] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[CustomerID] = [o_0].[CustomerID])))
    FOR JSON PATH
) AS [Elements]
FROM [Orders] AS [o]
WHERE [o].[CustomerID] = N'ALFKI'
GROUP BY [o].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_join_anonymous()
        {
            base.GroupBy_join_anonymous();

            AssertSql(@"
SELECT [x].[Key.OrderID] AS [Key.OrderID], [x].[Key.OrderDate] AS [Key.OrderDate], (
    SELECT [orderDetail].[ProductID] AS [ProductID], [orderDetail].[Quantity] AS [Quantity], [orderDetail].[UnitPrice] AS [UnitPrice]
    FROM [Orders] AS [order]
    INNER JOIN [Order Details] AS [orderDetail] ON [order].[OrderID] = [orderDetail].[OrderID]
    WHERE ([x].[Key.OrderID] = [order].[OrderID]) AND ((([x].[Key.OrderDate] IS NULL AND [order].[OrderDate] IS NULL) OR ([x].[Key.OrderDate] = [order].[OrderDate])))
    FOR JSON PATH
) AS [Elements]
FROM (
    SELECT [order_0].[OrderID] AS [Key.OrderID], [order_0].[OrderDate] AS [Key.OrderDate]
    FROM [Orders] AS [order_0]
    INNER JOIN [Order Details] AS [orderDetail_0] ON [order_0].[OrderID] = [orderDetail_0].[OrderID]
    GROUP BY [order_0].[OrderID], [order_0].[OrderDate]
) AS [x]
WHERE [x].[Key.OrderID] = 10248
");
        }

        [Fact]
        public override void GroupBy_join_default_if_empty_anonymous()
        {
            base.GroupBy_join_default_if_empty_anonymous();

            AssertSql(@"
SELECT [x].[Key.OrderID] AS [Key.OrderID], [x].[Key.OrderDate] AS [Key.OrderDate], (
    SELECT [orderDetail].[ProductID] AS [ProductID], [orderDetail].[Quantity] AS [Quantity], [orderDetail].[UnitPrice] AS [UnitPrice]
    FROM [Orders] AS [order]
    LEFT JOIN (
        SELECT [orderDetail_0].[OrderID] AS [OrderID], [orderDetail_0].[ProductID] AS [ProductID], [orderDetail_0].[Discount] AS [Discount], [orderDetail_0].[Quantity] AS [Quantity], [orderDetail_0].[UnitPrice] AS [UnitPrice]
        FROM [Order Details] AS [orderDetail_0]
    ) AS [orderDetail] ON [order].[OrderID] = [orderDetail].[OrderID]
    WHERE ([x].[Key.OrderID] = [order].[OrderID]) AND ((([x].[Key.OrderDate] IS NULL AND [order].[OrderDate] IS NULL) OR ([x].[Key.OrderDate] = [order].[OrderDate])))
    FOR JSON PATH
) AS [Elements]
FROM (
    SELECT [order_0].[OrderID] AS [Key.OrderID], [order_0].[OrderDate] AS [Key.OrderDate]
    FROM [Orders] AS [order_0]
    LEFT JOIN (
        SELECT 0 AS [$empty], [orderDetail_1].[OrderID] AS [OrderID], [orderDetail_1].[ProductID] AS [ProductID], [orderDetail_1].[Discount] AS [Discount], [orderDetail_1].[Quantity] AS [Quantity], [orderDetail_1].[UnitPrice] AS [UnitPrice]
        FROM [Order Details] AS [orderDetail_1]
    ) AS [orderDetail_2] ON [order_0].[OrderID] = [orderDetail_2].[OrderID]
    GROUP BY [order_0].[OrderID], [order_0].[OrderDate]
) AS [x]
WHERE [x].[Key.OrderID] = 10248
");
        }

        [Fact]
        public override void GroupBy_multi_navigation_members_Aggregate()
        {
            base.GroupBy_multi_navigation_members_Aggregate();

            AssertSql(@"
SELECT [o].[CustomerID] AS [CompositeKey.CustomerID], [p].[ProductName] AS [CompositeKey.ProductName], COUNT(*) AS [Count]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [o] ON [od].[OrderID] = [o].[OrderID]
INNER JOIN [Products] AS [p] ON [od].[ProductID] = [p].[ProductID]
GROUP BY [o].[CustomerID], [p].[ProductName]
");
        }

        [Fact]
        public override void GroupBy_nested_order_by_enumerable()
        {
            base.GroupBy_nested_order_by_enumerable();

            AssertSql(@"
SELECT (
    SELECT [c].[Country] AS [Country], [c].[CustomerID] AS [CustomerID]
    FROM [Customers] AS [c]
    WHERE (([c_0].[Country] IS NULL AND [c].[Country] IS NULL) OR ([c_0].[Country] = [c].[Country]))
    ORDER BY [c].[CustomerID] ASC
    FOR JSON PATH
)
FROM [Customers] AS [c_0]
GROUP BY [c_0].[Country]
");
        }

        [Fact]
        public override void GroupBy_optional_navigation_member_Aggregate()
        {
            base.GroupBy_optional_navigation_member_Aggregate();

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

        [Fact]
        public override void GroupBy_OrderBy_count()
        {
            base.GroupBy_OrderBy_count();

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
ORDER BY COUNT(*) ASC, [o].[CustomerID] ASC
");
        }

        [Fact]
        public override void GroupBy_OrderBy_count_Select_sum()
        {
            base.GroupBy_OrderBy_count_Select_sum();

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], SUM([o].[OrderID]) AS [Sum]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
ORDER BY COUNT(*) ASC, [o].[CustomerID] ASC
");
        }

        [Fact]
        public override void GroupBy_OrderBy_key()
        {
            base.GroupBy_OrderBy_key();

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], COUNT(*) AS [c]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
ORDER BY [o].[CustomerID] ASC
");
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void GroupBy_param_Select_Sum_Min_Key_Max_Avg()
        {
            var ex = Assert.Throws<EqualException>(() => base.GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("830", ex.Actual);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void GroupBy_param_with_element_selector_Select_Sum_Min_Key_Max_Avg()
        {
            var ex = Assert.Throws<EqualException>(() => base.GroupBy_Constant_with_element_selector_Select_Sum_Min_Key_Max_Avg());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("830", ex.Actual);
        }

        [Fact]
        public override void GroupBy_Property_anonymous_element_selector_Average()
        {
            base.GroupBy_Property_anonymous_element_selector_Average();

            AssertSql(@"
SELECT AVG(CAST([g].[OrderID] AS float))
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_anonymous_element_selector_Count()
        {
            base.GroupBy_Property_anonymous_element_selector_Count();

            AssertSql(@"
SELECT COUNT(*)
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_anonymous_element_selector_LongCount()
        {
            base.GroupBy_Property_anonymous_element_selector_LongCount();

            AssertSql(@"
SELECT COUNT_BIG(*)
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_anonymous_element_selector_Max()
        {
            base.GroupBy_Property_anonymous_element_selector_Max();

            AssertSql(@"
SELECT MAX([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_anonymous_element_selector_Min()
        {
            base.GroupBy_Property_anonymous_element_selector_Min();

            AssertSql(@"
SELECT MIN([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_anonymous_element_selector_Sum()
        {
            base.GroupBy_Property_anonymous_element_selector_Sum();

            AssertSql(@"
SELECT SUM([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg()
        {
            base.GroupBy_Property_anonymous_element_selector_Sum_Min_Max_Avg();

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[EmployeeID]) AS [Min], MAX([g].[EmployeeID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_scalar_element_selector_Average()
        {
            base.GroupBy_Property_scalar_element_selector_Average();

            AssertSql(@"
SELECT AVG(CAST([g].[OrderID] AS float))
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_scalar_element_selector_Count()
        {
            base.GroupBy_Property_scalar_element_selector_Count();

            AssertSql(@"
SELECT COUNT([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_scalar_element_selector_LongCount()
        {
            base.GroupBy_Property_scalar_element_selector_LongCount();

            AssertSql(@"
SELECT COUNT_BIG([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_scalar_element_selector_Max()
        {
            base.GroupBy_Property_scalar_element_selector_Max();

            AssertSql(@"
SELECT MAX([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_scalar_element_selector_Min()
        {
            base.GroupBy_Property_scalar_element_selector_Min();

            AssertSql(@"
SELECT MIN([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_scalar_element_selector_Sum()
        {
            base.GroupBy_Property_scalar_element_selector_Sum();

            AssertSql(@"
SELECT SUM([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg()
        {
            base.GroupBy_Property_scalar_element_selector_Sum_Min_Max_Avg();

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_Select_Average()
        {
            base.GroupBy_Property_Select_Average();

            AssertSql(@"
SELECT AVG(CAST([g].[OrderID] AS float))
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_Select_Count()
        {
            base.GroupBy_Property_Select_Count();

            AssertSql(@"
SELECT COUNT(*)
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_Select_Key_Average()
        {
            base.GroupBy_Property_Select_Key_Average();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], AVG(CAST([g].[OrderID] AS float)) AS [Average]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_Select_Key_Count()
        {
            base.GroupBy_Property_Select_Key_Count();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], COUNT(*) AS [Count]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_Select_Key_LongCount()
        {
            base.GroupBy_Property_Select_Key_LongCount();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], COUNT_BIG(*) AS [LongCount]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_Select_Key_Max()
        {
            base.GroupBy_Property_Select_Key_Max();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], MAX([g].[OrderID]) AS [Max]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_Select_Key_Min()
        {
            base.GroupBy_Property_Select_Key_Min();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], MIN([g].[OrderID]) AS [Min]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_Select_Key_Sum()
        {
            base.GroupBy_Property_Select_Key_Sum();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], SUM([g].[OrderID]) AS [Sum]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_Select_Key_Sum_Min_Max_Avg()
        {
            base.GroupBy_Property_Select_Key_Sum_Min_Max_Avg();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_Select_LongCount()
        {
            base.GroupBy_Property_Select_LongCount();

            AssertSql(@"
SELECT COUNT_BIG(*)
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_Select_Max()
        {
            base.GroupBy_Property_Select_Max();

            AssertSql(@"
SELECT MAX([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_Select_Min()
        {
            base.GroupBy_Property_Select_Min();

            AssertSql(@"
SELECT MIN([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_Select_Sum()
        {
            base.GroupBy_Property_Select_Sum();

            AssertSql(@"
SELECT SUM([g].[OrderID])
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_Select_Sum_Min_Key_Max_Avg()
        {
            base.GroupBy_Property_Select_Sum_Min_Key_Max_Avg();

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], [g].[CustomerID] AS [Key], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Property_Select_Sum_Min_Max_Avg()
        {
            base.GroupBy_Property_Select_Sum_Min_Max_Avg();

            AssertSql(@"
SELECT SUM([g].[OrderID]) AS [Sum], MIN([g].[OrderID]) AS [Min], MAX([g].[OrderID]) AS [Max], AVG(CAST([g].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_required_navigation_member_Aggregate()
        {
            base.GroupBy_required_navigation_member_Aggregate();

            AssertSql(@"
SELECT [o].[CustomerID] AS [CustomerId], COUNT(*) AS [Count]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [o] ON [od].[OrderID] = [o].[OrderID]
GROUP BY [o].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_SelectMany()
        {
            base.GroupBy_SelectMany();

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

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void GroupBy_Select_First_GroupBy()
        {
            AssertQuery<Customer>(
                cs =>
                    cs.GroupBy(c => c.City)
                      .Select(g => g.OrderBy(c => c.CustomerID).First())
                      .GroupBy(c => c.ContactName),
                elementSorter: GroupingSorter<string, object>(),
                elementAsserter: GroupingAsserter<string, dynamic>(d => d.CustomerID),
                entryCount: 91);

            AssertSql(@"
SELECT [g].[City] AS [Key], (
    SELECT [g_0].[CustomerID] AS [CustomerID], [g_0].[Address] AS [Address], [g_0].[City] AS [City], [g_0].[CompanyName] AS [CompanyName], [g_0].[ContactName] AS [ContactName], [g_0].[ContactTitle] AS [ContactTitle], [g_0].[Country] AS [Country], [g_0].[Fax] AS [Fax], [g_0].[Phone] AS [Phone], [g_0].[PostalCode] AS [PostalCode], [g_0].[Region] AS [Region]
    FROM [Customers] AS [g_0]
    WHERE (([g].[City] IS NULL AND [g_0].[City] IS NULL) OR ([g].[City] = [g_0].[City]))
    FOR JSON PATH
) AS [Elements]
FROM [Customers] AS [g]
GROUP BY [g].[City]
");
        }

        [Fact]
        public override void GroupBy_Select_sum_over_unmapped_property()
        {
            base.GroupBy_Select_sum_over_unmapped_property();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], (
    SELECT [g_0].[OrderID] AS [OrderID], [g_0].[CustomerID] AS [CustomerID], [g_0].[EmployeeID] AS [EmployeeID], [g_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [g_0]
    WHERE (([g].[CustomerID] IS NULL AND [g_0].[CustomerID] IS NULL) OR ([g].[CustomerID] = [g_0].[CustomerID]))
    FOR JSON PATH
) AS [Elements]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Shadow()
        {
            base.GroupBy_Shadow();

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

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void GroupBy_Shadow2()
        {
            var ex = Assert.Throws<EqualException>(() => base.GroupBy_Shadow2());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("1", ex.Actual);
        }

        [Fact]
        public override void GroupBy_Shadow3()
        {
            base.GroupBy_Shadow3();

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

        [Fact]
        public override void GroupBy_simple()
        {
            base.GroupBy_simple();

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_0]
    WHERE (([o].[CustomerID] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[CustomerID] = [o_0].[CustomerID]))
    FOR JSON PATH
) AS [Elements]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_simple2()
        {
            base.GroupBy_simple2();

            AssertSql(@"
SELECT [g].[CustomerID] AS [Key], (
    SELECT [g_0].[OrderID] AS [OrderID], [g_0].[CustomerID] AS [CustomerID], [g_0].[EmployeeID] AS [EmployeeID], [g_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [g_0]
    WHERE (([g].[CustomerID] IS NULL AND [g_0].[CustomerID] IS NULL) OR ([g].[CustomerID] = [g_0].[CustomerID]))
    FOR JSON PATH
) AS [Elements]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Sum_constant()
        {
            base.GroupBy_Sum_constant();

            AssertSql(@"
SELECT SUM(CAST(1 AS int))
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Sum_constant_cast()
        {
            base.GroupBy_Sum_constant_cast();

            AssertSql(@"
SELECT SUM(CAST(1 AS bigint))
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_Where_in_aggregate()
        {
            base.GroupBy_Where_in_aggregate();

            AssertSql(@"
SELECT (
    SELECT COUNT(*)
    FROM [Orders] AS [g]
    WHERE ((([g_0].[CustomerID] IS NULL AND [g].[CustomerID] IS NULL) OR ([g_0].[CustomerID] = [g].[CustomerID]))) AND ([g].[OrderID] < 10300)
)
FROM [Orders] AS [g_0]
GROUP BY [g_0].[CustomerID]
");
        }

        [Fact]
        public override void GroupBy_with_aggregate_through_navigation_property()
        {
            base.GroupBy_with_aggregate_through_navigation_property();

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

        [Fact]
        public override void GroupBy_with_element_selector()
        {
            base.GroupBy_with_element_selector();

            AssertSql(@"
SELECT (
    SELECT [g].[OrderID]
    FROM [Orders] AS [g]
    WHERE (([g_0].[CustomerID] IS NULL AND [g].[CustomerID] IS NULL) OR ([g_0].[CustomerID] = [g].[CustomerID]))
    ORDER BY [g].[OrderID] ASC
    FOR JSON PATH
)
FROM [Orders] AS [g_0]
GROUP BY [g_0].[CustomerID]
ORDER BY [g_0].[CustomerID] ASC
");
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void GroupBy_with_element_selector2()
        {
            var ex = Assert.Throws<EqualException>(() => base.GroupBy_with_element_selector2());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("830", ex.Actual);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void GroupBy_with_element_selector3()
        {
            var ex = Assert.Throws<EqualException>(() => base.GroupBy_with_element_selector3());

            Assert.Equal("0", ex.Expected);
            Assert.Equal("9", ex.Actual);
        }

        [Fact]
        public override void GroupBy_with_orderby()
        {
            base.GroupBy_with_orderby();

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_0]
    WHERE (([o].[CustomerID] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[CustomerID] = [o_0].[CustomerID]))
    FOR JSON PATH
) AS [Elements]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
ORDER BY [o].[CustomerID] ASC
");
        }

        [Fact]
        public override void GroupBy_with_orderby_and_anonymous_projection()
        {
            base.GroupBy_with_orderby_and_anonymous_projection();

            AssertSql(@"
SELECT N'Foo' AS [Foo], [g].[CustomerID] AS [Group.Key], (
    SELECT [g_0].[OrderID] AS [OrderID], [g_0].[CustomerID] AS [CustomerID], [g_0].[EmployeeID] AS [EmployeeID], [g_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [g_0]
    WHERE (([g].[CustomerID] IS NULL AND [g_0].[CustomerID] IS NULL) OR ([g].[CustomerID] = [g_0].[CustomerID]))
    FOR JSON PATH
) AS [Group.Elements]
FROM [Orders] AS [g]
GROUP BY [g].[CustomerID]
ORDER BY [g].[CustomerID] ASC
");
        }

        [Fact]
        public override void GroupBy_with_orderby_take_skip_distinct()
        {
            base.GroupBy_with_orderby_take_skip_distinct();

            AssertSql(@"
SELECT DISTINCT [t].[Key] AS [Key], (
    SELECT [g].[OrderID] AS [OrderID], [g].[CustomerID] AS [CustomerID], [g].[EmployeeID] AS [EmployeeID], [g].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [g]
    WHERE (([t].[Key] IS NULL AND [g].[CustomerID] IS NULL) OR ([t].[Key] = [g].[CustomerID]))
    FOR JSON PATH
) AS [Elements]
FROM (
    SELECT [t_0].[Key] AS [Key]
    FROM (
        SELECT TOP (5) [g_0].[CustomerID] AS [Key]
        FROM [Orders] AS [g_0]
        GROUP BY [g_0].[CustomerID]
        ORDER BY [g_0].[CustomerID] ASC
    ) AS [t_0]
    ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) ASC
    OFFSET 3 ROWS
) AS [t]
");
        }

        [Fact]
        public override void GroupBy_with_result_selector()
        {
            base.GroupBy_with_result_selector();

            AssertSql(@"
SELECT SUM([o].[OrderID]) AS [Sum], MIN([o].[OrderID]) AS [Min], MAX([o].[OrderID]) AS [Max], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
");
        }

        [Fact]
        public override void GroupJoin_complex_GroupBy_Aggregate()
        {
            base.GroupJoin_complex_GroupBy_Aggregate();

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

        [Fact]
        public override void GroupJoin_GroupBy_Aggregate()
        {
            base.GroupJoin_GroupBy_Aggregate();

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

        [Fact]
        public override void GroupJoin_GroupBy_Aggregate_2()
        {
            base.GroupJoin_GroupBy_Aggregate_2();

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

        [Fact]
        public override void GroupJoin_GroupBy_Aggregate_3()
        {
            base.GroupJoin_GroupBy_Aggregate_3();

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

        [Fact]
        public override void GroupJoin_GroupBy_Aggregate_4()
        {
            base.GroupJoin_GroupBy_Aggregate_4();

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

        [Fact]
        public override void GroupJoin_GroupBy_Aggregate_5()
        {
            base.GroupJoin_GroupBy_Aggregate_5();

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

        [Fact]
        public override void Join_complex_GroupBy_Aggregate()
        {
            base.Join_complex_GroupBy_Aggregate();

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

        [Fact]
        public override void Join_GroupBy_Aggregate()
        {
            base.Join_GroupBy_Aggregate();

            AssertSql(@"
SELECT [c].[CustomerID] AS [Key], AVG(CAST([o].[OrderID] AS float)) AS [Count]
FROM [Orders] AS [o]
INNER JOIN [Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[CustomerID]
");
        }

        [Fact]
        [Trait("Impatient", "EFCore missing entries")]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Join_GroupBy_Aggregate_in_subquery()
        {
            AssertQuery<Order, Customer>(
                (os, cs) =>
                    from o in os.Where(o => o.OrderID < 10400)
                    join i in (from c in cs
                               join a in os.GroupBy(o => o.CustomerID)
                                           .Where(g => g.Count() > 5)
                                           .Select(g => new { CustomerID = g.Key, LastOrderID = g.Max(o => o.OrderID) })
                                   on c.CustomerID equals a.CustomerID
                               select new { c, a.LastOrderID })
                        on o.CustomerID equals i.c.CustomerID
                    select new { o, i.c, i.c.CustomerID },
                entryCount: 187);

            AssertSql(@"
SELECT [o].[OrderID] AS [o.OrderID], [o].[CustomerID] AS [o.CustomerID], [o].[EmployeeID] AS [o.EmployeeID], [o].[OrderDate] AS [o.OrderDate], [i].[c.CustomerID] AS [c.CustomerID], [i].[c.Address] AS [c.Address], [i].[c.City] AS [c.City], [i].[c.CompanyName] AS [c.CompanyName], [i].[c.ContactName] AS [c.ContactName], [i].[c.ContactTitle] AS [c.ContactTitle], [i].[c.Country] AS [c.Country], [i].[c.Fax] AS [c.Fax], [i].[c.Phone] AS [c.Phone], [i].[c.PostalCode] AS [c.PostalCode], [i].[c.Region] AS [c.Region], [i].[c.CustomerID] AS [CustomerID]
FROM [Orders] AS [o]
INNER JOIN (
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
) AS [i] ON [o].[CustomerID] = [i].[c.CustomerID]
WHERE [o].[OrderID] < 10400
");
        }

        [Fact]
        public override void Join_GroupBy_Aggregate_multijoins()
        {
            base.Join_GroupBy_Aggregate_multijoins();

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

        [Fact]
        public override void Join_GroupBy_Aggregate_on_key()
        {
            base.Join_GroupBy_Aggregate_on_key();

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

        [Fact]
        public override void Join_GroupBy_Aggregate_single_join()
        {
            base.Join_GroupBy_Aggregate_single_join();

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

        [Fact]
        public override void Join_GroupBy_Aggregate_with_another_join()
        {
            base.Join_GroupBy_Aggregate_with_another_join();

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

        [Fact]
        public override void Join_GroupBy_entity_ToList()
        {
            base.Join_GroupBy_entity_ToList();

            AssertSql(@"
SELECT [c].[CustomerID] AS [C.CustomerID], [c].[Address] AS [C.Address], [c].[City] AS [C.City], [c].[CompanyName] AS [C.CompanyName], [c].[ContactName] AS [C.ContactName], [c].[ContactTitle] AS [C.ContactTitle], [c].[Country] AS [C.Country], [c].[Fax] AS [C.Fax], [c].[Phone] AS [C.Phone], [c].[PostalCode] AS [C.PostalCode], [c].[Region] AS [C.Region], (
    SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
    FROM (
        SELECT TOP (5) [c_0].[CustomerID] AS [CustomerID], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Country] AS [Country], [c_0].[Fax] AS [Fax], [c_0].[Phone] AS [Phone], [c_0].[PostalCode] AS [PostalCode], [c_0].[Region] AS [Region]
        FROM [Customers] AS [c_0]
        ORDER BY [c_0].[CustomerID] ASC
    ) AS [c_1]
    INNER JOIN (
        SELECT TOP (50) [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
        FROM [Orders] AS [o_0]
        ORDER BY [o_0].[OrderID] ASC
    ) AS [o] ON [c_1].[CustomerID] = [o].[CustomerID]
    WHERE [c].[CustomerID] = [c_1].[CustomerID]
    FOR JSON PATH
) AS [Os]
FROM (
    SELECT TOP (5) [c_2].[CustomerID] AS [CustomerID], [c_2].[Address] AS [Address], [c_2].[City] AS [City], [c_2].[CompanyName] AS [CompanyName], [c_2].[ContactName] AS [ContactName], [c_2].[ContactTitle] AS [ContactTitle], [c_2].[Country] AS [Country], [c_2].[Fax] AS [Fax], [c_2].[Phone] AS [Phone], [c_2].[PostalCode] AS [PostalCode], [c_2].[Region] AS [Region]
    FROM [Customers] AS [c_2]
    ORDER BY [c_2].[CustomerID] ASC
) AS [c]
INNER JOIN (
    SELECT TOP (50) [o_1].[OrderID] AS [OrderID], [o_1].[CustomerID] AS [CustomerID], [o_1].[EmployeeID] AS [EmployeeID], [o_1].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_1]
    ORDER BY [o_1].[OrderID] ASC
) AS [o_2] ON [c].[CustomerID] = [o_2].[CustomerID]
GROUP BY [c].[CustomerID], [c].[Address], [c].[City], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Country], [c].[Fax], [c].[Phone], [c].[PostalCode], [c].[Region]
");
        }

        [Fact]
        public override void OrderBy_GroupBy_Aggregate()
        {
            base.OrderBy_GroupBy_Aggregate();

            AssertSql(@"
SELECT SUM([o].[OrderID])
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
");
        }

        [Fact]
        public override void OrderBy_GroupBy_SelectMany()
        {
            base.OrderBy_GroupBy_SelectMany();

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

        [Fact]
        public override void OrderBy_GroupBy_SelectMany_shadow()
        {
            base.OrderBy_GroupBy_SelectMany_shadow();

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

        [Fact]
        public override void OrderBy_Skip_GroupBy()
        {
            base.OrderBy_Skip_GroupBy();

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
    FROM (
        SELECT [o_1].[OrderID] AS [OrderID], [o_1].[CustomerID] AS [CustomerID], [o_1].[EmployeeID] AS [EmployeeID], [o_1].[OrderDate] AS [OrderDate]
        FROM [Orders] AS [o_1]
        ORDER BY [o_1].[OrderDate] ASC, [o_1].[OrderID] ASC
        OFFSET 800 ROWS
    ) AS [o_0]
    WHERE (([o].[CustomerID] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[CustomerID] = [o_0].[CustomerID]))
    FOR JSON PATH
) AS [Elements]
FROM (
    SELECT [o_2].[OrderID] AS [OrderID], [o_2].[CustomerID] AS [CustomerID], [o_2].[EmployeeID] AS [EmployeeID], [o_2].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_2]
    ORDER BY [o_2].[OrderDate] ASC, [o_2].[OrderID] ASC
    OFFSET 800 ROWS
) AS [o]
GROUP BY [o].[CustomerID]
");
        }

        [Fact]
        public override void OrderBy_Skip_GroupBy_Aggregate()
        {
            base.OrderBy_Skip_GroupBy_Aggregate();

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

        [Fact]
        public override void OrderBy_Skip_Take_GroupBy()
        {
            base.OrderBy_Skip_Take_GroupBy();

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
    FROM (
        SELECT [o_1].[OrderID] AS [OrderID], [o_1].[CustomerID] AS [CustomerID], [o_1].[EmployeeID] AS [EmployeeID], [o_1].[OrderDate] AS [OrderDate]
        FROM [Orders] AS [o_1]
        ORDER BY [o_1].[OrderDate] ASC
        OFFSET 450 ROWS FETCH NEXT 50 ROWS ONLY
    ) AS [o_0]
    WHERE (([o].[CustomerID] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[CustomerID] = [o_0].[CustomerID]))
    FOR JSON PATH
) AS [Elements]
FROM (
    SELECT [o_2].[OrderID] AS [OrderID], [o_2].[CustomerID] AS [CustomerID], [o_2].[EmployeeID] AS [EmployeeID], [o_2].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_2]
    ORDER BY [o_2].[OrderDate] ASC
    OFFSET 450 ROWS FETCH NEXT 50 ROWS ONLY
) AS [o]
GROUP BY [o].[CustomerID]
");
        }

        [Fact]
        public override void OrderBy_Skip_Take_GroupBy_Aggregate()
        {
            base.OrderBy_Skip_Take_GroupBy_Aggregate();

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

        [Fact]
        public override void OrderBy_Take_GroupBy()
        {
            base.OrderBy_Take_GroupBy();

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
    FROM (
        SELECT TOP (50) [o_1].[OrderID] AS [OrderID], [o_1].[CustomerID] AS [CustomerID], [o_1].[EmployeeID] AS [EmployeeID], [o_1].[OrderDate] AS [OrderDate]
        FROM [Orders] AS [o_1]
        ORDER BY [o_1].[OrderDate] ASC
    ) AS [o_0]
    WHERE (([o].[CustomerID] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[CustomerID] = [o_0].[CustomerID]))
    FOR JSON PATH
) AS [Elements]
FROM (
    SELECT TOP (50) [o_2].[OrderID] AS [OrderID], [o_2].[CustomerID] AS [CustomerID], [o_2].[EmployeeID] AS [EmployeeID], [o_2].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_2]
    ORDER BY [o_2].[OrderDate] ASC
) AS [o]
GROUP BY [o].[CustomerID]
");
        }

        [Fact]
        public override void OrderBy_Take_GroupBy_Aggregate()
        {
            base.OrderBy_Take_GroupBy_Aggregate();

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

        [Fact]
        public override void SelectMany_GroupBy_Aggregate()
        {
            base.SelectMany_GroupBy_Aggregate();

            AssertSql(@"
SELECT [o].[EmployeeID] AS [Key], COUNT(*) AS [c]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
GROUP BY [o].[EmployeeID]
");
        }

        [Fact]
        public override void Select_anonymous_GroupBy_Aggregate()
        {
            base.Select_anonymous_GroupBy_Aggregate();

            AssertSql(@"
SELECT MIN([o].[OrderDate]) AS [Min], MAX([o].[OrderDate]) AS [Max], SUM([o].[OrderID]) AS [Sum], AVG(CAST([o].[OrderID] AS float)) AS [Avg]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
GROUP BY [o].[CustomerID]
");
        }

        [Fact]
        public override void Select_Distinct_GroupBy()
        {
            base.Select_Distinct_GroupBy();

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], (
    SELECT [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID]
    FROM (
        SELECT DISTINCT [o_1].[CustomerID] AS [CustomerID], [o_1].[EmployeeID] AS [EmployeeID]
        FROM [Orders] AS [o_1]
    ) AS [o_0]
    WHERE (([o].[CustomerID] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[CustomerID] = [o_0].[CustomerID]))
    FOR JSON PATH
) AS [Elements]
FROM (
    SELECT DISTINCT [o_2].[CustomerID] AS [CustomerID], [o_2].[EmployeeID] AS [EmployeeID]
    FROM [Orders] AS [o_2]
) AS [o]
GROUP BY [o].[CustomerID]
");
        }

        [Fact]
        public override void Select_GroupBy()
        {
            base.Select_GroupBy();

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], (
    SELECT [o_0].[OrderID] AS [Order], [o_0].[CustomerID] AS [Customer]
    FROM [Orders] AS [o_0]
    WHERE (([o].[CustomerID] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[CustomerID] = [o_0].[CustomerID]))
    FOR JSON PATH
) AS [Elements]
FROM [Orders] AS [o]
GROUP BY [o].[CustomerID]
");
        }

        [Fact]
        public override void Select_GroupBy_All()
        {
            base.Select_GroupBy_All();

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

        [Fact]
        public override void Select_GroupBy_SelectMany()
        {
            base.Select_GroupBy_SelectMany();

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

        [Fact]
        public override void Select_nested_collection_with_groupby()
        {
            base.Select_nested_collection_with_groupby();

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

        [Fact]
        public override void Self_join_GroupBy_Aggregate()
        {
            base.Self_join_GroupBy_Aggregate();

            AssertSql(@"
SELECT [o].[CustomerID] AS [Key], AVG(CAST([o2].[OrderID] AS float)) AS [Count]
FROM [Orders] AS [o]
INNER JOIN [Orders] AS [o2] ON [o].[OrderID] = [o2].[OrderID]
WHERE [o].[OrderID] < 10400
GROUP BY [o].[CustomerID]
");
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Union_simple_groupby()
        {
            // Corrected entry count from 19 to 0.

            AssertQuery<Customer>(
                cs => cs.Where(s => s.ContactTitle == "Owner")
                    .Union(cs.Where(c => c.City == "México D.F."))
                    .GroupBy(c => c.City)
                    .Select(
                        g => new
                        {
                            g.Key,
                            Total = g.Count()
                        }),
                entryCount: 0);

            AssertSql(@"
SELECT [set].[City] AS [Key], COUNT(*) AS [Total]
FROM (
    SELECT [s].[CustomerID] AS [CustomerID], [s].[Address] AS [Address], [s].[City] AS [City], [s].[CompanyName] AS [CompanyName], [s].[ContactName] AS [ContactName], [s].[ContactTitle] AS [ContactTitle], [s].[Country] AS [Country], [s].[Fax] AS [Fax], [s].[Phone] AS [Phone], [s].[PostalCode] AS [PostalCode], [s].[Region] AS [Region]
    FROM [Customers] AS [s]
    WHERE [s].[ContactTitle] = N'Owner'
    UNION
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'México D.F.'
) AS [set]
GROUP BY [set].[City]
");
        }

        #region utilities

        protected override void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

        private void AssertSql(string sql) => Fixture.AssertSql(sql);

        private void AssertSqlStartsWith(string sql) => Fixture.AssertSqlStartsWith(sql);

        #endregion
    }
}
