using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class SimpleQueryImpatientTest : SimpleQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public SimpleQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            ClearLog();
        }

        #region System.Math

        [Fact]
        public override void Select_math_round_int()
        {
            base.Select_math_round_int();

            AssertSql(@"SELECT ROUND(CAST(CAST([o].[OrderID] AS float) AS float), 0) AS [A]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10250");
        }

        [Fact]
        public override void Select_math_truncate_int()
        {
            base.Select_math_truncate_int();

            AssertSql(@"SELECT ROUND(CAST(CAST([o].[OrderID] AS float) AS float), 0, 1) AS [A]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10250");
        }

        [Fact]
        public override void Where_math_abs1()
        {
            base.Where_math_abs1();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ABS([od].[ProductID]) > 10");
        }

        [Fact]
        public override void Where_math_abs2()
        {
            base.Where_math_abs2();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE CAST(ABS([od].[Quantity]) AS int) > 10");
        }

        [Fact]
        public override void Where_math_abs3()
        {
            base.Where_math_abs3();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ABS([od].[UnitPrice]) > 10.0");
        }

        [Fact]
        public override void Where_math_abs_uncorrelated()
        {
            base.Where_math_abs_uncorrelated();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE 10 < [od].[ProductID]");
        }

        [Fact]
        public override void Where_math_acos()
        {
            base.Where_math_acos();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (ACOS(CAST([od].[Discount] AS float)) > 1)");
        }

        [Fact]
        public override void Where_math_asin()
        {
            base.Where_math_asin();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (ASIN(CAST([od].[Discount] AS float)) > 0)");
        }

        [Fact]
        public override void Where_math_atan()
        {
            base.Where_math_atan();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (ATAN(CAST([od].[Discount] AS float)) > 0)");
        }

        [Fact]
        public override void Where_math_atan2()
        {
            base.Where_math_atan2();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (ATN2(CAST([od].[Discount] AS float), 1) > 0)");
        }

        [Fact]
        public override void Where_math_ceiling1()
        {
            base.Where_math_ceiling1();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE CEILING(CAST([od].[Discount] AS float)) > 0");
        }

        [Fact]
        public override void Where_math_ceiling2()
        {
            base.Where_math_ceiling2();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE CEILING([od].[UnitPrice]) > 10.0");
        }

        [Fact]
        public override void Where_math_cos()
        {
            base.Where_math_cos();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (COS(CAST([od].[Discount] AS float)) > 0)");
        }

        [Fact]
        public override void Where_math_exp()
        {
            base.Where_math_exp();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (EXP(CAST([od].[Discount] AS float)) > 1)");
        }

        [Fact]
        public override void Where_math_floor()
        {
            base.Where_math_floor();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE FLOOR([od].[UnitPrice]) > 10.0");
        }

        [Fact]
        public override void Where_math_log()
        {
            base.Where_math_log();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE (([od].[OrderID] = 11077) AND ([od].[Discount] > 0)) AND (LOG(CAST([od].[Discount] AS float)) < 0)");
        }

        [Fact]
        public override void Where_math_log10()
        {
            base.Where_math_log10();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE (([od].[OrderID] = 11077) AND ([od].[Discount] > 0)) AND (LOG10(CAST([od].[Discount] AS float)) < 0)");
        }

        [Fact]
        public override void Where_math_log_new_base()
        {
            base.Where_math_log_new_base();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE (([od].[OrderID] = 11077) AND ([od].[Discount] > 0)) AND (LOG(CAST([od].[Discount] AS float), 7) < 0)");
        }

        [Fact]
        public override void Where_math_power()
        {
            base.Where_math_power();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE POWER(CAST([od].[Discount] AS float), 2) > 0.05000000074505806");
        }

        [Fact]
        public override void Where_math_round()
        {
            base.Where_math_round();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ROUND([od].[UnitPrice], 0) > 10.0");
        }

        [Fact]
        public override void Where_math_round2()
        {
            base.Where_math_round2();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ROUND([od].[UnitPrice], 2) > 100.0");
        }

        [Fact]
        public override void Where_math_sign()
        {
            base.Where_math_sign();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (SIGN([od].[Discount]) > 0)");
        }

        [Fact]
        public override void Where_math_sin()
        {
            base.Where_math_sin();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (SIN(CAST([od].[Discount] AS float)) > 0)");
        }

        [Fact]
        public override void Where_math_sqrt()
        {
            base.Where_math_sqrt();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (SQRT(CAST([od].[Discount] AS float)) > 0)");
        }

        [Fact]
        public override void Where_math_tan()
        {
            base.Where_math_tan();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ([od].[OrderID] = 11077) AND (TAN(CAST([od].[Discount] AS float)) > 0)");
        }

        [Fact]
        public override void Where_math_truncate()
        {
            base.Where_math_truncate();

            AssertSql(@"SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE ROUND([od].[UnitPrice], 0, 1) > 10.0");
        }

        #endregion

        #region Queryable.All

        // These next three tests could make use of predicate splitting
        // to limit what is pulled from the server for client evaluation
        // but that is low priority.

        [Fact]
        [Trait("Impatient", "Improve SQL")]
        [Trait("Impatient", "Low priority")]
        public override void All_client()
        {
            base.All_client();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
");
        }

        [Fact]
        [Trait("Impatient", "Improve SQL")]
        [Trait("Impatient", "Low priority")]
        public override void All_client_and_server_top_level()
        {
            base.All_client_and_server_top_level();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
");
        }

        [Fact]
        [Trait("Impatient", "Improve SQL")]
        [Trait("Impatient", "Low priority")]
        public override void All_client_or_server_top_level()
        {
            base.All_client_or_server_top_level();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
");
        }

        [Fact]
        public override void All_top_level()
        {
            base.All_top_level();

            AssertSql(@"
SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    WHERE LEFT([c].[ContactName], LEN(N'A')) <> N'A'
) THEN 0 ELSE 1 END) AS bit)
");
        }

        [Fact]
        public override void All_top_level_column()
        {
            base.All_top_level_column();

            AssertSql(@"
SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    WHERE (LEN([c].[ContactName]) <> 0) AND (([c].[ContactName] IS NULL OR (LEFT([c].[ContactName], LEN([c].[ContactName])) <> [c].[ContactName])))
) THEN 0 ELSE 1 END) AS bit)
");
        }

        [Fact]
        public override void All_top_level_subquery()
        {
            base.All_top_level_subquery();

            AssertSql(@"
SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM [Customers] AS [c1]
    WHERE (
        SELECT CAST((CASE WHEN EXISTS (
            SELECT 1
            FROM [Customers] AS [c2]
            WHERE (
                SELECT CAST((CASE WHEN EXISTS (
                    SELECT 1
                    FROM [Customers] AS [c3]
                    WHERE [c1].[CustomerID] = [c3].[CustomerID]
                ) THEN 1 ELSE 0 END) AS bit)
            ) = 1
        ) THEN 1 ELSE 0 END) AS bit)
    ) = 0
) THEN 0 ELSE 1 END) AS bit)
");
        }

        [Fact]
        public override void All_top_level_subquery_ef_property()
        {
            base.All_top_level_subquery_ef_property();

            AssertSql(@"
SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM [Customers] AS [c1]
    WHERE (
        SELECT CAST((CASE WHEN EXISTS (
            SELECT 1
            FROM [Customers] AS [c2]
            WHERE (
                SELECT CAST((CASE WHEN EXISTS (
                    SELECT 1
                    FROM [Customers] AS [c3]
                    WHERE [c1].[CustomerID] = [c3].[CustomerID]
                ) THEN 1 ELSE 0 END) AS bit)
            ) = 1
        ) THEN 1 ELSE 0 END) AS bit)
    ) = 0
) THEN 0 ELSE 1 END) AS bit)
");
        }

        [Fact]
        public override void Select_All()
        {
            base.Select_All();

            AssertSql(@"
SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM [Orders] AS [o]
    WHERE ([o].[CustomerID] IS NULL OR ([o].[CustomerID] <> N'ALFKI'))
) THEN 0 ELSE 1 END) AS bit)
");
        }

        [Fact]
        public override void Skip_Take_All()
        {
            base.Skip_Take_All();

            AssertSql(@"
SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM (
        SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID] ASC
        OFFSET 4 ROWS FETCH NEXT 7 ROWS ONLY
    ) AS [p]
    WHERE LEFT([p].[CustomerID], LEN(N'B')) <> N'B'
) THEN 0 ELSE 1 END) AS bit)
");
        }

        [Fact]
        public override void Take_All()
        {
            base.Take_All();

            AssertSql(@"
SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM (
        SELECT TOP (4) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID] ASC
    ) AS [p]
    WHERE LEFT([p].[CustomerID], LEN(N'A')) <> N'A'
) THEN 0 ELSE 1 END) AS bit)
");
        }

        // IDK, these next four tests are kinda dumb.
        // Anyone typing ids.All(li => li != c.CustomerID) should just be
        // typing !ids.Contains(c.CustomerID) instead. This would be an easy
        // thing to implement but not high priority at all.

        [Fact]
        [Trait("Impatient", "Improve SQL")]
        [Trait("Impatient", "Low priority")]
        public override void Where_subquery_all_not_equals()
        {
            base.Where_subquery_all_not_equals();
            
            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
");
        }

        [Fact]
        [Trait("Impatient", "Improve SQL")]
        [Trait("Impatient", "Low priority")]
        public override void Where_subquery_all_not_equals_operator()
        {
            base.Where_subquery_all_not_equals_operator();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
");
        }

        [Fact]
        [Trait("Impatient", "Improve SQL")]
        [Trait("Impatient", "Low priority")]
        public override void Where_subquery_all_not_equals_static()
        {
            base.Where_subquery_all_not_equals_static();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
");
        }

        [Fact]
        [Trait("Impatient", "Improve SQL")]
        [Trait("Impatient", "Low priority")]
        public override void Where_subquery_where_all()
        {
            base.Where_subquery_where_all();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'México D.F.'
");
        }

        #endregion

        #region Queryable.Any

        [Fact]
        public override void Any_nested()
        {
            base.Any_nested();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE (
    SELECT CAST((CASE WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE LEFT([o].[CustomerID], LEN(N'A')) = N'A'
    ) THEN 1 ELSE 0 END) AS bit)
) = 1
");
        }

        [Fact]
        public override void Any_nested2()
        {
            base.Any_nested2();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE (([c].[City] IS NULL OR ([c].[City] <> N'London'))) AND ((
    SELECT CAST((CASE WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE LEFT([o].[CustomerID], LEN(N'A')) = N'A'
    ) THEN 1 ELSE 0 END) AS bit)
) = 1)
");
        }

        [Fact]
        public override void Any_nested3()
        {
            base.Any_nested3();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE ((
    SELECT CAST((CASE WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE LEFT([o].[CustomerID], LEN(N'A')) = N'A'
    ) THEN 1 ELSE 0 END) AS bit)
) = 1) AND (([c].[City] IS NULL OR ([c].[City] <> N'London')))
");
        }

        [Fact]
        public override void Any_nested_negated()
        {
            base.Any_nested_negated();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE (
    SELECT CAST((CASE WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE LEFT([o].[CustomerID], LEN(N'A')) = N'A'
    ) THEN 1 ELSE 0 END) AS bit)
) = 0
");
        }

        [Fact]
        public override void Any_nested_negated2()
        {
            base.Any_nested_negated2();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE (([c].[City] IS NULL OR ([c].[City] <> N'London'))) AND ((
    SELECT CAST((CASE WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE LEFT([o].[CustomerID], LEN(N'A')) = N'A'
    ) THEN 1 ELSE 0 END) AS bit)
) = 0)
");
        }

        [Fact]
        public override void Any_nested_negated3()
        {
            base.Any_nested_negated3();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE ((
    SELECT CAST((CASE WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE LEFT([o].[CustomerID], LEN(N'A')) = N'A'
    ) THEN 1 ELSE 0 END) AS bit)
) = 0) AND (([c].[City] IS NULL OR ([c].[City] <> N'London')))
");
        }

        [Fact]
        public override void Any_predicate()
        {
            base.Any_predicate();

            AssertSql(@"
SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    WHERE LEFT([c].[ContactName], LEN(N'A')) = N'A'
) THEN 1 ELSE 0 END) AS bit)
");
        }

        [Fact]
        public override void Any_simple()
        {
            base.Any_simple();

            AssertSql(@"
SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
) THEN 1 ELSE 0 END) AS bit)
");
        }

        [Fact]
        public override void Any_with_multiple_conditions_still_uses_exists()
        {
            base.Any_with_multiple_conditions_still_uses_exists();

            AssertSql(@"
@p0='1'

SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE ([c].[City] = N'London') AND ((
    SELECT CAST((CASE WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE ([c].[CustomerID] = [o].[CustomerID]) AND ([o].[EmployeeID] = CAST(@p0 AS bigint))
    ) THEN 1 ELSE 0 END) AS bit)
) = 1)
");
        }

        [Fact]
        public override void Let_any_subquery_anonymous()
        {
            base.Let_any_subquery_anonymous();

            AssertSql(@"
SELECT [c].[CustomerID] AS [c.CustomerID], [c].[Address] AS [c.Address], [c].[City] AS [c.City], [c].[CompanyName] AS [c.CompanyName], [c].[ContactName] AS [c.ContactName], [c].[ContactTitle] AS [c.ContactTitle], [c].[Country] AS [c.Country], [c].[Fax] AS [c.Fax], [c].[Phone] AS [c.Phone], [c].[PostalCode] AS [c.PostalCode], [c].[Region] AS [c.Region], CAST((
    SELECT CAST((CASE WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE [o].[CustomerID] = [c].[CustomerID]
    ) THEN 1 ELSE 0 END) AS bit)
) AS bit) AS [hasOrders]
FROM [Customers] AS [c]
WHERE LEFT([c].[CustomerID], LEN(N'A')) = N'A'
ORDER BY [c].[CustomerID] ASC
");
        }

        [Fact]
        public override void Multiple_joins_Where_Order_Any()
        {
            base.Multiple_joins_Where_Order_Any();

            AssertSql(@"
SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
    INNER JOIN [Order Details] AS [od] ON [o].[OrderID] = [od].[OrderID]
    WHERE [c].[City] = N'London'
) THEN 1 ELSE 0 END) AS bit)
");
        }

        [Fact]
        public override void OrderBy_any()
        {
            base.OrderBy_any();

            AssertSql(@"
SELECT [p].[CustomerID] AS [CustomerID], [p].[Address] AS [Address], [p].[City] AS [City], [p].[CompanyName] AS [CompanyName], [p].[ContactName] AS [ContactName], [p].[ContactTitle] AS [ContactTitle], [p].[Country] AS [Country], [p].[Fax] AS [Fax], [p].[Phone] AS [Phone], [p].[PostalCode] AS [PostalCode], [p].[Region] AS [Region]
FROM [Customers] AS [p]
ORDER BY (
    SELECT CAST((CASE WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE ([p].[CustomerID] = [o].[CustomerID]) AND ([o].[OrderID] > 11000)
    ) THEN 1 ELSE 0 END) AS bit)
) ASC, [p].[CustomerID] ASC
");
        }

        [Fact]
        public override void OrderBy_ThenBy_Any()
        {
            base.OrderBy_ThenBy_Any();

            AssertSql(@"
SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
) THEN 1 ELSE 0 END) AS bit)
");
        }

        [Fact]
        public override void SelectMany_OrderBy_ThenBy_Any()
        {
            base.SelectMany_OrderBy_ThenBy_Any();

            AssertSql(@"
SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    CROSS JOIN [Orders] AS [o]
) THEN 1 ELSE 0 END) AS bit)
");
        }

        [Fact]
        public override void Skip_Take_Any()
        {
            base.Skip_Take_Any();

            AssertSql(@"
SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName] ASC
    OFFSET 5 ROWS FETCH NEXT 10 ROWS ONLY
) THEN 1 ELSE 0 END) AS bit)
");
        }

        [Fact]
        public override void Skip_Take_Any_with_predicate()
        {
            base.Skip_Take_Any_with_predicate();

            AssertSql(@"
SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM (
        SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID] ASC
        OFFSET 5 ROWS FETCH NEXT 7 ROWS ONLY
    ) AS [p]
    WHERE LEFT([p].[CustomerID], LEN(N'C')) = N'C'
) THEN 1 ELSE 0 END) AS bit)
");
        }

        [Fact]
        public override void Take_Any_with_predicate()
        {
            base.Take_Any_with_predicate();

            AssertSql(@"
SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM (
        SELECT TOP (5) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[CustomerID] ASC
    ) AS [p]
    WHERE LEFT([p].[CustomerID], LEN(N'B')) = N'B'
) THEN 1 ELSE 0 END) AS bit)
");
        }

        [Fact]
        public override void Where_Join_Any()
        {
            base.Where_Join_Any();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE ([c].[CustomerID] = N'ALFKI') AND ((
    SELECT CAST((CASE WHEN EXISTS (
        SELECT 1
        FROM [Orders] AS [o]
        WHERE ([c].[CustomerID] = [o].[CustomerID]) AND ([o].[OrderDate] = '2008-10-24T00:00:00.000')
    ) THEN 1 ELSE 0 END) AS bit)
) = 1)
");
        }

        [Fact]
        [Trait("Impatient", "Improve SQL")]
        [Trait("Impatient", "Low priority")]
        public override void Where_subquery_any_equals()
        {
            base.Where_subquery_any_equals();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
");
        }

        [Fact]
        [Trait("Impatient", "Improve SQL")]
        [Trait("Impatient", "Low priority")]
        public override void Where_subquery_any_equals_operator()
        {
            base.Where_subquery_any_equals_operator();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
");
        }

        [Fact]
        [Trait("Impatient", "Improve SQL")]
        [Trait("Impatient", "Low priority")]
        public override void Where_subquery_any_equals_static()
        {
            base.Where_subquery_any_equals_static();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
");
        }

        [Fact]
        [Trait("Impatient", "Improve SQL")]
        [Trait("Impatient", "Low priority")]
        public override void Where_subquery_where_any()
        {
            base.Where_subquery_where_any();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE [c].[City] = N'México D.F.'
");
        }

        #endregion

        #region Queryable.Contains

        [Fact]
        public override void Contains_over_entityType_should_materialize_when_composite()
        {
            base.Contains_over_entityType_should_materialize_when_composite();

            AssertSql(@"
SELECT TOP (1) [o].[OrderID] AS [OrderID], [o].[ProductID] AS [ProductID], [o].[Discount] AS [Discount], [o].[Quantity] AS [Quantity], [o].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [o]
WHERE ([o].[OrderID] = 10248) AND ([o].[ProductID] = 42)

SELECT [o].[OrderID] AS [OrderID], [o].[ProductID] AS [ProductID], [o].[Discount] AS [Discount], [o].[Quantity] AS [Quantity], [o].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [o]
WHERE [o].[ProductID] = 42
");
        }

        [Fact]
        public override void Contains_over_entityType_should_rewrite_to_identity_equality()
        {
            base.Contains_over_entityType_should_rewrite_to_identity_equality();

            AssertSql(@"
SELECT TOP (2) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] = 10248

@p0='10248'

SELECT CAST((CASE WHEN @p0 IN (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    WHERE [o].[CustomerID] = N'VINET'
) THEN 1 ELSE 0 END) AS bit)
");
        }

        [Fact]
        public override void Contains_top_level()
        {
            base.Contains_top_level();

            AssertSql(@"
SELECT CAST((CASE WHEN N'ALFKI' IN (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
) THEN 1 ELSE 0 END) AS bit)
");
        }

        [Fact]
        public override void Contains_with_DateTime_Date()
        {
            base.Contains_with_DateTime_Date();

            AssertSql(@"
@p0_0='1996-07-04T00:00:00'
@p0_1='1996-07-16T00:00:00'

SELECT [e].[OrderID] AS [OrderID], [e].[CustomerID] AS [CustomerID], [e].[EmployeeID] AS [EmployeeID], [e].[OrderDate] AS [OrderDate]
FROM [Orders] AS [e]
WHERE CAST([e].[OrderDate] AS date) IN (@p0_0, @p0_1)

@p0_0='1996-07-04T00:00:00'

SELECT [e].[OrderID] AS [OrderID], [e].[CustomerID] AS [CustomerID], [e].[EmployeeID] AS [EmployeeID], [e].[OrderDate] AS [OrderDate]
FROM [Orders] AS [e]
WHERE CAST([e].[OrderDate] AS date) IN (@p0_0)
");
        }

        [Fact]
        public override void Contains_with_local_anonymous_type_array_closure()
        {
            base.Contains_with_local_anonymous_type_array_closure();

            AssertSql(@"
SELECT [o].[OrderID] AS [OrderID], [o].[ProductID] AS [ProductID], [o].[Discount] AS [Discount], [o].[Quantity] AS [Quantity], [o].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [o]

SELECT [o].[OrderID] AS [OrderID], [o].[ProductID] AS [ProductID], [o].[Discount] AS [Discount], [o].[Quantity] AS [Quantity], [o].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [o]
");
        }

        [Fact]
        public override void Contains_with_local_array_closure()
        {
            base.Contains_with_local_array_closure();

            AssertSql(@"
@p0_0='ABCDE' (Nullable = false) (Size = 5)
@p0_1='ALFKI' (Nullable = false) (Size = 5)

SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (@p0_0, @p0_1)

@p0_0='ABCDE' (Nullable = false) (Size = 5)

SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (@p0_0)
");
        }

        [Fact]
        public override void Contains_with_local_array_inline()
        {
            base.Contains_with_local_array_inline();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')
");
        }

        [Fact]
        public override void Contains_with_local_collection_complex_predicate_and()
        {
            base.Contains_with_local_collection_complex_predicate_and();

            AssertSql(@"
@p0_0='ABCDE' (Nullable = false) (Size = 5)
@p0_1='ALFKI' (Nullable = false) (Size = 5)

SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE (([c].[CustomerID] = N'ALFKI') OR ([c].[CustomerID] = N'ABCDE')) AND [c].[CustomerID] IN (@p0_0, @p0_1)
");
        }

        [Fact]
        public override void Contains_with_local_collection_complex_predicate_not_matching_ins1()
        {
            base.Contains_with_local_collection_complex_predicate_not_matching_ins1();

            AssertSql(@"
@p0_0='ABCDE' (Nullable = false) (Size = 5)
@p0_1='ALFKI' (Nullable = false) (Size = 5)

SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE (([c].[CustomerID] = N'ALFKI') OR ([c].[CustomerID] = N'ABCDE')) OR NOT [c].[CustomerID] IN (@p0_0, @p0_1)
");
        }

        [Fact]
        public override void Contains_with_local_collection_complex_predicate_not_matching_ins2()
        {
            base.Contains_with_local_collection_complex_predicate_not_matching_ins2();

            AssertSql(@"
@p0_0='ABCDE' (Nullable = false) (Size = 5)
@p0_1='ALFKI' (Nullable = false) (Size = 5)

SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (@p0_0, @p0_1) AND (([c].[CustomerID] <> N'ALFKI') AND ([c].[CustomerID] <> N'ABCDE'))
");
        }

        [Fact]
        public override void Contains_with_local_collection_complex_predicate_or()
        {
            base.Contains_with_local_collection_complex_predicate_or();

            AssertSql(@"
@p0_0='ABCDE' (Nullable = false) (Size = 5)
@p0_1='ALFKI' (Nullable = false) (Size = 5)

SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (@p0_0, @p0_1) OR (([c].[CustomerID] = N'ALFKI') OR ([c].[CustomerID] = N'ABCDE'))
");
        }

        [Fact]
        public override void Contains_with_local_collection_empty_closure()
        {
            base.Contains_with_local_collection_empty_closure();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (NULL)
");
        }

        [Fact]
        public override void Contains_with_local_collection_empty_inline()
        {
            base.Contains_with_local_collection_empty_inline();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE NOT [c].[CustomerID] IN (SELECT 1 WHERE 1 = 0)
");
        }

        [Fact]
        public override void Contains_with_local_collection_false()
        {
            base.Contains_with_local_collection_false();

            AssertSql(@"
@p0_0='ABCDE' (Nullable = false) (Size = 5)
@p0_1='ALFKI' (Nullable = false) (Size = 5)

SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE NOT [c].[CustomerID] IN (@p0_0, @p0_1)
");
        }

        [Fact]
        public override void Contains_with_local_collection_sql_injection()
        {
            base.Contains_with_local_collection_sql_injection();

            AssertSql(@"
@p0_0='ALFKI' (Nullable = false) (Size = 5)
@p0_1='ABC')); GO; DROP TABLE Orders; GO; --' (Nullable = false) (Size = 37)

SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (@p0_0, @p0_1) OR (([c].[CustomerID] = N'ALFKI') OR ([c].[CustomerID] = N'ABCDE'))
");
        }

        [Fact]
        public override void Contains_with_local_int_array_closure()
        {
            base.Contains_with_local_int_array_closure();

            AssertSql(@"
@p0_0='0'
@p0_1='1'

SELECT [e].[EmployeeID] AS [EmployeeID], [e].[City] AS [City], [e].[Country] AS [Country], [e].[FirstName] AS [FirstName], [e].[ReportsTo] AS [ReportsTo], [e].[Title] AS [Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (@p0_0, @p0_1)

@p0_0='0'

SELECT [e].[EmployeeID] AS [EmployeeID], [e].[City] AS [City], [e].[Country] AS [Country], [e].[FirstName] AS [FirstName], [e].[ReportsTo] AS [ReportsTo], [e].[Title] AS [Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (@p0_0)
");
        }

        [Fact]
        public override void Contains_with_local_list_closure()
        {
            base.Contains_with_local_list_closure();

            AssertSql(@"
@p0_0='ABCDE' (Nullable = false) (Size = 5)
@p0_1='ALFKI' (Nullable = false) (Size = 5)

SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (@p0_0, @p0_1)
");
        }

        [Fact]
        public override void Contains_with_local_list_inline()
        {
            base.Contains_with_local_list_inline();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', N'ALFKI')
");
        }

        [Fact]
        public override void Contains_with_local_list_inline_closure_mix()
        {
            base.Contains_with_local_list_inline_closure_mix();

            AssertSql(@"
@p0='ALFKI' (Nullable = false) (Size = 5)

SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', @p0)

@p0='ANATR' (Nullable = false) (Size = 5)

SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (N'ABCDE', @p0)
");
        }

        [Fact]
        public override void Contains_with_local_nullable_int_array_closure()
        {
            base.Contains_with_local_nullable_int_array_closure();

            AssertSql(@"
@p0_0='0'
@p0_1='1'

SELECT [e].[EmployeeID] AS [EmployeeID], [e].[City] AS [City], [e].[Country] AS [Country], [e].[FirstName] AS [FirstName], [e].[ReportsTo] AS [ReportsTo], [e].[Title] AS [Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (@p0_0, @p0_1)

@p0_0='0'

SELECT [e].[EmployeeID] AS [EmployeeID], [e].[City] AS [City], [e].[Country] AS [Country], [e].[FirstName] AS [FirstName], [e].[ReportsTo] AS [ReportsTo], [e].[Title] AS [Title]
FROM [Employees] AS [e]
WHERE [e].[EmployeeID] IN (@p0_0)
");
        }

        [Fact]
        public override void Contains_with_local_tuple_array_closure()
        {
            base.Contains_with_local_tuple_array_closure();

            AssertSql(@"
SELECT [o].[OrderID] AS [OrderID], [o].[ProductID] AS [ProductID], [o].[Discount] AS [Discount], [o].[Quantity] AS [Quantity], [o].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [o]

SELECT [o].[OrderID] AS [OrderID], [o].[ProductID] AS [ProductID], [o].[Discount] AS [Discount], [o].[Quantity] AS [Quantity], [o].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [o]
");
        }

        [Fact]
        public override void Contains_with_subquery()
        {
            base.Contains_with_subquery();

            Fixture.AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] IN (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
)
");
        }

        [Fact]
        public override void Contains_with_subquery_and_local_array_closure()
        {
            base.Contains_with_subquery_and_local_array_closure();

            AssertSql(@"
@p0_0='London' (Nullable = false) (Size = 6)
@p0_1='Buenos Aires' (Nullable = false) (Size = 12)

SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE (
    SELECT CAST((CASE WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c1]
        WHERE [c1].[City] IN (@p0_0, @p0_1) AND ([c1].[CustomerID] = [c].[CustomerID])
    ) THEN 1 ELSE 0 END) AS bit)
) = 1

@p0_0='London' (Nullable = false) (Size = 6)

SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
WHERE (
    SELECT CAST((CASE WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c1]
        WHERE [c1].[City] IN (@p0_0) AND ([c1].[CustomerID] = [c].[CustomerID])
    ) THEN 1 ELSE 0 END) AS bit)
) = 1
");
        }

        [Fact]
        public override void Contains_with_subquery_involving_join_binds_to_correct_table()
        {
            base.Contains_with_subquery_involving_join_binds_to_correct_table();

            Fixture.AssertSql(@"
SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
FROM [Orders] AS [o]
WHERE ([o].[OrderID] > 11000) AND [o].[OrderID] IN (
    SELECT [od].[OrderID]
    FROM [Order Details] AS [od]
    INNER JOIN [Products] AS [p] ON [od].[ProductID] = [p].[ProductID]
    WHERE [p].[ProductName] = N'Chai'
)
");
        }

        [Fact]
        public override void OrderBy_empty_list_contains()
        {
            base.OrderBy_empty_list_contains();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
ORDER BY (
    SELECT CAST((CASE WHEN [c].[CustomerID] IN (NULL) THEN 1 ELSE 0 END) AS bit)
) ASC
");
        }

        [Fact]
        public override void OrderBy_empty_list_does_not_contains()
        {
            base.OrderBy_empty_list_does_not_contains();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
ORDER BY (
    SELECT CAST((CASE WHEN [c].[CustomerID] IN (NULL) THEN 0 ELSE 1 END) AS bit)
) ASC
");
        }

        [Fact]
        public override void Where_contains_on_navigation()
        {
            base.Where_contains_on_navigation();

            AssertSqlStartsWith(@"
SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
FROM [Orders] AS [o]
WHERE (
    SELECT CAST((CASE WHEN EXISTS (
        SELECT 1
        FROM [Customers] AS [c]
        WHERE [o].[OrderID] IN (
            SELECT [o_0].[OrderID]
            FROM [Orders] AS [o_0]
            WHERE [c].[CustomerID] = [o_0].[CustomerID]
        )
    ) THEN 1 ELSE 0 END) AS bit)
) = 1
");
        }

        [Fact]
        public override void Where_multiple_contains_in_subquery_with_and()
        {
            base.Where_multiple_contains_in_subquery_with_and();

            Fixture.AssertSql(@"
SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE [od].[ProductID] IN (
    SELECT TOP (20) [p].[ProductID]
    FROM [Products] AS [p]
    ORDER BY [p].[ProductID] ASC
) AND [od].[OrderID] IN (
    SELECT TOP (10) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID] ASC
)
");
        }

        [Fact]
        public override void Where_multiple_contains_in_subquery_with_or()
        {
            base.Where_multiple_contains_in_subquery_with_or();

            Fixture.AssertSql(@"
SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
WHERE [od].[ProductID] IN (
    SELECT TOP (1) [p].[ProductID]
    FROM [Products] AS [p]
    ORDER BY [p].[ProductID] ASC
) OR [od].[OrderID] IN (
    SELECT TOP (1) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID] ASC
)
");
        }

        [Fact]
        [Trait("Impatient", "Improve SQL")]
        [Trait("Impatient", "Medium priority")]
        public override void Where_navigation_contains()
        {
            base.Where_navigation_contains();

            // We want the WHERE clause in the second query to be this instead:
            // WHERE [o].[CustomerID] = @p0

            AssertSql(@"
SELECT TOP (2) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region], (
    SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    FOR JSON PATH
) AS [Orders]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'

@p0_0='10643'
@p0_1='10692'
@p0_2='10702'
@p0_3='10835'
@p0_4='10952'
@p0_5='11011'

SELECT [od].[OrderID] AS [OrderID], [od].[ProductID] AS [ProductID], [od].[Discount] AS [Discount], [od].[Quantity] AS [Quantity], [od].[UnitPrice] AS [UnitPrice]
FROM [Order Details] AS [od]
INNER JOIN [Orders] AS [o] ON [od].[OrderID] = [o].[OrderID]
WHERE [o].[OrderID] IN (@p0_0, @p0_1, @p0_2, @p0_3, @p0_4, @p0_5)
");
        }

        #endregion

        #region Queryable.Distinct

        [Fact]
        public override void Anonymous_complex_distinct_orderby()
        {
            base.Anonymous_complex_distinct_orderby();

            AssertSql(@"
SELECT [n].[A] AS [A]
FROM (
    SELECT DISTINCT [c].[CustomerID] + [c].[City] AS [A]
    FROM [Customers] AS [c]
) AS [n]
ORDER BY [n].[A] ASC
");
        }

        [Fact]
        public override void Anonymous_complex_distinct_result()
        {
            base.Anonymous_complex_distinct_result();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID] + [c].[City] AS [A]
    FROM [Customers] AS [c]
) AS [n]
WHERE LEFT([n].[A], LEN(N'A')) = N'A'
");
        }

        [Fact]
        public override void Anonymous_complex_distinct_where()
        {
            base.Anonymous_complex_distinct_where();

            AssertSql(@"
SELECT [n].[A] AS [A]
FROM (
    SELECT DISTINCT [c].[CustomerID] + [c].[City] AS [A]
    FROM [Customers] AS [c]
) AS [n]
WHERE [n].[A] = N'ALFKIBerlin'
");
        }

        [Fact]
        public override void Anonymous_member_distinct_orderby()
        {
            base.Anonymous_member_distinct_orderby();

            AssertSql(@"
SELECT [n].[CustomerID] AS [CustomerID]
FROM (
    SELECT DISTINCT [c].[CustomerID] AS [CustomerID]
    FROM [Customers] AS [c]
) AS [n]
ORDER BY [n].[CustomerID] ASC
");
        }

        [Fact]
        public override void Anonymous_member_distinct_result()
        {
            base.Anonymous_member_distinct_result();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID] AS [CustomerID]
    FROM [Customers] AS [c]
) AS [n]
WHERE LEFT([n].[CustomerID], LEN(N'A')) = N'A'
");
        }

        [Fact]
        public override void Anonymous_member_distinct_where()
        {
            base.Anonymous_member_distinct_where();

            AssertSql(@"
SELECT [n].[CustomerID] AS [CustomerID]
FROM (
    SELECT DISTINCT [c].[CustomerID] AS [CustomerID]
    FROM [Customers] AS [c]
) AS [n]
WHERE [n].[CustomerID] = N'ALFKI'
");
        }

        [Fact]
        public override void Distinct()
        {
            base.Distinct();

            AssertSql(@"
SELECT DISTINCT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
");
        }

        [Fact]
        public override void Distinct_Count()
        {
            base.Distinct_Count();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
) AS [t]
");
        }

        [Fact]
        public override void Distinct_OrderBy()
        {
            base.Distinct_OrderBy();

            AssertSql(@"
SELECT [c].[Country]
FROM (
    SELECT DISTINCT [c_0].[Country]
    FROM [Customers] AS [c_0]
) AS [c]
ORDER BY [c].[Country] ASC
");
        }

        [Fact]
        public override void Distinct_OrderBy2()
        {
            base.Distinct_OrderBy2();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM (
    SELECT DISTINCT [c_0].[CustomerID] AS [CustomerID], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Country] AS [Country], [c_0].[Fax] AS [Fax], [c_0].[Phone] AS [Phone], [c_0].[PostalCode] AS [PostalCode], [c_0].[Region] AS [Region]
    FROM [Customers] AS [c_0]
) AS [c]
ORDER BY [c].[CustomerID] ASC
");
        }

        [Fact]
        public override void Distinct_OrderBy3()
        {
            base.Distinct_OrderBy3();

            AssertSql(@"
SELECT [a].[CustomerID] AS [CustomerID]
FROM (
    SELECT DISTINCT [c].[CustomerID] AS [CustomerID]
    FROM [Customers] AS [c]
) AS [a]
ORDER BY [a].[CustomerID] ASC
");
        }

        [Fact]
        public override void Distinct_Scalar()
        {
            base.Distinct_Scalar();

            AssertSql(@"
SELECT DISTINCT [c].[City]
FROM [Customers] AS [c]
");
        }

        [Fact]
        public override void Distinct_Skip()
        {
            base.Distinct_Skip();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM (
    SELECT DISTINCT [c_0].[CustomerID] AS [CustomerID], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Country] AS [Country], [c_0].[Fax] AS [Fax], [c_0].[Phone] AS [Phone], [c_0].[PostalCode] AS [PostalCode], [c_0].[Region] AS [Region]
    FROM [Customers] AS [c_0]
) AS [c]
ORDER BY [c].[CustomerID] ASC
OFFSET 5 ROWS
");
        }

        [Fact]
        public override void Distinct_Skip_Take()
        {
            base.Distinct_Skip_Take();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM (
    SELECT DISTINCT [c_0].[CustomerID] AS [CustomerID], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Country] AS [Country], [c_0].[Fax] AS [Fax], [c_0].[Phone] AS [Phone], [c_0].[PostalCode] AS [PostalCode], [c_0].[Region] AS [Region]
    FROM [Customers] AS [c_0]
) AS [c]
ORDER BY [c].[ContactName] ASC
OFFSET 5 ROWS FETCH NEXT 10 ROWS ONLY
");
        }

        [Fact]
        public override void Distinct_Take()
        {
            base.Distinct_Take();

            AssertSql(@"
SELECT TOP (5) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
FROM (
    SELECT DISTINCT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_0]
) AS [o]
ORDER BY [o].[OrderID] ASC
");
        }

        [Fact]
        public override void Distinct_Take_Count()
        {
            base.Distinct_Take_Count();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT TOP (5) [t].[OrderID] AS [OrderID], [t].[CustomerID] AS [CustomerID], [t].[EmployeeID] AS [EmployeeID], [t].[OrderDate] AS [OrderDate]
    FROM (
        SELECT DISTINCT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
        FROM [Orders] AS [o]
    ) AS [t]
) AS [t_0]
");
        }

        [Fact]
        public override void DTO_complex_distinct_orderby()
        {
            base.DTO_complex_distinct_orderby();

            AssertSql(@"
SELECT [n].[Property] AS [Property]
FROM (
    SELECT DISTINCT [c].[CustomerID] + [c].[City] AS [Property]
    FROM [Customers] AS [c]
) AS [n]
ORDER BY [n].[Property] ASC
");
        }

        [Fact]
        public override void DTO_complex_distinct_result()
        {
            base.DTO_complex_distinct_result();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID] + [c].[City] AS [Property]
    FROM [Customers] AS [c]
) AS [n]
WHERE LEFT([n].[Property], LEN(N'A')) = N'A'
");
        }

        [Fact]
        public override void DTO_complex_distinct_where()
        {
            base.DTO_complex_distinct_where();

            AssertSql(@"
SELECT [n].[Property] AS [Property]
FROM (
    SELECT DISTINCT [c].[CustomerID] + [c].[City] AS [Property]
    FROM [Customers] AS [c]
) AS [n]
WHERE [n].[Property] = N'ALFKIBerlin'
");
        }

        [Fact]
        public override void DTO_member_distinct_orderby()
        {
            base.DTO_member_distinct_orderby();

            AssertSql(@"
SELECT [n].[Property] AS [Property]
FROM (
    SELECT DISTINCT [c].[CustomerID] AS [Property]
    FROM [Customers] AS [c]
) AS [n]
ORDER BY [n].[Property] ASC
");
        }

        [Fact]
        public override void DTO_member_distinct_result()
        {
            base.DTO_member_distinct_result();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID] AS [Property]
    FROM [Customers] AS [c]
) AS [n]
WHERE LEFT([n].[Property], LEN(N'A')) = N'A'
");
        }

        [Fact]
        public override void DTO_member_distinct_where()
        {
            base.DTO_member_distinct_where();

            AssertSql(@"
SELECT [n].[Property] AS [Property]
FROM (
    SELECT DISTINCT [c].[CustomerID] AS [Property]
    FROM [Customers] AS [c]
) AS [n]
WHERE [n].[Property] = N'ALFKI'
");
        }

        [Fact]
        public override void OrderBy_coalesce_skip_take_distinct()
        {
            base.OrderBy_coalesce_skip_take_distinct();

            AssertSql(@"
SELECT DISTINCT [t].[ProductID] AS [ProductID], [t].[Discontinued] AS [Discontinued], [t].[ProductName] AS [ProductName], [t].[SupplierID] AS [SupplierID], [t].[UnitPrice] AS [UnitPrice], [t].[UnitsInStock] AS [UnitsInStock]
FROM (
    SELECT [p].[ProductID] AS [ProductID], [p].[Discontinued] AS [Discontinued], [p].[ProductName] AS [ProductName], [p].[SupplierID] AS [SupplierID], [p].[UnitPrice] AS [UnitPrice], [p].[UnitsInStock] AS [UnitsInStock]
    FROM [Products] AS [p]
    ORDER BY COALESCE([p].[UnitPrice], 0.0) ASC
    OFFSET 5 ROWS FETCH NEXT 15 ROWS ONLY
) AS [t]
");
        }

        [Fact]
        public override void OrderBy_coalesce_skip_take_distinct_take()
        {
            base.OrderBy_coalesce_skip_take_distinct_take();

            AssertSql(@"
SELECT TOP (5) [t].[ProductID] AS [ProductID], [t].[Discontinued] AS [Discontinued], [t].[ProductName] AS [ProductName], [t].[SupplierID] AS [SupplierID], [t].[UnitPrice] AS [UnitPrice], [t].[UnitsInStock] AS [UnitsInStock]
FROM (
    SELECT DISTINCT [t_0].[ProductID] AS [ProductID], [t_0].[Discontinued] AS [Discontinued], [t_0].[ProductName] AS [ProductName], [t_0].[SupplierID] AS [SupplierID], [t_0].[UnitPrice] AS [UnitPrice], [t_0].[UnitsInStock] AS [UnitsInStock]
    FROM (
        SELECT [p].[ProductID] AS [ProductID], [p].[Discontinued] AS [Discontinued], [p].[ProductName] AS [ProductName], [p].[SupplierID] AS [SupplierID], [p].[UnitPrice] AS [UnitPrice], [p].[UnitsInStock] AS [UnitsInStock]
        FROM [Products] AS [p]
        ORDER BY COALESCE([p].[UnitPrice], 0.0) ASC
        OFFSET 5 ROWS FETCH NEXT 15 ROWS ONLY
    ) AS [t_0]
) AS [t]
");
        }

        [Fact]
        public override void OrderBy_coalesce_take_distinct()
        {
            base.OrderBy_coalesce_take_distinct();

            AssertSql(@"
SELECT DISTINCT [t].[ProductID] AS [ProductID], [t].[Discontinued] AS [Discontinued], [t].[ProductName] AS [ProductName], [t].[SupplierID] AS [SupplierID], [t].[UnitPrice] AS [UnitPrice], [t].[UnitsInStock] AS [UnitsInStock]
FROM (
    SELECT TOP (15) [p].[ProductID] AS [ProductID], [p].[Discontinued] AS [Discontinued], [p].[ProductName] AS [ProductName], [p].[SupplierID] AS [SupplierID], [p].[UnitPrice] AS [UnitPrice], [p].[UnitsInStock] AS [UnitsInStock]
    FROM [Products] AS [p]
    ORDER BY COALESCE([p].[UnitPrice], 0.0) ASC
) AS [t]
");
        }

        [Fact]
        public override void OrderBy_Distinct()
        {
            base.OrderBy_Distinct();

            AssertSql(@"
SELECT DISTINCT [c].[City]
FROM [Customers] AS [c]
");
        }

        [Fact]
        public override void OrderBy_skip_take_distinct()
        {
            base.OrderBy_skip_take_distinct();

            AssertSql(@"
SELECT DISTINCT [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
FROM (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactTitle] ASC, [c].[ContactName] ASC
    OFFSET 5 ROWS FETCH NEXT 15 ROWS ONLY
) AS [t]
");
        }

        [Fact]
        public override void OrderBy_skip_take_distinct_orderby_take()
        {
            base.OrderBy_skip_take_distinct_orderby_take();

            AssertSql(@"
SELECT TOP (8) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM (
    SELECT DISTINCT [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
    FROM (
        SELECT [c_0].[CustomerID] AS [CustomerID], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Country] AS [Country], [c_0].[Fax] AS [Fax], [c_0].[Phone] AS [Phone], [c_0].[PostalCode] AS [PostalCode], [c_0].[Region] AS [Region]
        FROM [Customers] AS [c_0]
        ORDER BY [c_0].[ContactTitle] ASC, [c_0].[ContactName] ASC
        OFFSET 5 ROWS FETCH NEXT 15 ROWS ONLY
    ) AS [t]
) AS [c]
ORDER BY [c].[ContactTitle] ASC
");
        }

        [Fact]
        public override void Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault()
        {
            base.Project_single_element_from_collection_with_OrderBy_Distinct_and_FirstOrDefault();

            AssertSql(@"
SELECT (
    SELECT TOP (1) [t].[CustomerID]
    FROM (
        SELECT DISTINCT [o].[CustomerID]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
    ) AS [t]
)
FROM [Customers] AS [c]
");
        }

        [Fact]
        public override void Select_distinct_average()
        {
            base.Select_distinct_average();

            AssertSql(@"
SELECT AVG(CAST([t].[OrderID] AS float))
FROM (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [t]
");
        }

        [Fact]
        public override void Select_distinct_count()
        {
            base.Select_distinct_count();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
) AS [t]
");
        }

        [Fact]
        public override void Select_distinct_long_count()
        {
            base.Select_distinct_long_count();

            AssertSql(@"
SELECT COUNT_BIG(*)
FROM (
    SELECT DISTINCT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
) AS [t]
");
        }

        [Fact]
        public override void Select_distinct_max()
        {
            base.Select_distinct_max();

            AssertSql(@"
SELECT MAX([t].[OrderID])
FROM (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [t]
");
        }

        [Fact]
        public override void Select_distinct_min()
        {
            base.Select_distinct_min();

            AssertSql(@"
SELECT MIN([t].[OrderID])
FROM (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [t]
");
        }

        [Fact]
        public override void Select_distinct_sum()
        {
            base.Select_distinct_sum();

            AssertSql(@"
SELECT SUM([t].[OrderID])
FROM (
    SELECT DISTINCT [o].[OrderID]
    FROM [Orders] AS [o]
) AS [t]
");
        }

        [Fact]
        public override void Select_DTO_constructor_distinct_translated_to_server()
        {
            base.Select_DTO_constructor_distinct_translated_to_server();

            AssertSql(@"
SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
");
        }

        [Fact]
        public override void Select_DTO_distinct_translated_to_server()
        {
            base.Select_DTO_distinct_translated_to_server();

            AssertSql(@"
SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
");
        }

        [Fact]
        public override void Select_DTO_with_member_init_distinct_in_subquery_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_in_subquery_translated_to_server();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM (
    SELECT DISTINCT [o].[CustomerID] AS [Id], [o].[OrderID] AS [Count]
    FROM [Orders] AS [o]
    WHERE [o].[OrderID] < 10300
) AS [o_0]
CROSS APPLY (
    SELECT [c_0].[CustomerID] AS [CustomerID], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Country] AS [Country], [c_0].[Fax] AS [Fax], [c_0].[Phone] AS [Phone], [c_0].[PostalCode] AS [PostalCode], [c_0].[Region] AS [Region]
    FROM [Customers] AS [c_0]
    WHERE [c_0].[CustomerID] = [o_0].[Id]
) AS [c]
");
        }

        [Fact]
        public override void Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_in_subquery_used_in_projection_translated_to_server();

            AssertSql(@"
SELECT [c].[CustomerID] AS [c.CustomerID], [c].[Address] AS [c.Address], [c].[City] AS [c.City], [c].[CompanyName] AS [c.CompanyName], [c].[ContactName] AS [c.ContactName], [c].[ContactTitle] AS [c.ContactTitle], [c].[Country] AS [c.Country], [c].[Fax] AS [c.Fax], [c].[Phone] AS [c.Phone], [c].[PostalCode] AS [c.PostalCode], [c].[Region] AS [c.Region], [o].[Id] AS [o.Id], [o].[Count] AS [o.Count]
FROM [Customers] AS [c]
CROSS JOIN (
    SELECT DISTINCT [o_0].[CustomerID] AS [Id], [o_0].[OrderID] AS [Count]
    FROM [Orders] AS [o_0]
    WHERE [o_0].[OrderID] < 10300
) AS [o]
WHERE LEFT([c].[CustomerID], LEN(N'A')) = N'A'
");
        }

        [Fact]
        public override void Select_DTO_with_member_init_distinct_translated_to_server()
        {
            base.Select_DTO_with_member_init_distinct_translated_to_server();

            AssertSql(@"
SELECT DISTINCT [o].[CustomerID] AS [Id], [o].[OrderID] AS [Count]
FROM [Orders] AS [o]
WHERE [o].[OrderID] < 10300
");
        }

        [Fact]
        public override void Select_Select_Distinct_Count()
        {
            base.Select_Select_Distinct_Count();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT DISTINCT [c].[City]
    FROM [Customers] AS [c]
) AS [t]
");
        }

        [Fact]
        public override void Skip_Distinct()
        {
            base.Skip_Distinct();

            AssertSql(@"
SELECT DISTINCT [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
FROM (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName] ASC
    OFFSET 5 ROWS
) AS [t]
");
        }

        [Fact]
        public override void Skip_Take_Distinct()
        {
            base.Skip_Take_Distinct();

            AssertSql(@"
SELECT DISTINCT [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
FROM (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName] ASC
    OFFSET 5 ROWS FETCH NEXT 10 ROWS ONLY
) AS [t]
");
        }

        [Fact]
        public override void Take_Distinct()
        {
            base.Take_Distinct();

            AssertSql(@"
SELECT DISTINCT [t].[OrderID] AS [OrderID], [t].[CustomerID] AS [CustomerID], [t].[EmployeeID] AS [EmployeeID], [t].[OrderDate] AS [OrderDate]
FROM (
    SELECT TOP (5) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID] ASC
) AS [t]
");
        }

        [Fact]
        public override void Take_Distinct_Count()
        {
            base.Take_Distinct_Count();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT DISTINCT [t].[OrderID] AS [OrderID], [t].[CustomerID] AS [CustomerID], [t].[EmployeeID] AS [EmployeeID], [t].[OrderDate] AS [OrderDate]
    FROM (
        SELECT TOP (5) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
        FROM [Orders] AS [o]
    ) AS [t]
) AS [t_0]
");
        }

        [Fact]
        public override void Take_Skip_Distinct()
        {
            base.Take_Skip_Distinct();

            AssertSql(@"
SELECT DISTINCT [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
FROM (
    SELECT [t_0].[CustomerID] AS [CustomerID], [t_0].[Address] AS [Address], [t_0].[City] AS [City], [t_0].[CompanyName] AS [CompanyName], [t_0].[ContactName] AS [ContactName], [t_0].[ContactTitle] AS [ContactTitle], [t_0].[Country] AS [Country], [t_0].[Fax] AS [Fax], [t_0].[Phone] AS [Phone], [t_0].[PostalCode] AS [PostalCode], [t_0].[Region] AS [Region]
    FROM (
        SELECT TOP (10) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName] ASC
    ) AS [t_0]
    ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) ASC
    OFFSET 5 ROWS
) AS [t]
");
        }

        [Fact]
        public override void Take_Skip_Distinct_Caching()
        {
            base.Take_Skip_Distinct_Caching();

            AssertSql(@"
SELECT DISTINCT [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
FROM (
    SELECT [t_0].[CustomerID] AS [CustomerID], [t_0].[Address] AS [Address], [t_0].[City] AS [City], [t_0].[CompanyName] AS [CompanyName], [t_0].[ContactName] AS [ContactName], [t_0].[ContactTitle] AS [ContactTitle], [t_0].[Country] AS [Country], [t_0].[Fax] AS [Fax], [t_0].[Phone] AS [Phone], [t_0].[PostalCode] AS [PostalCode], [t_0].[Region] AS [Region]
    FROM (
        SELECT TOP (10) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName] ASC
    ) AS [t_0]
    ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) ASC
    OFFSET 5 ROWS
) AS [t]

SELECT DISTINCT [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
FROM (
    SELECT [t_0].[CustomerID] AS [CustomerID], [t_0].[Address] AS [Address], [t_0].[City] AS [City], [t_0].[CompanyName] AS [CompanyName], [t_0].[ContactName] AS [ContactName], [t_0].[ContactTitle] AS [ContactTitle], [t_0].[Country] AS [Country], [t_0].[Fax] AS [Fax], [t_0].[Phone] AS [Phone], [t_0].[PostalCode] AS [PostalCode], [t_0].[Region] AS [Region]
    FROM (
        SELECT TOP (15) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactName] ASC
    ) AS [t_0]
    ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) ASC
    OFFSET 10 ROWS
) AS [t]
");
        }

        [Fact]
        public override void Take_Where_Distinct_Count()
        {
            base.Take_Where_Distinct_Count();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT DISTINCT [t].[OrderID] AS [OrderID], [t].[CustomerID] AS [CustomerID], [t].[EmployeeID] AS [EmployeeID], [t].[OrderDate] AS [OrderDate]
    FROM (
        SELECT TOP (5) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
        FROM [Orders] AS [o]
        WHERE [o].[CustomerID] = N'FRANK'
    ) AS [t]
) AS [t_0]
");
        }

        #endregion

        #region Queryable.Concat

        [Fact]
        public override void Concat_dbset()
        {
            base.Concat_dbset();

            AssertSql(@"
SELECT [set].[CustomerID] AS [CustomerID], [set].[Address] AS [Address], [set].[City] AS [City], [set].[CompanyName] AS [CompanyName], [set].[ContactName] AS [ContactName], [set].[ContactTitle] AS [ContactTitle], [set].[Country] AS [Country], [set].[Fax] AS [Fax], [set].[Phone] AS [Phone], [set].[PostalCode] AS [PostalCode], [set].[Region] AS [Region]
FROM (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'México D.F.'
    UNION ALL
    SELECT [c_0].[CustomerID] AS [CustomerID], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Country] AS [Country], [c_0].[Fax] AS [Fax], [c_0].[Phone] AS [Phone], [c_0].[PostalCode] AS [PostalCode], [c_0].[Region] AS [Region]
    FROM [Customers] AS [c_0]
) AS [set]
");
        }

        [Fact]
        public override void Concat_nested()
        {
            base.Concat_nested();

            AssertSql(@"
SELECT [set].[CustomerID] AS [CustomerID], [set].[Address] AS [Address], [set].[City] AS [City], [set].[CompanyName] AS [CompanyName], [set].[ContactName] AS [ContactName], [set].[ContactTitle] AS [ContactTitle], [set].[Country] AS [Country], [set].[Fax] AS [Fax], [set].[Phone] AS [Phone], [set].[PostalCode] AS [PostalCode], [set].[Region] AS [Region]
FROM (
    SELECT [set_0].[CustomerID] AS [CustomerID], [set_0].[Address] AS [Address], [set_0].[City] AS [City], [set_0].[CompanyName] AS [CompanyName], [set_0].[ContactName] AS [ContactName], [set_0].[ContactTitle] AS [ContactTitle], [set_0].[Country] AS [Country], [set_0].[Fax] AS [Fax], [set_0].[Phone] AS [Phone], [set_0].[PostalCode] AS [PostalCode], [set_0].[Region] AS [Region]
    FROM (
        SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
        FROM [Customers] AS [c]
        WHERE [c].[City] = N'México D.F.'
        UNION ALL
        SELECT [s].[CustomerID] AS [CustomerID], [s].[Address] AS [Address], [s].[City] AS [City], [s].[CompanyName] AS [CompanyName], [s].[ContactName] AS [ContactName], [s].[ContactTitle] AS [ContactTitle], [s].[Country] AS [Country], [s].[Fax] AS [Fax], [s].[Phone] AS [Phone], [s].[PostalCode] AS [PostalCode], [s].[Region] AS [Region]
        FROM [Customers] AS [s]
        WHERE [s].[City] = N'Berlin'
    ) AS [set_0]
    UNION ALL
    SELECT [e].[CustomerID] AS [CustomerID], [e].[Address] AS [Address], [e].[City] AS [City], [e].[CompanyName] AS [CompanyName], [e].[ContactName] AS [ContactName], [e].[ContactTitle] AS [ContactTitle], [e].[Country] AS [Country], [e].[Fax] AS [Fax], [e].[Phone] AS [Phone], [e].[PostalCode] AS [PostalCode], [e].[Region] AS [Region]
    FROM [Customers] AS [e]
    WHERE [e].[City] = N'London'
) AS [set]
");
        }

        [Fact]
        public override void Concat_non_entity()
        {
            base.Concat_non_entity();

            AssertSql(@"
SELECT [set].[CustomerID]
FROM (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'México D.F.'
    UNION ALL
    SELECT [s].[CustomerID]
    FROM [Customers] AS [s]
    WHERE [s].[ContactTitle] = N'Owner'
) AS [set]
");
        }

        [Fact]
        public override void Concat_simple()
        {
            base.Concat_simple();

            AssertSql(@"
SELECT [set].[CustomerID] AS [CustomerID], [set].[Address] AS [Address], [set].[City] AS [City], [set].[CompanyName] AS [CompanyName], [set].[ContactName] AS [ContactName], [set].[ContactTitle] AS [ContactTitle], [set].[Country] AS [Country], [set].[Fax] AS [Fax], [set].[Phone] AS [Phone], [set].[PostalCode] AS [PostalCode], [set].[Region] AS [Region]
FROM (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'México D.F.'
    UNION ALL
    SELECT [s].[CustomerID] AS [CustomerID], [s].[Address] AS [Address], [s].[City] AS [City], [s].[CompanyName] AS [CompanyName], [s].[ContactName] AS [ContactName], [s].[ContactTitle] AS [ContactTitle], [s].[Country] AS [Country], [s].[Fax] AS [Fax], [s].[Phone] AS [Phone], [s].[PostalCode] AS [PostalCode], [s].[Region] AS [Region]
    FROM [Customers] AS [s]
    WHERE [s].[ContactTitle] = N'Owner'
) AS [set]
");
        }

        #endregion

        #region Queryable.Except

        [Fact]
        public override void Except_dbset()
        {
            base.Except_dbset();

            AssertSql(@"
SELECT [set].[CustomerID] AS [CustomerID], [set].[Address] AS [Address], [set].[City] AS [City], [set].[CompanyName] AS [CompanyName], [set].[ContactName] AS [ContactName], [set].[ContactTitle] AS [ContactTitle], [set].[Country] AS [Country], [set].[Fax] AS [Fax], [set].[Phone] AS [Phone], [set].[PostalCode] AS [PostalCode], [set].[Region] AS [Region]
FROM (
    SELECT [s].[CustomerID] AS [CustomerID], [s].[Address] AS [Address], [s].[City] AS [City], [s].[CompanyName] AS [CompanyName], [s].[ContactName] AS [ContactName], [s].[ContactTitle] AS [ContactTitle], [s].[Country] AS [Country], [s].[Fax] AS [Fax], [s].[Phone] AS [Phone], [s].[PostalCode] AS [PostalCode], [s].[Region] AS [Region]
    FROM [Customers] AS [s]
    WHERE [s].[ContactTitle] = N'Owner'
    EXCEPT
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
) AS [set]
");
        }

        [Fact]
        public override void Except_nested()
        {
            base.Except_nested();

            AssertSql(@"
SELECT [set].[CustomerID] AS [CustomerID], [set].[Address] AS [Address], [set].[City] AS [City], [set].[CompanyName] AS [CompanyName], [set].[ContactName] AS [ContactName], [set].[ContactTitle] AS [ContactTitle], [set].[Country] AS [Country], [set].[Fax] AS [Fax], [set].[Phone] AS [Phone], [set].[PostalCode] AS [PostalCode], [set].[Region] AS [Region]
FROM (
    SELECT [set_0].[CustomerID] AS [CustomerID], [set_0].[Address] AS [Address], [set_0].[City] AS [City], [set_0].[CompanyName] AS [CompanyName], [set_0].[ContactName] AS [ContactName], [set_0].[ContactTitle] AS [ContactTitle], [set_0].[Country] AS [Country], [set_0].[Fax] AS [Fax], [set_0].[Phone] AS [Phone], [set_0].[PostalCode] AS [PostalCode], [set_0].[Region] AS [Region]
    FROM (
        SELECT [s].[CustomerID] AS [CustomerID], [s].[Address] AS [Address], [s].[City] AS [City], [s].[CompanyName] AS [CompanyName], [s].[ContactName] AS [ContactName], [s].[ContactTitle] AS [ContactTitle], [s].[Country] AS [Country], [s].[Fax] AS [Fax], [s].[Phone] AS [Phone], [s].[PostalCode] AS [PostalCode], [s].[Region] AS [Region]
        FROM [Customers] AS [s]
        WHERE [s].[ContactTitle] = N'Owner'
        EXCEPT
        SELECT [s_0].[CustomerID] AS [CustomerID], [s_0].[Address] AS [Address], [s_0].[City] AS [City], [s_0].[CompanyName] AS [CompanyName], [s_0].[ContactName] AS [ContactName], [s_0].[ContactTitle] AS [ContactTitle], [s_0].[Country] AS [Country], [s_0].[Fax] AS [Fax], [s_0].[Phone] AS [Phone], [s_0].[PostalCode] AS [PostalCode], [s_0].[Region] AS [Region]
        FROM [Customers] AS [s_0]
        WHERE [s_0].[City] = N'México D.F.'
    ) AS [set_0]
    EXCEPT
    SELECT [e].[CustomerID] AS [CustomerID], [e].[Address] AS [Address], [e].[City] AS [City], [e].[CompanyName] AS [CompanyName], [e].[ContactName] AS [ContactName], [e].[ContactTitle] AS [ContactTitle], [e].[Country] AS [Country], [e].[Fax] AS [Fax], [e].[Phone] AS [Phone], [e].[PostalCode] AS [PostalCode], [e].[Region] AS [Region]
    FROM [Customers] AS [e]
    WHERE [e].[City] = N'Seattle'
) AS [set]
");
        }

        [Fact]
        public override void Except_non_entity()
        {
            base.Except_non_entity();

            AssertSql(@"
SELECT [set].[CustomerID]
FROM (
    SELECT [s].[CustomerID]
    FROM [Customers] AS [s]
    WHERE [s].[ContactTitle] = N'Owner'
    EXCEPT
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'México D.F.'
) AS [set]
");
        }

        [Fact]
        public override void Except_simple()
        {
            base.Except_simple();

            AssertSql(@"
SELECT [set].[CustomerID] AS [CustomerID], [set].[Address] AS [Address], [set].[City] AS [City], [set].[CompanyName] AS [CompanyName], [set].[ContactName] AS [ContactName], [set].[ContactTitle] AS [ContactTitle], [set].[Country] AS [Country], [set].[Fax] AS [Fax], [set].[Phone] AS [Phone], [set].[PostalCode] AS [PostalCode], [set].[Region] AS [Region]
FROM (
    SELECT [s].[CustomerID] AS [CustomerID], [s].[Address] AS [Address], [s].[City] AS [City], [s].[CompanyName] AS [CompanyName], [s].[ContactName] AS [ContactName], [s].[ContactTitle] AS [ContactTitle], [s].[Country] AS [Country], [s].[Fax] AS [Fax], [s].[Phone] AS [Phone], [s].[PostalCode] AS [PostalCode], [s].[Region] AS [Region]
    FROM [Customers] AS [s]
    WHERE [s].[ContactTitle] = N'Owner'
    EXCEPT
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'México D.F.'
) AS [set]
");
        }

        #endregion

        #region Queryable.Intersect

        [Fact]
        public override void Intersect_dbset()
        {
            base.Intersect_dbset();

            AssertSql(@"
SELECT [set].[CustomerID] AS [CustomerID], [set].[Address] AS [Address], [set].[City] AS [City], [set].[CompanyName] AS [CompanyName], [set].[ContactName] AS [ContactName], [set].[ContactTitle] AS [ContactTitle], [set].[Country] AS [Country], [set].[Fax] AS [Fax], [set].[Phone] AS [Phone], [set].[PostalCode] AS [PostalCode], [set].[Region] AS [Region]
FROM (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'México D.F.'
    INTERSECT
    SELECT [c_0].[CustomerID] AS [CustomerID], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Country] AS [Country], [c_0].[Fax] AS [Fax], [c_0].[Phone] AS [Phone], [c_0].[PostalCode] AS [PostalCode], [c_0].[Region] AS [Region]
    FROM [Customers] AS [c_0]
) AS [set]
");
        }

        [Fact]
        public override void Intersect_nested()
        {
            base.Intersect_nested();

            AssertSql(@"
SELECT [set].[CustomerID] AS [CustomerID], [set].[Address] AS [Address], [set].[City] AS [City], [set].[CompanyName] AS [CompanyName], [set].[ContactName] AS [ContactName], [set].[ContactTitle] AS [ContactTitle], [set].[Country] AS [Country], [set].[Fax] AS [Fax], [set].[Phone] AS [Phone], [set].[PostalCode] AS [PostalCode], [set].[Region] AS [Region]
FROM (
    SELECT [set_0].[CustomerID] AS [CustomerID], [set_0].[Address] AS [Address], [set_0].[City] AS [City], [set_0].[CompanyName] AS [CompanyName], [set_0].[ContactName] AS [ContactName], [set_0].[ContactTitle] AS [ContactTitle], [set_0].[Country] AS [Country], [set_0].[Fax] AS [Fax], [set_0].[Phone] AS [Phone], [set_0].[PostalCode] AS [PostalCode], [set_0].[Region] AS [Region]
    FROM (
        SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
        FROM [Customers] AS [c]
        WHERE [c].[City] = N'México D.F.'
        INTERSECT
        SELECT [s].[CustomerID] AS [CustomerID], [s].[Address] AS [Address], [s].[City] AS [City], [s].[CompanyName] AS [CompanyName], [s].[ContactName] AS [ContactName], [s].[ContactTitle] AS [ContactTitle], [s].[Country] AS [Country], [s].[Fax] AS [Fax], [s].[Phone] AS [Phone], [s].[PostalCode] AS [PostalCode], [s].[Region] AS [Region]
        FROM [Customers] AS [s]
        WHERE [s].[ContactTitle] = N'Owner'
    ) AS [set_0]
    INTERSECT
    SELECT [e].[CustomerID] AS [CustomerID], [e].[Address] AS [Address], [e].[City] AS [City], [e].[CompanyName] AS [CompanyName], [e].[ContactName] AS [ContactName], [e].[ContactTitle] AS [ContactTitle], [e].[Country] AS [Country], [e].[Fax] AS [Fax], [e].[Phone] AS [Phone], [e].[PostalCode] AS [PostalCode], [e].[Region] AS [Region]
    FROM [Customers] AS [e]
    WHERE [e].[Fax] IS NOT NULL
) AS [set]
");
        }

        [Fact]
        public override void Intersect_non_entity()
        {
            base.Intersect_non_entity();

            AssertSql(@"
SELECT [set].[CustomerID]
FROM (
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'México D.F.'
    INTERSECT
    SELECT [s].[CustomerID]
    FROM [Customers] AS [s]
    WHERE [s].[ContactTitle] = N'Owner'
) AS [set]
");
        }

        [Fact]
        public override void Intersect_simple()
        {
            base.Intersect_simple();

            AssertSql(@"
SELECT [set].[CustomerID] AS [CustomerID], [set].[Address] AS [Address], [set].[City] AS [City], [set].[CompanyName] AS [CompanyName], [set].[ContactName] AS [ContactName], [set].[ContactTitle] AS [ContactTitle], [set].[Country] AS [Country], [set].[Fax] AS [Fax], [set].[Phone] AS [Phone], [set].[PostalCode] AS [PostalCode], [set].[Region] AS [Region]
FROM (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'México D.F.'
    INTERSECT
    SELECT [s].[CustomerID] AS [CustomerID], [s].[Address] AS [Address], [s].[City] AS [City], [s].[CompanyName] AS [CompanyName], [s].[ContactName] AS [ContactName], [s].[ContactTitle] AS [ContactTitle], [s].[Country] AS [Country], [s].[Fax] AS [Fax], [s].[Phone] AS [Phone], [s].[PostalCode] AS [PostalCode], [s].[Region] AS [Region]
    FROM [Customers] AS [s]
    WHERE [s].[ContactTitle] = N'Owner'
) AS [set]
");
        }

        #endregion

        #region Queryable.Union

        [Fact]
        public override void Union_dbset()
        {
            base.Union_dbset();

            AssertSql(@"
SELECT [set].[CustomerID] AS [CustomerID], [set].[Address] AS [Address], [set].[City] AS [City], [set].[CompanyName] AS [CompanyName], [set].[ContactName] AS [ContactName], [set].[ContactTitle] AS [ContactTitle], [set].[Country] AS [Country], [set].[Fax] AS [Fax], [set].[Phone] AS [Phone], [set].[PostalCode] AS [PostalCode], [set].[Region] AS [Region]
FROM (
    SELECT [s].[CustomerID] AS [CustomerID], [s].[Address] AS [Address], [s].[City] AS [City], [s].[CompanyName] AS [CompanyName], [s].[ContactName] AS [ContactName], [s].[ContactTitle] AS [ContactTitle], [s].[Country] AS [Country], [s].[Fax] AS [Fax], [s].[Phone] AS [Phone], [s].[PostalCode] AS [PostalCode], [s].[Region] AS [Region]
    FROM [Customers] AS [s]
    WHERE [s].[ContactTitle] = N'Owner'
    UNION
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
) AS [set]
");
        }

        [Fact]
        public override void Union_nested()
        {
            base.Union_nested();

            AssertSql(@"
SELECT [set].[CustomerID] AS [CustomerID], [set].[Address] AS [Address], [set].[City] AS [City], [set].[CompanyName] AS [CompanyName], [set].[ContactName] AS [ContactName], [set].[ContactTitle] AS [ContactTitle], [set].[Country] AS [Country], [set].[Fax] AS [Fax], [set].[Phone] AS [Phone], [set].[PostalCode] AS [PostalCode], [set].[Region] AS [Region]
FROM (
    SELECT [set_0].[CustomerID] AS [CustomerID], [set_0].[Address] AS [Address], [set_0].[City] AS [City], [set_0].[CompanyName] AS [CompanyName], [set_0].[ContactName] AS [ContactName], [set_0].[ContactTitle] AS [ContactTitle], [set_0].[Country] AS [Country], [set_0].[Fax] AS [Fax], [set_0].[Phone] AS [Phone], [set_0].[PostalCode] AS [PostalCode], [set_0].[Region] AS [Region]
    FROM (
        SELECT [s].[CustomerID] AS [CustomerID], [s].[Address] AS [Address], [s].[City] AS [City], [s].[CompanyName] AS [CompanyName], [s].[ContactName] AS [ContactName], [s].[ContactTitle] AS [ContactTitle], [s].[Country] AS [Country], [s].[Fax] AS [Fax], [s].[Phone] AS [Phone], [s].[PostalCode] AS [PostalCode], [s].[Region] AS [Region]
        FROM [Customers] AS [s]
        WHERE [s].[ContactTitle] = N'Owner'
        UNION
        SELECT [s_0].[CustomerID] AS [CustomerID], [s_0].[Address] AS [Address], [s_0].[City] AS [City], [s_0].[CompanyName] AS [CompanyName], [s_0].[ContactName] AS [ContactName], [s_0].[ContactTitle] AS [ContactTitle], [s_0].[Country] AS [Country], [s_0].[Fax] AS [Fax], [s_0].[Phone] AS [Phone], [s_0].[PostalCode] AS [PostalCode], [s_0].[Region] AS [Region]
        FROM [Customers] AS [s_0]
        WHERE [s_0].[City] = N'México D.F.'
    ) AS [set_0]
    UNION
    SELECT [e].[CustomerID] AS [CustomerID], [e].[Address] AS [Address], [e].[City] AS [City], [e].[CompanyName] AS [CompanyName], [e].[ContactName] AS [ContactName], [e].[ContactTitle] AS [ContactTitle], [e].[Country] AS [Country], [e].[Fax] AS [Fax], [e].[Phone] AS [Phone], [e].[PostalCode] AS [PostalCode], [e].[Region] AS [Region]
    FROM [Customers] AS [e]
    WHERE [e].[City] = N'London'
) AS [set]
");
        }

        [Fact]
        public override void Union_non_entity()
        {
            base.Union_non_entity();

            AssertSql(@"
SELECT [set].[CustomerID]
FROM (
    SELECT [s].[CustomerID]
    FROM [Customers] AS [s]
    WHERE [s].[ContactTitle] = N'Owner'
    UNION
    SELECT [c].[CustomerID]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'México D.F.'
) AS [set]
");
        }

        [Fact]
        public override void Union_simple()
        {
            base.Union_simple();

            AssertSql(@"
SELECT [set].[CustomerID] AS [CustomerID], [set].[Address] AS [Address], [set].[City] AS [City], [set].[CompanyName] AS [CompanyName], [set].[ContactName] AS [ContactName], [set].[ContactTitle] AS [ContactTitle], [set].[Country] AS [Country], [set].[Fax] AS [Fax], [set].[Phone] AS [Phone], [set].[PostalCode] AS [PostalCode], [set].[Region] AS [Region]
FROM (
    SELECT [s].[CustomerID] AS [CustomerID], [s].[Address] AS [Address], [s].[City] AS [City], [s].[CompanyName] AS [CompanyName], [s].[ContactName] AS [ContactName], [s].[ContactTitle] AS [ContactTitle], [s].[Country] AS [Country], [s].[Fax] AS [Fax], [s].[Phone] AS [Phone], [s].[PostalCode] AS [PostalCode], [s].[Region] AS [Region]
    FROM [Customers] AS [s]
    WHERE [s].[ContactTitle] = N'Owner'
    UNION
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    WHERE [c].[City] = N'México D.F.'
) AS [set]
");
        }

        [Fact]
        public override void Union_with_custom_projection()
        {
            base.Union_with_custom_projection();

            AssertSql(@"
SELECT [set].[CustomerID] AS [Id]
FROM (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    WHERE LEFT([c].[CompanyName], LEN(N'A')) = N'A'
    UNION
    SELECT [c_0].[CustomerID] AS [CustomerID], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Country] AS [Country], [c_0].[Fax] AS [Fax], [c_0].[Phone] AS [Phone], [c_0].[PostalCode] AS [PostalCode], [c_0].[Region] AS [Region]
    FROM [Customers] AS [c_0]
    WHERE LEFT([c_0].[CompanyName], LEN(N'B')) = N'B'
) AS [set]
");
        }

        #endregion

        #region Queryable.Zip

        // *tumbleweeds*

        #endregion

        #region Queryable.SequenceEqual

        // *coyote howl*

        #endregion

        #region Queryable.SkipWhile

        // *gust of wind*

        #endregion

        #region Queryable.TakeWhile

        // *owl hooting*

        #endregion

        #region Queryable.Skip

        [Fact]
        public override void Join_Customers_Orders_Orders_Skip_Take_Same_Properties()
        {
            base.Join_Customers_Orders_Orders_Skip_Take_Same_Properties();

            AssertSql(@"
SELECT [o].[OrderID] AS [OrderID], [ca].[CustomerID] AS [CustomerIDA], [cb].[CustomerID] AS [CustomerIDB], [ca].[ContactName] AS [ContactNameA], [cb].[ContactName] AS [ContactNameB]
FROM [Orders] AS [o]
INNER JOIN [Customers] AS [ca] ON [o].[CustomerID] = [ca].[CustomerID]
INNER JOIN [Customers] AS [cb] ON [o].[CustomerID] = [cb].[CustomerID]
ORDER BY [o].[OrderID] ASC
OFFSET 10 ROWS FETCH NEXT 5 ROWS ONLY
");
        }

        [Fact]
        public override void Join_Customers_Orders_Projection_With_String_Concat_Skip_Take()
        {
            base.Join_Customers_Orders_Projection_With_String_Concat_Skip_Take();

            AssertSql(@"
SELECT ([c].[ContactName] + N' ') + [c].[ContactTitle] AS [Contact], [o].[OrderID] AS [OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID] ASC
OFFSET 10 ROWS FETCH NEXT 5 ROWS ONLY
");
        }

        [Fact]
        public override void Join_Customers_Orders_Skip_Take()
        {
            base.Join_Customers_Orders_Skip_Take();

            AssertSql(@"
SELECT [c].[ContactName] AS [ContactName], [o].[OrderID] AS [OrderID]
FROM [Customers] AS [c]
INNER JOIN [Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
ORDER BY [o].[OrderID] ASC
OFFSET 10 ROWS FETCH NEXT 5 ROWS ONLY
");
        }

        [Fact]
        public override void OrderBy_Dto_projection_skip_take()
        {
            base.OrderBy_Dto_projection_skip_take();

            AssertSql(@"
SELECT [c].[CustomerID] AS [Id]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] ASC
OFFSET 5 ROWS FETCH NEXT 10 ROWS ONLY
");
        }

        [Fact]
        public override void OrderBy_Skip_Last_gives_correct_result()
        {
            base.OrderBy_Skip_Last_gives_correct_result();

            AssertSql(@"
SELECT TOP (1) [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
FROM (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID] ASC
    OFFSET 20 ROWS
) AS [t]
ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) DESC
");
        }

        [Fact]
        public override void OrderBy_skip_skip_take()
        {
            base.OrderBy_skip_skip_take();

            AssertSql(@"
SELECT [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
FROM (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactTitle] ASC, [c].[ContactName] ASC
    OFFSET 5 ROWS
) AS [t]
ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) ASC
OFFSET 8 ROWS FETCH NEXT 3 ROWS ONLY
");
        }

        [Fact]
        public override void OrderBy_skip_take()
        {
            base.OrderBy_skip_take();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactTitle] ASC, [c].[ContactName] ASC
OFFSET 5 ROWS FETCH NEXT 8 ROWS ONLY
");
        }

        [Fact]
        public override void OrderBy_skip_take_skip_take_skip()
        {
            base.OrderBy_skip_take_skip_take_skip();

            AssertSql(@"
SELECT [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
FROM (
    SELECT [t_0].[CustomerID] AS [CustomerID], [t_0].[Address] AS [Address], [t_0].[City] AS [City], [t_0].[CompanyName] AS [CompanyName], [t_0].[ContactName] AS [ContactName], [t_0].[ContactTitle] AS [ContactTitle], [t_0].[Country] AS [Country], [t_0].[Fax] AS [Fax], [t_0].[Phone] AS [Phone], [t_0].[PostalCode] AS [PostalCode], [t_0].[Region] AS [Region]
    FROM (
        SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
        FROM [Customers] AS [c]
        ORDER BY [c].[ContactTitle] ASC, [c].[ContactName] ASC
        OFFSET 5 ROWS FETCH NEXT 15 ROWS ONLY
    ) AS [t_0]
    ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) ASC
    OFFSET 2 ROWS FETCH NEXT 8 ROWS ONLY
) AS [t]
ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) ASC
OFFSET 5 ROWS
");
        }

        [Fact]
        public override void OrderBy_skip_take_take()
        {
            base.OrderBy_skip_take_take();

            AssertSql(@"
SELECT TOP (3) [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
FROM (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactTitle] ASC, [c].[ContactName] ASC
    OFFSET 5 ROWS FETCH NEXT 8 ROWS ONLY
) AS [t]
");
        }

        [Fact]
        public override void OrderBy_skip_take_take_take_take()
        {
            base.OrderBy_skip_take_take_take_take();

            AssertSql(@"
SELECT TOP (5) [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
FROM (
    SELECT TOP (8) [t_0].[CustomerID] AS [CustomerID], [t_0].[Address] AS [Address], [t_0].[City] AS [City], [t_0].[CompanyName] AS [CompanyName], [t_0].[ContactName] AS [ContactName], [t_0].[ContactTitle] AS [ContactTitle], [t_0].[Country] AS [Country], [t_0].[Fax] AS [Fax], [t_0].[Phone] AS [Phone], [t_0].[PostalCode] AS [PostalCode], [t_0].[Region] AS [Region]
    FROM (
        SELECT TOP (10) [t_1].[CustomerID] AS [CustomerID], [t_1].[Address] AS [Address], [t_1].[City] AS [City], [t_1].[CompanyName] AS [CompanyName], [t_1].[ContactName] AS [ContactName], [t_1].[ContactTitle] AS [ContactTitle], [t_1].[Country] AS [Country], [t_1].[Fax] AS [Fax], [t_1].[Phone] AS [Phone], [t_1].[PostalCode] AS [PostalCode], [t_1].[Region] AS [Region]
        FROM (
            SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
            FROM [Customers] AS [c]
            ORDER BY [c].[ContactTitle] ASC, [c].[ContactName] ASC
            OFFSET 5 ROWS FETCH NEXT 15 ROWS ONLY
        ) AS [t_1]
    ) AS [t_0]
) AS [t]
");
        }

        [Fact]
        public override void Project_single_element_from_collection_with_OrderBy_Skip_and_FirstOrDefault()
        {
            base.Project_single_element_from_collection_with_OrderBy_Skip_and_FirstOrDefault();

            AssertSql(@"
SELECT (
    SELECT [o].[CustomerID]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    ORDER BY [o].[OrderID] ASC
    OFFSET 1 ROWS FETCH NEXT 1 ROWS ONLY
)
FROM [Customers] AS [c]
");
        }

        [Fact]
        public override void Select_orderBy_skip_count()
        {
            base.Select_orderBy_skip_count();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[Country] ASC
    OFFSET 7 ROWS
) AS [t]
");
        }

        [Fact]
        public override void Select_orderBy_skip_long_count()
        {
            base.Select_orderBy_skip_long_count();

            AssertSql(@"
SELECT COUNT_BIG(*)
FROM (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[Country] ASC
    OFFSET 7 ROWS
) AS [t]
");
        }

        [Fact]
        public override void Select_skip_average()
        {
            base.Select_skip_average();

            AssertSql(@"
SELECT AVG(CAST([t].[OrderID] AS float))
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID] ASC
    OFFSET 10 ROWS
) AS [t]
");
        }

        [Fact]
        public override void Select_skip_count()
        {
            base.Select_skip_count();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) ASC
    OFFSET 7 ROWS
) AS [t]
");
        }

        [Fact]
        public override void Select_skip_long_count()
        {
            base.Select_skip_long_count();

            AssertSql(@"
SELECT COUNT_BIG(*)
FROM (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) ASC
    OFFSET 7 ROWS
) AS [t]
");
        }

        [Fact]
        public override void Select_skip_max()
        {
            base.Select_skip_max();

            AssertSql(@"
SELECT MAX([t].[OrderID])
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID] ASC
    OFFSET 10 ROWS
) AS [t]
");
        }

        [Fact]
        public override void Select_skip_min()
        {
            base.Select_skip_min();

            AssertSql(@"
SELECT MIN([t].[OrderID])
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID] ASC
    OFFSET 10 ROWS
) AS [t]
");
        }

        [Fact]
        public override void Select_skip_sum()
        {
            base.Select_skip_sum();

            AssertSql(@"
SELECT SUM([t].[OrderID])
FROM (
    SELECT [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID] ASC
    OFFSET 10 ROWS
) AS [t]
");
        }

        [Fact]
        public override void Select_take_skip_null_coalesce_operator()
        {
            base.Select_take_skip_null_coalesce_operator();

            AssertSql(@"
SELECT [t].[CustomerID] AS [CustomerID], [t].[CompanyName] AS [CompanyName], [t].[Region] AS [Region]
FROM (
    SELECT TOP (10) [c].[CustomerID] AS [CustomerID], [c].[CompanyName] AS [CompanyName], COALESCE([c].[Region], N'ZZ') AS [Region]
    FROM [Customers] AS [c]
    ORDER BY COALESCE([c].[Region], N'ZZ') ASC
) AS [t]
ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) ASC
OFFSET 5 ROWS
");
        }

        [Fact]
        public override void Select_take_skip_null_coalesce_operator2()
        {
            base.Select_take_skip_null_coalesce_operator2();

            AssertSql(@"
SELECT [t].[CustomerID] AS [CustomerID], [t].[CompanyName] AS [CompanyName], [t].[Region] AS [Region]
FROM (
    SELECT TOP (10) [c].[CustomerID] AS [CustomerID], [c].[CompanyName] AS [CompanyName], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY COALESCE([c].[Region], N'ZZ') ASC
) AS [t]
ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) ASC
OFFSET 5 ROWS
");
        }

        [Fact]
        public override void Select_take_skip_null_coalesce_operator3()
        {
            base.Select_take_skip_null_coalesce_operator3();

            AssertSql(@"
SELECT [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
FROM (
    SELECT TOP (10) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY COALESCE([c].[Region], N'ZZ') ASC
) AS [t]
ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) ASC
OFFSET 5 ROWS
");
        }

        [Fact]
        public override void Skip()
        {
            base.Skip();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] ASC
OFFSET 5 ROWS
");
        }

        [Fact]
        public override void Skip_no_orderby()
        {
            base.Skip_no_orderby();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) ASC
OFFSET 5 ROWS
");
        }

        [Fact]
        public override void Skip_Take()
        {
            base.Skip_Take();

            AssertSql(@"
SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
ORDER BY [c].[ContactName] ASC
OFFSET 5 ROWS FETCH NEXT 10 ROWS ONLY
");
        }

        [Fact]
        public override void Take_Skip()
        {
            base.Take_Skip();

            AssertSql(@"
SELECT [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
FROM (
    SELECT TOP (10) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[ContactName] ASC
) AS [t]
ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) ASC
OFFSET 5 ROWS
");
        }

        [Fact]
        public override void Take_skip_null_coalesce_operator()
        {
            base.Take_skip_null_coalesce_operator();

            AssertSql(@"
SELECT DISTINCT [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
FROM (
    SELECT [t_0].[CustomerID] AS [CustomerID], [t_0].[Address] AS [Address], [t_0].[City] AS [City], [t_0].[CompanyName] AS [CompanyName], [t_0].[ContactName] AS [ContactName], [t_0].[ContactTitle] AS [ContactTitle], [t_0].[Country] AS [Country], [t_0].[Fax] AS [Fax], [t_0].[Phone] AS [Phone], [t_0].[PostalCode] AS [PostalCode], [t_0].[Region] AS [Region]
    FROM (
        SELECT TOP (10) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
        FROM [Customers] AS [c]
        ORDER BY COALESCE([c].[Region], N'ZZ') ASC
    ) AS [t_0]
    ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) ASC
    OFFSET 5 ROWS
) AS [t]
");
        }

        #endregion

        #region Queryable.Take

        [Fact]
        public override void GroupJoin_customers_employees_subquery_shadow_take()
        {
            base.GroupJoin_customers_employees_subquery_shadow_take();

            AssertSql(@"
SELECT [t].[Title] AS [Title], [t].[EmployeeID] AS [Id]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT TOP (5) [e].[EmployeeID] AS [EmployeeID], [e].[City] AS [City], [e].[Country] AS [Country], [e].[FirstName] AS [FirstName], [e].[ReportsTo] AS [ReportsTo], [e].[Title] AS [Title]
    FROM [Employees] AS [e]
    ORDER BY [e].[City] ASC
) AS [t] ON [c].[City] = [t].[City]
");
        }

        [Fact]
        public override void Join_customers_orders_with_subquery_anonymous_property_method_with_take()
        {
            base.Join_customers_orders_with_subquery_anonymous_property_method_with_take();

            AssertSql(@"
SELECT [o1].[o2.OrderID] AS [o1.o2.OrderID], [o1].[o2.CustomerID] AS [o1.o2.CustomerID], [o1].[o2.EmployeeID] AS [o1.o2.EmployeeID], [o1].[o2.OrderDate] AS [o1.o2.OrderDate], [o1].[o2.OrderID] AS [o2.OrderID], [o1].[o2.CustomerID] AS [o2.CustomerID], [o1].[o2.EmployeeID] AS [o2.EmployeeID], [o1].[o2.OrderDate] AS [o2.OrderDate], [o1].[o2.OrderDate] AS [Shadow]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT TOP (5) [o2].[OrderID] AS [o2.OrderID], [o2].[CustomerID] AS [o2.CustomerID], [o2].[EmployeeID] AS [o2.EmployeeID], [o2].[OrderDate] AS [o2.OrderDate]
    FROM [Orders] AS [o2]
    ORDER BY [o2].[OrderID] ASC
) AS [o1] ON [c].[CustomerID] = [o1].[o2.CustomerID]
WHERE [o1].[o2.CustomerID] = N'ALFKI'
");
        }

        [Fact]
        public override void Join_customers_orders_with_subquery_predicate_with_take()
        {
            base.Join_customers_orders_with_subquery_predicate_with_take();

            AssertSql(@"
SELECT [c].[ContactName] AS [ContactName], [o1].[OrderID] AS [OrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT TOP (5) [o2].[OrderID] AS [OrderID], [o2].[CustomerID] AS [CustomerID], [o2].[EmployeeID] AS [EmployeeID], [o2].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o2]
    WHERE [o2].[OrderID] > 0
    ORDER BY [o2].[OrderID] ASC
) AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [o1].[CustomerID] = N'ALFKI'
");
        }

        [Fact]
        public override void Join_customers_orders_with_subquery_with_take()
        {
            base.Join_customers_orders_with_subquery_with_take();

            AssertSql(@"
SELECT [c].[ContactName] AS [ContactName], [o1].[OrderID] AS [OrderID]
FROM [Customers] AS [c]
INNER JOIN (
    SELECT TOP (5) [o2].[OrderID] AS [OrderID], [o2].[CustomerID] AS [CustomerID], [o2].[EmployeeID] AS [EmployeeID], [o2].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o2]
    ORDER BY [o2].[OrderID] ASC
) AS [o1] ON [c].[CustomerID] = [o1].[CustomerID]
WHERE [o1].[CustomerID] = N'ALFKI'
");
        }

        [Fact]
        public override void Join_take_count_works()
        {
            base.Join_take_count_works();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT TOP (5) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o]
    INNER JOIN (
        SELECT [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
        FROM [Customers] AS [c]
        WHERE [c].[CustomerID] = N'ALFKI'
    ) AS [c_0] ON [o].[CustomerID] = [c_0].[CustomerID]
    WHERE ([o].[OrderID] > 690) AND ([o].[OrderID] < 710)
) AS [t]
");
        }

        [Fact]
        public override void OrderBy_client_Take()
        {
            base.OrderBy_client_Take();

            AssertSql(@"
SELECT [o].[EmployeeID] AS [EmployeeID], [o].[City] AS [City], [o].[Country] AS [Country], [o].[FirstName] AS [FirstName], [o].[ReportsTo] AS [ReportsTo], [o].[Title] AS [Title]
FROM [Employees] AS [o]
");
        }

        [Fact]
        public override void OrderBy_Take_Count()
        {
            base.OrderBy_Take_Count();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT TOP (5) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID] ASC
) AS [t]
");
        }

        [Fact]
        public override void OrderBy_Take_Last_gives_correct_result()
        {
            base.OrderBy_Take_Last_gives_correct_result();

            AssertSql(@"
SELECT TOP (1) [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
FROM (
    SELECT TOP (20) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID] ASC
) AS [t]
ORDER BY ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) DESC
");
        }

        [Fact]
        public override void Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault()
        {
            base.Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault();

            AssertSql(@"
SELECT (
    SELECT TOP (1) [t].[CustomerID]
    FROM (
        SELECT TOP (2) [o].[CustomerID]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderID] ASC, [o].[OrderDate] DESC
    ) AS [t]
)
FROM [Customers] AS [c]
");
        }

        [Fact]
        public override void Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2()
        {
            base.Project_single_element_from_collection_with_multiple_OrderBys_Take_and_FirstOrDefault_2();

            AssertSql(@"
SELECT (
    SELECT TOP (1) [t].[CustomerID]
    FROM (
        SELECT TOP (2) [o].[CustomerID]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[CustomerID] ASC, [o].[OrderDate] DESC
    ) AS [t]
)
FROM [Customers] AS [c]
");
        }

        [Fact]
        public override void Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault()
        {
            base.Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault();

            AssertSql(@"
SELECT (
    SELECT TOP (1) [t].[OrderID]
    FROM (
        SELECT TOP (1) [o].[OrderID]
        FROM [Order Details] AS [o]
        INNER JOIN [Products] AS [p] ON [o].[ProductID] = [p].[ProductID]
        WHERE [o_0].[OrderID] = [o].[OrderID]
        ORDER BY [p].[ProductName] ASC
    ) AS [t]
)
FROM [Orders] AS [o_0]
WHERE [o_0].[OrderID] < 10300
");
        }

        [Fact]
        public override void Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault()
        {
            base.Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault();

            AssertSql(@"
SELECT (
    SELECT TOP (1) [t].[CustomerID]
    FROM (
        SELECT TOP (1) [o].[CustomerID]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderID] ASC
    ) AS [t]
)
FROM [Customers] AS [c]
");
        }

        [Fact]
        public override void Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault_with_parameter()
        {
            base.Project_single_element_from_collection_with_OrderBy_Take_and_FirstOrDefault_with_parameter();

            AssertSql(@"
@p0='1'

SELECT (
    SELECT TOP (1) [t].[CustomerID]
    FROM (
        SELECT TOP (@p0) [o].[CustomerID]
        FROM [Orders] AS [o]
        WHERE [c].[CustomerID] = [o].[CustomerID]
        ORDER BY [o].[OrderID] ASC
    ) AS [t]
)
FROM [Customers] AS [c]
");
        }

        [Fact]
        [Trait("Impatient", "Improve SQL")]
        [Trait("Impatient", "Low priority")]
        public override void Project_single_element_from_collection_with_OrderBy_Take_and_SingleOrDefault()
        {
            base.Project_single_element_from_collection_with_OrderBy_Take_and_SingleOrDefault();

            AssertSql(@"
SELECT [c].[CustomerID] AS [$outer.CustomerID], [c].[Address] AS [$outer.Address], [c].[City] AS [$outer.City], [c].[CompanyName] AS [$outer.CompanyName], [c].[ContactName] AS [$outer.ContactName], [c].[ContactTitle] AS [$outer.ContactTitle], [c].[Country] AS [$outer.Country], [c].[Fax] AS [$outer.Fax], [c].[Phone] AS [$outer.Phone], [c].[PostalCode] AS [$outer.PostalCode], [c].[Region] AS [$outer.Region], (
    SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    FOR JSON PATH
) AS [$inner]
FROM [Customers] AS [c]
WHERE [c].[CustomerID] = N'ALFKI'
");
        }

        [Fact]
        public override void SelectMany_Joined_Take()
        {
            base.SelectMany_Joined_Take();

            AssertSql(@"
SELECT [c].[ContactName] AS [ContactName], [o].[OrderID] AS [o.OrderID], [o].[CustomerID] AS [o.CustomerID], [o].[EmployeeID] AS [o.EmployeeID], [o].[OrderDate] AS [o.OrderDate]
FROM [Customers] AS [c]
CROSS APPLY (
    SELECT TOP (1000) [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o_0]
    WHERE [o_0].[CustomerID] = [c].[CustomerID]
) AS [o]
");
        }

        [Fact]
        public override void Select_orderBy_take_count()
        {
            base.Select_orderBy_take_count();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT TOP (7) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[Country] ASC
) AS [t]
");
        }

        [Fact]
        public override void Select_orderBy_take_long_count()
        {
            base.Select_orderBy_take_long_count();

            AssertSql(@"
SELECT COUNT_BIG(*)
FROM (
    SELECT TOP (7) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[Country] ASC
) AS [t]
");
        }

        [Fact]
        public override void Select_scalar_primitive_after_take()
        {
            base.Select_scalar_primitive_after_take();

            AssertSql(@"
SELECT TOP (9) [e].[EmployeeID]
FROM [Employees] AS [e]
");
        }

        [Fact]
        public override void Select_take_average()
        {
            base.Select_take_average();

            AssertSql(@"
SELECT AVG(CAST([t].[OrderID] AS float))
FROM (
    SELECT TOP (10) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID] ASC
) AS [t]
");
        }

        [Fact]
        public override void Select_take_count()
        {
            base.Select_take_count();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT TOP (7) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
) AS [t]
");
        }

        [Fact]
        public override void Select_take_long_count()
        {
            base.Select_take_long_count();

            AssertSql(@"
SELECT COUNT_BIG(*)
FROM (
    SELECT TOP (7) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
) AS [t]
");
        }

        [Fact]
        public override void Select_take_max()
        {
            base.Select_take_max();

            AssertSql(@"
SELECT MAX([t].[OrderID])
FROM (
    SELECT TOP (10) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID] ASC
) AS [t]
");
        }

        [Fact]
        public override void Select_take_min()
        {
            base.Select_take_min();

            AssertSql(@"
SELECT MIN([t].[OrderID])
FROM (
    SELECT TOP (10) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID] ASC
) AS [t]
");
        }

        [Fact]
        public override void Select_take_null_coalesce_operator()
        {
            base.Select_take_null_coalesce_operator();

            AssertSql(@"
SELECT TOP (5) [c].[CustomerID] AS [CustomerID], [c].[CompanyName] AS [CompanyName], COALESCE([c].[Region], N'ZZ') AS [Region]
FROM [Customers] AS [c]
ORDER BY COALESCE([c].[Region], N'ZZ') ASC
");
        }

        [Fact]
        public override void Select_take_sum()
        {
            base.Select_take_sum();

            AssertSql(@"
SELECT SUM([t].[OrderID])
FROM (
    SELECT TOP (10) [o].[OrderID]
    FROM [Orders] AS [o]
    ORDER BY [o].[OrderID] ASC
) AS [t]
");
        }

        [Fact]
        public override void Take_OrderBy_Count()
        {
            base.Take_OrderBy_Count();

            AssertSql(@"
SELECT COUNT(*)
FROM (
    SELECT TOP (5) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate]
    FROM [Orders] AS [o]
) AS [o_0]
");
        }

        [Fact]
        public override void Take_simple()
        {
            base.Take_simple();

            AssertSql(@"
SELECT TOP (10) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] ASC
");
        }

        [Fact]
        public override void Take_simple_parameterized()
        {
            base.Take_simple_parameterized();

            // Why would we bother parameterizing a constant literal?
            // What if the database engine could optimize for the constant
            // value better than it could for a parameter here?

            AssertSql(@"
SELECT TOP (10) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] ASC
");
        }

        [Fact]
        public override void Take_simple_projection()
        {
            base.Take_simple_projection();

            AssertSql(@"
SELECT TOP (10) [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] ASC
");
        }

        [Fact]
        public override void Take_subquery_projection()
        {
            base.Take_subquery_projection();

            AssertSql(@"
SELECT TOP (2) [c].[City]
FROM [Customers] AS [c]
ORDER BY [c].[CustomerID] ASC
");
        }

        [Fact]
        public override void Take_with_single()
        {
            base.Take_with_single();

            AssertSql(@"
SELECT TOP (2) [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region]
FROM (
    SELECT TOP (1) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
    FROM [Customers] AS [c]
    ORDER BY [c].[CustomerID] ASC
) AS [t]
");
        }

        [Fact]
        public override void Take_with_single_select_many()
        {
            base.Take_with_single_select_many();

            // We don't have to add the TOP (2) to the query when it already has TOP.
            // If someone did Take(1000000).Single(), that might be a performance issue,
            // but that would be a dumb query.

            AssertSql(@"
SELECT TOP (1) [c].[CustomerID] AS [c.CustomerID], [c].[Address] AS [c.Address], [c].[City] AS [c.City], [c].[CompanyName] AS [c.CompanyName], [c].[ContactName] AS [c.ContactName], [c].[ContactTitle] AS [c.ContactTitle], [c].[Country] AS [c.Country], [c].[Fax] AS [c.Fax], [c].[Phone] AS [c.Phone], [c].[PostalCode] AS [c.PostalCode], [c].[Region] AS [c.Region], [o].[OrderID] AS [o.OrderID], [o].[CustomerID] AS [o.CustomerID], [o].[EmployeeID] AS [o.EmployeeID], [o].[OrderDate] AS [o.OrderDate]
FROM [Customers] AS [c]
CROSS JOIN [Orders] AS [o]
ORDER BY [c].[CustomerID] ASC, [o].[OrderID] ASC
");
        }

        #endregion

        #region skips

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Join_client_new_expression()
        {
            base.Join_client_new_expression();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void OrderBy_multiple_queries()
        {
            base.OrderBy_multiple_queries();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Average_no_data()
        {
            base.Average_no_data();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Average_no_data_subquery()
        {
            base.Average_no_data_subquery();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Max_no_data()
        {
            base.Max_no_data();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Max_no_data_subquery()
        {
            base.Max_no_data_subquery();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Min_no_data()
        {
            base.Min_no_data();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Min_no_data_subquery()
        {
            base.Min_no_data_subquery();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Default_if_empty_top_level_arg()
        {
            base.Default_if_empty_top_level_arg();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void First_client_predicate()
        {
            // TODO: split predicate from method call during composition
            base.First_client_predicate();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Parameter_extraction_short_circuits_1()
        {
            base.Parameter_extraction_short_circuits_1();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Parameter_extraction_short_circuits_2()
        {
            base.Parameter_extraction_short_circuits_2();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Parameter_extraction_short_circuits_3()
        {
            base.Parameter_extraction_short_circuits_3();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Parameter_extraction_can_throw_exception_from_user_code()
        {
            base.Parameter_extraction_can_throw_exception_from_user_code();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Select_expression_date_add_milliseconds_above_the_range()
        {
            base.Select_expression_date_add_milliseconds_above_the_range();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Select_expression_date_add_milliseconds_below_the_range()
        {
            base.Select_expression_date_add_milliseconds_below_the_range();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Comparing_collection_navigation_to_null()
        {
            base.Comparing_collection_navigation_to_null();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void QueryType_select_where_navigation()
        {
            base.QueryType_select_where_navigation();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void QueryType_select_where_navigation_multi_level()
        {
            base.QueryType_select_where_navigation_multi_level();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void QueryType_with_defining_query()
        {
            base.QueryType_with_defining_query();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void QueryType_with_included_nav()
        {
            base.QueryType_with_included_nav();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void QueryType_with_mixed_tracking()
        {
            base.QueryType_with_mixed_tracking();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void QueryType_with_included_navs_multi_level()
        {
            base.QueryType_with_included_navs_multi_level();
        }

        #endregion

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void GroupJoin_customers_orders()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os.OrderBy(o => o.OrderID) on c.CustomerID equals o.CustomerID into orders
                    select new { customer = c, orders = orders.ToList() },
                e => e.customer.CustomerID,
                elementAsserter: (e, a) =>
                {
                    Assert.Equal(e.customer.CustomerID, a.customer.CustomerID);
                    CollectionAsserter<Order>(o => o.OrderID)(e.orders, a.orders);
                },
                entryCount: 921);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Select_correlated_subquery_filtered()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    where c.CustomerID.StartsWith("A")
                    orderby c.CustomerID
                    select os.Where(o => o.CustomerID == c.CustomerID),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Order>(o => o.OrderID),
                entryCount: 30);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Select_correlated_subquery_projection()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs.OrderBy(cc => cc.CustomerID).Take(3)
                    select os.Where(o => o.CustomerID == c.CustomerID),
                assertOrder: true,
                elementAsserter: CollectionAsserter<Order>(o => o.OrderID),
                entryCount: 17);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Select_subquery_recursive_trivial()
        {
            AssertQuery<Employee>(
                es =>
                    from e1 in es
                    select (from e2 in es
                            select (from e3 in es
                                    orderby e3.EmployeeID
                                    select e3)),
                e => ((IEnumerable<IEnumerable<Employee>>)e).Count(),
                elementAsserter: (e, a) =>
                {
                    var expected = ((IEnumerable<IEnumerable<Employee>>)e).SelectMany(i => i).ToList();
                    var actual = ((IEnumerable<IEnumerable<Employee>>)e).SelectMany(i => i).ToList();

                    Assert.Equal(expected, actual);
                },
                entryCount: 9);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        [Trait("Impatient", "EFCore missing entries")]
        public override void Join_customers_orders_with_subquery_anonymous_property_method()
        {
            //base.Join_customers_orders_with_subquery_anonymous_property_method();

            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o1 in
                        (from o2 in os orderby o2.OrderID select new { o2 }) on c.CustomerID equals o1.o2.CustomerID
                    where EF.Property<string>(o1.o2, "CustomerID") == "ALFKI"
                    select new { o1, o1.o2, Shadow = EF.Property<DateTime?>(o1.o2, "OrderDate") },
                elementSorter: e => e.o1.o2.OrderID,
                entryCount: 6);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        [Trait("Impatient", "EFCore missing entries")]
        public override void Queryable_simple_anonymous_subquery()
        {
            //base.Queryable_simple_anonymous_subquery();

            AssertQuery<Customer>(
                cs => cs.Select(c => new { c }).Take(91).Select(a => a.c),
                entryCount: 91);
        }

        private static IEnumerable<TElement> ClientDefaultIfEmpty<TElement>(IEnumerable<TElement> source)
        {
            return source?.Count() == 0 ? new[] { default(TElement) } : source;
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        [Trait("Impatient", "Pessimistic tracking")]
        public override void Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ()
        {
            AssertQuery<Employee>(
                es => from e1 in es
                      join e2 in es on e1.EmployeeID equals e2.ReportsTo into grouping
                      from e2 in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                      select new { City1 = e1.City, City2 = e2 != null ? e2.City : null },
#pragma warning restore IDE0031 // Use null propagation
                elementSorter: e => e.City1 + " " + e.City2,
                entryCount: 9);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        [Trait("Impatient", "Pessimistic tracking")]
        public override void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from o in os
                    join c in cs on o.CustomerID equals c.CustomerID into grouping
                    from c in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new { Id1 = o.CustomerID, Id2 = c != null ? c.CustomerID : null },
#pragma warning restore IDE0031 // Use null propagation
                elementSorter: e => e.Id1 + " " + e.Id2,
                entryCount: 919);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        [Trait("Impatient", "Pessimistic tracking")]
        public override void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition1()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from o in os
                    join c in cs on new { o.CustomerID, o.OrderID } equals new { c.CustomerID, OrderID = 10000 } into grouping
                    from c in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new { Id1 = o.CustomerID, Id2 = c != null ? c.CustomerID : null },
#pragma warning restore IDE0031 // Use null propagation
                elementSorter: e => e.Id1 + " " + e.Id2,
                entryCount: 830);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        [Trait("Impatient", "Pessimistic tracking")]
        public override void No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from o in os
                    join c in cs on new { o.OrderID, o.CustomerID } equals new { OrderID = 10000, c.CustomerID } into grouping
                    from c in ClientDefaultIfEmpty(grouping)
#pragma warning disable IDE0031 // Use null propagation
                    select new { Id1 = o.CustomerID, Id2 = c != null ? c.CustomerID : null },
#pragma warning restore IDE0031 // Use null propagation
                elementSorter: e => e.Id1 + " " + e.Id2,
                entryCount: 830);
        }

        [Fact]
        [Trait("Impatient", "Overridden for sorting")]
        public override void GroupJoin_outer_projection3()
        {
            AssertQuery<Customer, Order>(
                (cs, os) => cs.GroupJoin(os, c => c.CustomerID, o => o.CustomerID, (c, g) => new { g = g.Select(o => o.CustomerID) }),
                elementSorter: e => ((IEnumerable<string>)e.g).FirstOrDefault(),
                elementAsserter: (e, a) => CollectionAsserter<string>(s => s)(e.g, a.g));
        }

        [Fact]
        [Trait("Impatient", "Overridden for sorting")]
        public override void GroupJoin_outer_projection4()
        {
            AssertQuery<Customer, Order>(
                (cs, os) => cs.GroupJoin(os, c => c.CustomerID, o => o.CustomerID, (c, g) => g.Select(o => o.CustomerID)),
                elementSorter: e => ((IEnumerable<string>)e).FirstOrDefault(),
                elementAsserter: CollectionAsserter<string>(s => s));
        }

        [Fact]
        [Trait("Impatient", "Overridden for sorting")]
        public override void Include_with_orderby_skip_preserves_ordering()
        {
            // Had to add the ThenBy call.

            AssertQuery<Customer>(
                cs => cs
                    .Include(c => c.Orders)
                    .Where(c => c.CustomerID != "VAFFE" && c.CustomerID != "DRACD")
                    .OrderBy(c => c.City)
                    .ThenBy(c => c.CustomerID)
                    .Skip(40)
                    .Take(5),
                entryCount: 48,
                assertOrder: true);
        }

        [Fact]
        [Trait("Impatient", "Overridden for sorting")]
        public override void GroupJoin_tracking_groups()
        {
            AssertQuery<Customer, Order>(
                (cs, os) =>
                    from c in cs
                    join o in os on c.CustomerID equals o.CustomerID into orders
                    select orders,
                elementSorter: os => ((IEnumerable<Order>)os).Select(o => o.CustomerID).FirstOrDefault(),
                elementAsserter: CollectionAsserter<Order>(o => o.OrderID),
                entryCount: 830);
        }

        [Fact]
        [Trait("Impatient", "Overridden for sorting")]
        public override void OrderBy_multiple()
        {
            // Enumerable.OrderBy is implemented with a stable sort algorithm
            // but Queryable.OrderBy does not offer the same guarantee, even though
            // EFCore implements theirs that way.
            AssertQuery<Customer>(
                cs =>
                    cs.OrderBy(c => c.CustomerID)
                        // ReSharper disable once MultipleOrderBy
                        .OrderBy(c => c.Country)
                        .Select(c => c.City),
                assertOrder: false);
        }

        [Fact]
        [Trait("Impatient", "Overridden for semantics")]
        public override void String_Contains_Literal()
        {
            // Overridden; EFCore SQL Server test does this too.
            AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Where_subquery_anon()
        {
            base.Where_subquery_anon();
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Where_subquery_anon_nested()
        {
            AssertQuery<Employee, Order, Customer>(
                (es, os, cs) =>
                    from t in (
                        from e in es.OrderBy(ee => ee.EmployeeID).Take(3).Select(e => new { e }).Where(e => e.e.City == "Seattle")
                        from o in os.OrderBy(oo => oo.OrderID).Take(5).Select(o => new { o })
                        select new { e, o })
                    from c in cs.Take(2).Select(c => new { c })
                    select new { t.e, t.o, c },
                entryCount: 8);
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Compare_two_collection_navigations_using_equals()
        {
            base.Compare_two_collection_navigations_using_equals();
        }

        [Fact]
        [Trait("Impatient", "Overridden for infrastructure")]
        public override void Method_with_constant_queryable_arg()
        {
            // Overridden because the base test performs checks using EF Core's own
            // query compilation cache, which does not apply for us

            using (var context = CreateContext())
            {
                var count = QueryableArgQuery(context, new[] { "ALFKI" }.AsQueryable()).Count();
                Assert.Equal(1, count);

                count = QueryableArgQuery(context, new[] { "FOO" }.AsQueryable()).Count();
                Assert.Equal(0, count);
            }
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Project_single_element_from_collection_with_OrderBy_over_navigation_Take_and_FirstOrDefault_2()
        {
            // Overridden to correct entry count
            AssertQuery<Order>(
                os => os.Where(o => o.OrderID < 10250)
                .Select(
                    o => o.OrderDetails.OrderBy(od => od.Product.ProductName).Take(1).FirstOrDefault()),
                entryCount: 2);
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Projection_when_arithmetic_mixed_subqueries()
        {
            // Overridden to correct entry count
            AssertQuery<Order, Employee>(
                (os, es) =>
                    from o in os.OrderBy(o => o.OrderID).Take(3).Select(o2 => new { o2, Mod = o2.OrderID % 2 })
                    from e in es.OrderBy(e => e.EmployeeID).Take(2).Select(e2 => new { e2, Square = e2.EmployeeID ^ 2 })
                    select new
                    {
                        Add = e.e2.EmployeeID + o.o2.OrderID,
                        e.Square,
                        e.e2,
                        Literal = 42,
                        o.o2,
                        o.Mod
                    },
                elementSorter: e => e.e2.EmployeeID + " " + e.o2.OrderID,
                entryCount: 5);
        }

        private static IQueryable<Customer> QueryableArgQuery(NorthwindContext context, IQueryable<string> ids)
        {
            return context.Customers.Where(c => ids.Contains(c.CustomerID));
        }

        #region utilities

        protected override void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

        private void AssertSql(string sql) => Fixture.AssertSql(sql);

        private void AssertSqlStartsWith(string sql) => Fixture.AssertSqlStartsWith(sql);

        #endregion
    }
}
