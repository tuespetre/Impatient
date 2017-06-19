using Impatient.Query;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors;
using Impatient.Tests.Northwind;
using Impatient.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Impatient.Tests
{
    [TestClass]
    public class NavigationTests
    {
        private static readonly ImpatientQueryProvider impatient;

        private static readonly StringBuilder commandLog = new StringBuilder();

        private static string SqlLog => commandLog.ToString();

        private static Expression CreateQueryExpression<TElement>()
        {
            var type = typeof(TElement);

            var annotation = type.GetTypeInfo().GetCustomAttribute<TableAttribute>();

            var table = new BaseTableExpression(
                annotation?.Schema ?? "dbo",
                annotation?.Name ?? type.Name,
                (annotation?.Name ?? type.Name).ToLower().First().ToString(),
                type);

            return new EnumerableRelationalQueryExpression(
                new SelectExpression(
                    new ServerProjectionExpression(
                        Expression.MemberInit(
                            Expression.New(type),
                            from property in type.GetTypeInfo().DeclaredProperties
                            where property.PropertyType.IsScalarType()
                            let nullable =
                                (property.PropertyType.IsConstructedGenericType
                                    && property.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                                || (!property.PropertyType.GetTypeInfo().IsValueType
                                    && property.GetCustomAttribute<RequiredAttribute>() == null)
                            let column = new SqlColumnExpression(table, property.Name, property.PropertyType, nullable)
                            select Expression.Bind(property, column))),
                    table));
        }

        private static LambdaExpression GetExpression<TSource, TResult>(Expression<Func<TSource, TResult>> expression)
        {
            return expression;
        }

        private class QueryContext
        {
            private readonly ImpatientQueryProvider impatient;

            public QueryContext(ImpatientQueryProvider impatient)
            {
                this.impatient = impatient;
            }

            public IQueryable<Customer> Customers => impatient.CreateQuery<Customer>(CreateQueryExpression<Customer>());

            public IQueryable<Order> Orders => impatient.CreateQuery<Order>(CreateQueryExpression<Order>());

            public IQueryable<OrderDetail> OrderDetails => impatient.CreateQuery<OrderDetail>(CreateQueryExpression<OrderDetail>());
        }

        static NavigationTests()
        {
            var expressionVisitorProvider =
                new DefaultImpatientExpressionVisitorProvider()
                    .WithNavigations(new[]
                    {
                        new NavigationDescriptor
                        {
                            Type = typeof(Customer),
                            Member = typeof(Customer).GetRuntimeProperty(nameof(Customer.Orders)),
                            OuterKeySelector = GetExpression((Customer c) => c.CustomerID),
                            InnerKeySelector = GetExpression((Order o) => o.CustomerID),
                            Expansion = CreateQueryExpression<Order>(),
                        },
                        new NavigationDescriptor
                        {
                            Type = typeof(Order),
                            Member = typeof(Order).GetRuntimeProperty(nameof(Order.Customer)),
                            OuterKeySelector = GetExpression((Order o) => o.CustomerID),
                            InnerKeySelector = GetExpression((Customer c) => c.CustomerID),
                            Expansion = CreateQueryExpression<Customer>(),
                        },
                        new NavigationDescriptor
                        {
                            Type = typeof(Order),
                            Member = typeof(Order).GetRuntimeProperty(nameof(Order.OrderDetails)),
                            OuterKeySelector = GetExpression((Order o) => o.OrderID),
                            InnerKeySelector = GetExpression((OrderDetail d) => d.OrderID),
                            Expansion = CreateQueryExpression<OrderDetail>(),
                        },
                        new NavigationDescriptor
                        {
                            Type = typeof(OrderDetail),
                            Member = typeof(OrderDetail).GetRuntimeProperty(nameof(OrderDetail.Order)),
                            OuterKeySelector = GetExpression((OrderDetail d) => d.OrderID),
                            InnerKeySelector = GetExpression((Order o) => o.OrderID),
                            Expansion = CreateQueryExpression<Order>(),
                        },
                    });

            impatient = new ImpatientQueryProvider(
                new TestImpatientConnectionFactory(@"Server=.\sqlexpress; Database=NORTHWND; Trusted_Connection=True"),
                new DefaultImpatientQueryCache(),
                expressionVisitorProvider)
            {
                DbCommandInterceptor = command =>
                {
                    if (commandLog.Length > 0)
                    {
                        commandLog.AppendLine().AppendLine();
                    }

                    commandLog.Append(command.CommandText);
                }
            };
        }

        [TestCleanup]
        public void Cleanup()
        {
            commandLog.Clear();
        }

        [TestMethod]
        public void Where_navigation_m1r()
        {
            var context = new QueryContext(impatient);

            var query = from o in context.Orders
                        where o.Customer.City == "Berlin"
                        select o;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[City] = N'Berlin'",
                SqlLog);
        }

        [TestMethod]
        public void Where_navigation_m1r_m1r()
        {
            var context = new QueryContext(impatient);

            var query = from d in context.OrderDetails
                        where d.Order.Customer.City == "Berlin"
                        select d;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [d].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[UnitPrice] AS [UnitPrice], [d].[Quantity] AS [Quantity], [d].[Discount] AS [Discount]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[City] = N'Berlin'",
                SqlLog);
        }

        [TestMethod]
        public void Where_navigation_m1r_m1r_repeated_access()
        {
            var context = new QueryContext(impatient);

            var query = from d in context.OrderDetails
                        where d.Order.Customer.City == "Berlin"
                        where d.Order.Customer.City != "Winnipeg"
                        let o = d.Order
                        let c = o.Customer
                        select new { d, o, c };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [d].[OrderID] AS [d.OrderID], [d].[ProductID] AS [d.ProductID], [d].[UnitPrice] AS [d.UnitPrice], [d].[Quantity] AS [d.Quantity], [d].[Discount] AS [d.Discount], [o].[OrderID] AS [o.OrderID], [o].[CustomerID] AS [o.CustomerID], [o].[EmployeeID] AS [o.EmployeeID], [o].[OrderDate] AS [o.OrderDate], [o].[RequiredDate] AS [o.RequiredDate], [o].[ShippedDate] AS [o.ShippedDate], [o].[ShipVia] AS [o.ShipVia], [o].[Freight] AS [o.Freight], [o].[ShipName] AS [o.ShipName], [o].[ShipAddress] AS [o.ShipAddress], [o].[ShipCity] AS [o.ShipCity], [o].[ShipRegion] AS [o.ShipRegion], [o].[ShipPostalCode] AS [o.ShipPostalCode], [o].[ShipCountry] AS [o.ShipCountry], [c].[CustomerID] AS [c.CustomerID], [c].[CompanyName] AS [c.CompanyName], [c].[ContactName] AS [c.ContactName], [c].[ContactTitle] AS [c.ContactTitle], [c].[Address] AS [c.Address], [c].[City] AS [c.City], [c].[Region] AS [c.Region], [c].[PostalCode] AS [c.PostalCode], [c].[Country] AS [c.Country], [c].[Phone] AS [c.Phone], [c].[Fax] AS [c.Fax]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE ([c].[City] = N'Berlin') AND (([c].[City] IS NULL OR ([c].[City] <> N'Winnipeg')))",
                SqlLog);
        }

        [TestMethod]
        public void Where2_navigation()
        {
            var context = new QueryContext(impatient);

            var query = context.Orders.Where((o, i) => o.Customer.City == "Berlin");

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[City] = N'Berlin'",
                SqlLog);
        }

        [TestMethod]
        public void Select_navigation_m1r()
        {
            var context = new QueryContext(impatient);

            var query = from o in context.Orders
                        select o.Customer;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [CustomerID], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Address] AS [Address], [c].[City] AS [City], [c].[Region] AS [Region], [c].[PostalCode] AS [PostalCode], [c].[Country] AS [Country], [c].[Phone] AS [Phone], [c].[Fax] AS [Fax]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]",
                SqlLog);
        }

        [TestMethod]
        public void Select_navigation_m1r_repeated_access()
        {
            var context = new QueryContext(impatient);

            var query = from o in context.Orders
                        select new
                        {
                            o.OrderID,
                            o.OrderDate,
                            o.Customer.CustomerID,
                            o.Customer.ContactName,
                            o.Customer.CompanyName,
                        };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [OrderID], [o].[OrderDate] AS [OrderDate], [c].[CustomerID] AS [CustomerID], [c].[ContactName] AS [ContactName], [c].[CompanyName] AS [CompanyName]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]",
                SqlLog);
        }

        [TestMethod]
        public void Select_navigation_m1r_m1r()
        {
            var context = new QueryContext(impatient);

            var query = from d in context.OrderDetails
                        select d.Order.Customer;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [CustomerID], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Address] AS [Address], [c].[City] AS [City], [c].[Region] AS [Region], [c].[PostalCode] AS [PostalCode], [c].[Country] AS [Country], [c].[Phone] AS [Phone], [c].[Fax] AS [Fax]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]",
                SqlLog);
        }

        [TestMethod]
        public void Select_navigation_12m_scalar()
        {
            var context = new QueryContext(impatient);

            var query = from c in context.Customers
                        select new
                        {
                            c.CustomerID,
                            count = c.Orders.Count(),
                            max = c.Orders.Max(o => o.OrderID),
                        };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [CustomerID], (
    SELECT COUNT(*)
    FROM [dbo].[Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [count], (
    SELECT MAX([o].[OrderID])
    FROM [dbo].[Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
) AS [max]
FROM [dbo].[Customers] AS [c]",
                SqlLog);
        }

        [TestMethod]
        public void SelectMany_navigation_12m()
        {
            var context = new QueryContext(impatient);

            var query = from c in context.Customers
                        from o in c.Orders
                        select new { c.CustomerID, o.OrderID };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [CustomerID], [o].[OrderID] AS [OrderID]
FROM [dbo].[Customers] AS [c]
INNER JOIN [dbo].[Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]",
                SqlLog);
        }

        [TestMethod]
        public void SelectMany_navigation_12m_2()
        {
            var context = new QueryContext(impatient);

            var query = context.Customers.SelectMany(c => c.Orders).Select(o => o.OrderID);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID]
FROM [dbo].[Customers] AS [c]
INNER JOIN [dbo].[Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]",
                SqlLog);
        }

        [TestMethod]
        public void SelectMany_navigation_12m_12m()
        {
            var context = new QueryContext(impatient);

            var query = from c in context.Customers
                        from o in c.Orders
                        from d in o.OrderDetails
                        select new { c.CustomerID, o.OrderID, d.ProductID };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [CustomerID], [o].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID]
FROM [dbo].[Customers] AS [c]
INNER JOIN [dbo].[Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]
INNER JOIN [dbo].[Order Details] AS [d] ON [o].[OrderID] = [d].[OrderID]",
                SqlLog);
        }

        [TestMethod]
        public void SelectMany_navigation_12m_DefaultIfEmpty()
        {
            var context = new QueryContext(impatient);

            var query = from c in context.Customers
                        from o in c.Orders.DefaultIfEmpty()
                        select new { c.CustomerID, o.OrderID };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [CustomerID], [o].[OrderID] AS [OrderID]
FROM [dbo].[Customers] AS [c]
LEFT JOIN (
    SELECT 0 AS [$empty], [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate], [o_0].[RequiredDate] AS [RequiredDate], [o_0].[ShippedDate] AS [ShippedDate], [o_0].[ShipVia] AS [ShipVia], [o_0].[Freight] AS [Freight], [o_0].[ShipName] AS [ShipName], [o_0].[ShipAddress] AS [ShipAddress], [o_0].[ShipCity] AS [ShipCity], [o_0].[ShipRegion] AS [ShipRegion], [o_0].[ShipPostalCode] AS [ShipPostalCode], [o_0].[ShipCountry] AS [ShipCountry]
    FROM [dbo].[Orders] AS [o_0]
) AS [o] ON [c].[CustomerID] = [o].[CustomerID]",
                SqlLog);
        }

        [TestMethod]
        public void SelectMany_navigation_12m_DefaultIfEmpty_12m_DefaultIfEmpty()
        {
            var context = new QueryContext(impatient);

            var query = from c in context.Customers
                        from o in c.Orders.DefaultIfEmpty()
                        from d in o.OrderDetails.DefaultIfEmpty()
                        select new { c.CustomerID, o.OrderID };

            query.ToList();

            // TODO: Eliminate the unnecessary null checking in the predicate
            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [CustomerID], [o].[OrderID] AS [OrderID]
FROM [dbo].[Customers] AS [c]
LEFT JOIN (
    SELECT 0 AS [$empty], [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate], [o_0].[RequiredDate] AS [RequiredDate], [o_0].[ShippedDate] AS [ShippedDate], [o_0].[ShipVia] AS [ShipVia], [o_0].[Freight] AS [Freight], [o_0].[ShipName] AS [ShipName], [o_0].[ShipAddress] AS [ShipAddress], [o_0].[ShipCity] AS [ShipCity], [o_0].[ShipRegion] AS [ShipRegion], [o_0].[ShipPostalCode] AS [ShipPostalCode], [o_0].[ShipCountry] AS [ShipCountry]
    FROM [dbo].[Orders] AS [o_0]
) AS [o] ON [c].[CustomerID] = [o].[CustomerID]
LEFT JOIN (
    SELECT 0 AS [$empty], [d].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[UnitPrice] AS [UnitPrice], [d].[Quantity] AS [Quantity], [d].[Discount] AS [Discount]
    FROM [dbo].[Order Details] AS [d]
) AS [d_0] ON (([o].[OrderID] IS NULL AND [d_0].[OrderID] IS NULL) OR ([o].[OrderID] = [d_0].[OrderID]))",
                SqlLog);
        }

        [TestMethod]
        public void OrderBy_navigation()
        {
            var context = new QueryContext(impatient);

            var query = from o in context.Orders
                        orderby o.Customer.ContactName
                        select o;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[ContactName] ASC",
                SqlLog);
        }

        [TestMethod]
        public void OrderByDescending_navigation()
        {
            var context = new QueryContext(impatient);

            var query = from o in context.Orders
                        orderby o.Customer.ContactName descending
                        select o;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[ContactName] DESC",
                SqlLog);
        }

        [TestMethod]
        public void ThenBy_navigation()
        {
            var context = new QueryContext(impatient);

            var query = from o in context.Orders
                        orderby o.Customer.City, o.Customer.ContactName
                        select o;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[City] ASC, [c].[ContactName] ASC",
                SqlLog);
        }

        [TestMethod]
        public void ThenByDescending_navigation()
        {
            var context = new QueryContext(impatient);

            var query = from o in context.Orders
                        orderby o.Customer.City, o.Customer.ContactName descending
                        select o;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[City] ASC, [c].[ContactName] DESC",
                SqlLog);
        }

        [TestMethod]
        public void SkipWhile1_navigation()
        {
            var context = new QueryContext(impatient);

            var query = context.Orders.SkipWhile(o => o.Customer.City != "Berlin");

            query.ToList();

            Assert.AreEqual(
                @"SELECT [t].[OrderID] AS [OrderID], [t].[CustomerID] AS [CustomerID], [t].[EmployeeID] AS [EmployeeID], [t].[OrderDate] AS [OrderDate], [t].[RequiredDate] AS [RequiredDate], [t].[ShippedDate] AS [ShippedDate], [t].[ShipVia] AS [ShipVia], [t].[Freight] AS [Freight], [t].[ShipName] AS [ShipName], [t].[ShipAddress] AS [ShipAddress], [t].[ShipCity] AS [ShipCity], [t].[ShipRegion] AS [ShipRegion], [t].[ShipPostalCode] AS [ShipPostalCode], [t].[ShipCountry] AS [ShipCountry]
FROM (
    SELECT [o].[OrderID] AS [OrderID], [c].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Address] AS [Address], [c].[City] AS [City], [c].[Region] AS [Region], [c].[PostalCode] AS [PostalCode], [c].[Country] AS [Country], [c].[Phone] AS [Phone], [c].[Fax] AS [Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
    FROM [dbo].[Orders] AS [o]
    INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
) AS [t]
WHERE [t].[$rownumber] >= (
    SELECT COALESCE(MIN([t_0].[$rownumber]), 0)
    FROM (
        SELECT [o_0].[OrderID] AS [OrderID], [c_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate], [o_0].[RequiredDate] AS [RequiredDate], [o_0].[ShippedDate] AS [ShippedDate], [o_0].[ShipVia] AS [ShipVia], [o_0].[Freight] AS [Freight], [o_0].[ShipName] AS [ShipName], [o_0].[ShipAddress] AS [ShipAddress], [o_0].[ShipCity] AS [ShipCity], [o_0].[ShipRegion] AS [ShipRegion], [o_0].[ShipPostalCode] AS [ShipPostalCode], [o_0].[ShipCountry] AS [ShipCountry], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[Region] AS [Region], [c_0].[PostalCode] AS [PostalCode], [c_0].[Country] AS [Country], [c_0].[Phone] AS [Phone], [c_0].[Fax] AS [Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
        FROM [dbo].[Orders] AS [o_0]
        INNER JOIN [dbo].[Customers] AS [c_0] ON [o_0].[CustomerID] = [c_0].[CustomerID]
    ) AS [t_0]
    WHERE [t_0].[City] = N'Berlin'
)",
                SqlLog);
        }

        [TestMethod]
        public void SkipWhile2_navigation()
        {
            var context = new QueryContext(impatient);

            var query = context.Orders.SkipWhile((o, i) => o.Customer.City != "Berlin");

            query.ToList();

            Assert.AreEqual(
                @"SELECT [t].[OrderID] AS [OrderID], [t].[CustomerID] AS [CustomerID], [t].[EmployeeID] AS [EmployeeID], [t].[OrderDate] AS [OrderDate], [t].[RequiredDate] AS [RequiredDate], [t].[ShippedDate] AS [ShippedDate], [t].[ShipVia] AS [ShipVia], [t].[Freight] AS [Freight], [t].[ShipName] AS [ShipName], [t].[ShipAddress] AS [ShipAddress], [t].[ShipCity] AS [ShipCity], [t].[ShipRegion] AS [ShipRegion], [t].[ShipPostalCode] AS [ShipPostalCode], [t].[ShipCountry] AS [ShipCountry]
FROM (
    SELECT [o].[OrderID] AS [OrderID], [c].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Address] AS [Address], [c].[City] AS [City], [c].[Region] AS [Region], [c].[PostalCode] AS [PostalCode], [c].[Country] AS [Country], [c].[Phone] AS [Phone], [c].[Fax] AS [Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
    FROM [dbo].[Orders] AS [o]
    INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
) AS [t]
WHERE [t].[$rownumber] >= (
    SELECT COALESCE(MIN([t_0].[$rownumber]), 0)
    FROM (
        SELECT [o_0].[OrderID] AS [OrderID], [c_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate], [o_0].[RequiredDate] AS [RequiredDate], [o_0].[ShippedDate] AS [ShippedDate], [o_0].[ShipVia] AS [ShipVia], [o_0].[Freight] AS [Freight], [o_0].[ShipName] AS [ShipName], [o_0].[ShipAddress] AS [ShipAddress], [o_0].[ShipCity] AS [ShipCity], [o_0].[ShipRegion] AS [ShipRegion], [o_0].[ShipPostalCode] AS [ShipPostalCode], [o_0].[ShipCountry] AS [ShipCountry], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[Region] AS [Region], [c_0].[PostalCode] AS [PostalCode], [c_0].[Country] AS [Country], [c_0].[Phone] AS [Phone], [c_0].[Fax] AS [Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
        FROM [dbo].[Orders] AS [o_0]
        INNER JOIN [dbo].[Customers] AS [c_0] ON [o_0].[CustomerID] = [c_0].[CustomerID]
    ) AS [t_0]
    WHERE [t_0].[City] = N'Berlin'
)",
                SqlLog);
        }

        [TestMethod]
        public void TakeWhile1_navigation()
        {
            var context = new QueryContext(impatient);

            var query = context.Orders.TakeWhile(o => o.Customer.City != "Berlin");

            query.ToList();

            Assert.AreEqual(
                @"SELECT [t].[OrderID] AS [OrderID], [t].[CustomerID] AS [CustomerID], [t].[EmployeeID] AS [EmployeeID], [t].[OrderDate] AS [OrderDate], [t].[RequiredDate] AS [RequiredDate], [t].[ShippedDate] AS [ShippedDate], [t].[ShipVia] AS [ShipVia], [t].[Freight] AS [Freight], [t].[ShipName] AS [ShipName], [t].[ShipAddress] AS [ShipAddress], [t].[ShipCity] AS [ShipCity], [t].[ShipRegion] AS [ShipRegion], [t].[ShipPostalCode] AS [ShipPostalCode], [t].[ShipCountry] AS [ShipCountry]
FROM (
    SELECT [o].[OrderID] AS [OrderID], [c].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Address] AS [Address], [c].[City] AS [City], [c].[Region] AS [Region], [c].[PostalCode] AS [PostalCode], [c].[Country] AS [Country], [c].[Phone] AS [Phone], [c].[Fax] AS [Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
    FROM [dbo].[Orders] AS [o]
    INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
) AS [t]
WHERE [t].[$rownumber] < (
    SELECT COALESCE(MIN([t_0].[$rownumber]), [t].[$rownumber] + 1)
    FROM (
        SELECT [o_0].[OrderID] AS [OrderID], [c_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate], [o_0].[RequiredDate] AS [RequiredDate], [o_0].[ShippedDate] AS [ShippedDate], [o_0].[ShipVia] AS [ShipVia], [o_0].[Freight] AS [Freight], [o_0].[ShipName] AS [ShipName], [o_0].[ShipAddress] AS [ShipAddress], [o_0].[ShipCity] AS [ShipCity], [o_0].[ShipRegion] AS [ShipRegion], [o_0].[ShipPostalCode] AS [ShipPostalCode], [o_0].[ShipCountry] AS [ShipCountry], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[Region] AS [Region], [c_0].[PostalCode] AS [PostalCode], [c_0].[Country] AS [Country], [c_0].[Phone] AS [Phone], [c_0].[Fax] AS [Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
        FROM [dbo].[Orders] AS [o_0]
        INNER JOIN [dbo].[Customers] AS [c_0] ON [o_0].[CustomerID] = [c_0].[CustomerID]
    ) AS [t_0]
    WHERE [t_0].[City] = N'Berlin'
)",
                SqlLog);
        }

        [TestMethod]
        public void TakeWhile2_navigation()
        {
            var context = new QueryContext(impatient);

            var query = context.Orders.TakeWhile((o, i) => o.Customer.City != "Berlin");

            query.ToList();

            Assert.AreEqual(
                @"SELECT [t].[OrderID] AS [OrderID], [t].[CustomerID] AS [CustomerID], [t].[EmployeeID] AS [EmployeeID], [t].[OrderDate] AS [OrderDate], [t].[RequiredDate] AS [RequiredDate], [t].[ShippedDate] AS [ShippedDate], [t].[ShipVia] AS [ShipVia], [t].[Freight] AS [Freight], [t].[ShipName] AS [ShipName], [t].[ShipAddress] AS [ShipAddress], [t].[ShipCity] AS [ShipCity], [t].[ShipRegion] AS [ShipRegion], [t].[ShipPostalCode] AS [ShipPostalCode], [t].[ShipCountry] AS [ShipCountry]
FROM (
    SELECT [o].[OrderID] AS [OrderID], [c].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Address] AS [Address], [c].[City] AS [City], [c].[Region] AS [Region], [c].[PostalCode] AS [PostalCode], [c].[Country] AS [Country], [c].[Phone] AS [Phone], [c].[Fax] AS [Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
    FROM [dbo].[Orders] AS [o]
    INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
) AS [t]
WHERE [t].[$rownumber] < (
    SELECT COALESCE(MIN([t_0].[$rownumber]), [t].[$rownumber] + 1)
    FROM (
        SELECT [o_0].[OrderID] AS [OrderID], [c_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate], [o_0].[RequiredDate] AS [RequiredDate], [o_0].[ShippedDate] AS [ShippedDate], [o_0].[ShipVia] AS [ShipVia], [o_0].[Freight] AS [Freight], [o_0].[ShipName] AS [ShipName], [o_0].[ShipAddress] AS [ShipAddress], [o_0].[ShipCity] AS [ShipCity], [o_0].[ShipRegion] AS [ShipRegion], [o_0].[ShipPostalCode] AS [ShipPostalCode], [o_0].[ShipCountry] AS [ShipCountry], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[Region] AS [Region], [c_0].[PostalCode] AS [PostalCode], [c_0].[Country] AS [Country], [c_0].[Phone] AS [Phone], [c_0].[Fax] AS [Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
        FROM [dbo].[Orders] AS [o_0]
        INNER JOIN [dbo].[Customers] AS [c_0] ON [o_0].[CustomerID] = [c_0].[CustomerID]
    ) AS [t_0]
    WHERE [t_0].[City] = N'Berlin'
)",
                SqlLog);
        }

        [TestMethod]
        public void All_navigation()
        {
            var context = new QueryContext(impatient);

            var result = context.Orders.All(o => o.Customer.ContactName != null);

            Assert.AreEqual(
                @"SELECT CAST((CASE WHEN NOT EXISTS (
    SELECT 1
    FROM [dbo].[Orders] AS [o]
    INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    WHERE [c].[ContactName] IS NULL
) THEN 1 ELSE 0 END) AS BIT)",
                SqlLog);
        }

        [TestMethod]
        public void Any2_navigation()
        {
            var context = new QueryContext(impatient);

            var result = context.Orders.Any(o => o.Customer.ContactName != null);

            Assert.AreEqual(
                @"SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM [dbo].[Orders] AS [o]
    INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    WHERE [c].[ContactName] IS NOT NULL
) THEN 1 ELSE 0 END) AS BIT)",
                SqlLog);
        }

        [TestMethod]
        public void Count2_navigation()
        {
            var context = new QueryContext(impatient);

            var result = context.Orders.Count(o => o.Customer.ContactName != null);

            Assert.AreEqual(
                @"SELECT COUNT(*)
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[ContactName] IS NOT NULL",
                SqlLog);
        }

        [TestMethod]
        public void LongCount2_navigation()
        {
            var context = new QueryContext(impatient);

            var result = context.Orders.LongCount(o => o.Customer.ContactName != null);

            Assert.AreEqual(
                @"SELECT COUNT_BIG(*)
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[ContactName] IS NOT NULL",
                SqlLog);
        }

        [TestMethod]
        public void First2_navigation()
        {
            var context = new QueryContext(impatient);

            var result = context.Orders.First(o => o.Customer.ContactName != null);

            Assert.AreEqual(
                @"SELECT TOP (1) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[ContactName] IS NOT NULL",
                SqlLog);
        }

        [TestMethod]
        public void FirstOrDefault2_navigation()
        {
            var context = new QueryContext(impatient);

            var result = context.Orders.FirstOrDefault(o => o.Customer.ContactName != null);

            Assert.AreEqual(
                @"SELECT TOP (1) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[ContactName] IS NOT NULL",
                SqlLog);
        }

        [TestMethod]
        public void Last2_navigation()
        {
            var context = new QueryContext(impatient);

            var result = context.Orders.OrderBy(o => o.OrderDate).Last(o => o.Customer.ContactName != null);

            Assert.AreEqual(
                @"SELECT TOP (1) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[ContactName] IS NOT NULL
ORDER BY [o].[OrderDate] DESC",
                SqlLog);
        }

        [TestMethod]
        public void LastOrDefault2_navigation()
        {
            var context = new QueryContext(impatient);

            var result = context.Orders.OrderBy(o => o.OrderDate).LastOrDefault(o => o.Customer.ContactName != null);

            Assert.AreEqual(
                @"SELECT TOP (1) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[ContactName] IS NOT NULL
ORDER BY [o].[OrderDate] DESC",
                SqlLog);
        }

        [TestMethod]
        public void Single2_navigation()
        {
            var context = new QueryContext(impatient);

            try
            {
                context.Orders.Single(o => o.Customer.ContactName != null);
            }
            catch
            {
            }

            Assert.AreEqual(
                @"SELECT TOP (2) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[ContactName] IS NOT NULL",
                SqlLog);
        }

        [TestMethod]
        public void SingleOrDefault2_navigation()
        {
            var context = new QueryContext(impatient);

            try
            {
                context.Orders.SingleOrDefault(o => o.Customer.ContactName != null);
            }
            catch
            {
            }

            Assert.AreEqual(
                @"SELECT TOP (2) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[ContactName] IS NOT NULL",
                SqlLog);
        }

        [TestMethod]
        public void Average_navigation()
        {
            var context = new QueryContext(impatient);

            var result = context.OrderDetails.Average(d => d.Order.Freight);

            Assert.AreEqual(
                @"SELECT AVG(CAST([o].[Freight] AS decimal))
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]",
                SqlLog);
        }

        [TestMethod]
        public void Max_navigation()
        {
            var context = new QueryContext(impatient);

            var result = context.OrderDetails.Max(d => d.Order.Freight);

            Assert.AreEqual(
                @"SELECT MAX([o].[Freight])
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]",
                SqlLog);
        }

        [TestMethod]
        public void Min_navigation()
        {
            var context = new QueryContext(impatient);

            var result = context.OrderDetails.Min(d => d.Order.Freight);

            Assert.AreEqual(
                @"SELECT MIN([o].[Freight])
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]",
                SqlLog);
        }

        [TestMethod]
        public void Sum_navigation()
        {
            var context = new QueryContext(impatient);

            var result = context.OrderDetails.Sum(d => d.Order.Freight);

            Assert.AreEqual(
                @"SELECT SUM([o].[Freight])
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]",
                SqlLog);
        }

        [TestMethod]
        public void Cast_navigation()
        {
            var context = new QueryContext(impatient);

            var query = context.OrderDetails.Select(d => d.Order).Cast<Order>().Select(o => o.OrderDate);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderDate]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]",
                SqlLog);
        }

        [TestMethod]
        public void OfType_navigation()
        {
            var context = new QueryContext(impatient);

            var query = context.OrderDetails.Select(d => d.Order).Cast<Order>().Select(o => o.OrderDate);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderDate]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]",
                SqlLog);
        }

        [TestMethod]
        public void Reverse_navigation()
        {
            var context = new QueryContext(impatient);

            var query = context.OrderDetails.OrderBy(d => d.UnitPrice).Select(d => d.Order).Reverse().Select(o => o.OrderDate);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderDate]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
ORDER BY [d].[UnitPrice] DESC",
                SqlLog);
        }

        [TestMethod]
        public void Skip_navigation()
        {
            var context = new QueryContext(impatient);

            var query = context.OrderDetails.OrderBy(d => d.UnitPrice).Select(d => d.Order).Skip(1).Select(o => o.OrderDate);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderDate]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
ORDER BY [d].[UnitPrice] ASC
OFFSET 1 ROWS",
                SqlLog);
        }

        [TestMethod]
        public void Take_navigation()
        {
            var context = new QueryContext(impatient);

            var query = context.OrderDetails.Select(d => d.Order).Take(1).Select(o => o.OrderDate);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (1) [o].[OrderDate]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]",
                SqlLog);
        }

        [TestMethod]
        public void Join_navigation()
        {
            var context = new QueryContext(impatient);

            var query = from o1 in context.Orders
                        join o2 in context.Orders on o1.Customer.City equals o2.Customer.City
                        select new { o1, o2 };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o1].[OrderID] AS [o1.OrderID], [o1].[CustomerID] AS [o1.CustomerID], [o1].[EmployeeID] AS [o1.EmployeeID], [o1].[OrderDate] AS [o1.OrderDate], [o1].[RequiredDate] AS [o1.RequiredDate], [o1].[ShippedDate] AS [o1.ShippedDate], [o1].[ShipVia] AS [o1.ShipVia], [o1].[Freight] AS [o1.Freight], [o1].[ShipName] AS [o1.ShipName], [o1].[ShipAddress] AS [o1.ShipAddress], [o1].[ShipCity] AS [o1.ShipCity], [o1].[ShipRegion] AS [o1.ShipRegion], [o1].[ShipPostalCode] AS [o1.ShipPostalCode], [o1].[ShipCountry] AS [o1.ShipCountry], [t].[OrderID] AS [o2.OrderID], [t].[CustomerID] AS [o2.CustomerID], [t].[EmployeeID] AS [o2.EmployeeID], [t].[OrderDate] AS [o2.OrderDate], [t].[RequiredDate] AS [o2.RequiredDate], [t].[ShippedDate] AS [o2.ShippedDate], [t].[ShipVia] AS [o2.ShipVia], [t].[Freight] AS [o2.Freight], [t].[ShipName] AS [o2.ShipName], [t].[ShipAddress] AS [o2.ShipAddress], [t].[ShipCity] AS [o2.ShipCity], [t].[ShipRegion] AS [o2.ShipRegion], [t].[ShipPostalCode] AS [o2.ShipPostalCode], [t].[ShipCountry] AS [o2.ShipCountry]
FROM [dbo].[Orders] AS [o1]
INNER JOIN [dbo].[Customers] AS [c] ON [o1].[CustomerID] = [c].[CustomerID]
INNER JOIN (
    SELECT [o2].[OrderID] AS [OrderID], [c_0].[CustomerID] AS [CustomerID], [o2].[EmployeeID] AS [EmployeeID], [o2].[OrderDate] AS [OrderDate], [o2].[RequiredDate] AS [RequiredDate], [o2].[ShippedDate] AS [ShippedDate], [o2].[ShipVia] AS [ShipVia], [o2].[Freight] AS [Freight], [o2].[ShipName] AS [ShipName], [o2].[ShipAddress] AS [ShipAddress], [o2].[ShipCity] AS [ShipCity], [o2].[ShipRegion] AS [ShipRegion], [o2].[ShipPostalCode] AS [ShipPostalCode], [o2].[ShipCountry] AS [ShipCountry], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[Region] AS [Region], [c_0].[PostalCode] AS [PostalCode], [c_0].[Country] AS [Country], [c_0].[Phone] AS [Phone], [c_0].[Fax] AS [Fax]
    FROM [dbo].[Orders] AS [o2]
    INNER JOIN [dbo].[Customers] AS [c_0] ON [o2].[CustomerID] = [c_0].[CustomerID]
) AS [t] ON [c].[City] = [t].[City]",
                SqlLog);
        }

        [TestMethod]
        public void Join_navigation_repeated_access()
        {
            var context = new QueryContext(impatient);

            var query = from o1 in context.Orders
                        join o2 in context.Orders on o1.Customer.City equals o2.Customer.City
                        select new { c1 = o1.Customer.CustomerID, c2 = o2.Customer.CustomerID };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [c1], [t].[CustomerID] AS [c2]
FROM [dbo].[Orders] AS [o1]
INNER JOIN [dbo].[Customers] AS [c] ON [o1].[CustomerID] = [c].[CustomerID]
INNER JOIN (
    SELECT [o2].[OrderID] AS [OrderID], [c_0].[CustomerID] AS [CustomerID], [o2].[EmployeeID] AS [EmployeeID], [o2].[OrderDate] AS [OrderDate], [o2].[RequiredDate] AS [RequiredDate], [o2].[ShippedDate] AS [ShippedDate], [o2].[ShipVia] AS [ShipVia], [o2].[Freight] AS [Freight], [o2].[ShipName] AS [ShipName], [o2].[ShipAddress] AS [ShipAddress], [o2].[ShipCity] AS [ShipCity], [o2].[ShipRegion] AS [ShipRegion], [o2].[ShipPostalCode] AS [ShipPostalCode], [o2].[ShipCountry] AS [ShipCountry], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[Region] AS [Region], [c_0].[PostalCode] AS [PostalCode], [c_0].[Country] AS [Country], [c_0].[Phone] AS [Phone], [c_0].[Fax] AS [Fax]
    FROM [dbo].[Orders] AS [o2]
    INNER JOIN [dbo].[Customers] AS [c_0] ON [o2].[CustomerID] = [c_0].[CustomerID]
) AS [t] ON [c].[City] = [t].[City]",
                SqlLog);
        }

        [TestMethod]
        public void GroupJoin_navigation()
        {
            var context = new QueryContext(impatient);

            var query = from o1 in context.Orders
                        join o2 in context.Orders on o1.Customer.City equals o2.Customer.City into o2g
                        select new { o1, count = o2g.Count() };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o1].[OrderID] AS [o1.OrderID], [o1].[CustomerID] AS [o1.CustomerID], [o1].[EmployeeID] AS [o1.EmployeeID], [o1].[OrderDate] AS [o1.OrderDate], [o1].[RequiredDate] AS [o1.RequiredDate], [o1].[ShippedDate] AS [o1.ShippedDate], [o1].[ShipVia] AS [o1.ShipVia], [o1].[Freight] AS [o1.Freight], [o1].[ShipName] AS [o1.ShipName], [o1].[ShipAddress] AS [o1.ShipAddress], [o1].[ShipCity] AS [o1.ShipCity], [o1].[ShipRegion] AS [o1.ShipRegion], [o1].[ShipPostalCode] AS [o1.ShipPostalCode], [o1].[ShipCountry] AS [o1.ShipCountry], (
    SELECT COUNT(*)
    FROM [dbo].[Orders] AS [o2]
    INNER JOIN [dbo].[Customers] AS [c] ON [o2].[CustomerID] = [c].[CustomerID]
    WHERE (([c_0].[City] IS NULL AND [c].[City] IS NULL) OR ([c_0].[City] = [c].[City]))
) AS [count]
FROM [dbo].[Orders] AS [o1]
INNER JOIN [dbo].[Customers] AS [c_0] ON [o1].[CustomerID] = [c_0].[CustomerID]",
                SqlLog);
        }

        /*
         * GroupBy test matrix: 
         * 
         * - Overloads:
         * 
         *      - GroupBy1 (key)
         *      - GroupBy2 (key/element)
         *      - GroupBy3 (key/result)
         *      - GroupBy4 (key/element/result)
         * 
         * - Each overload can be used
         * 
         *      - 'intact' (the IGrouping is materialized), or 
         *      - 'aggregate' (the IGrouping or IEnumerable is collapsed into aggregate functions.)
         * 
         * - Each overload has different possibilities based on which selectors contain navigations
         * 
         *      - key: 1
         *      - key/element: 3
         *      - key/result: 3
         *      - key/element/result: 7
         *      
         *  - Total of 28 basic tests:
         *  
         *      - GroupBy1_navigation_intact_key
         *      - GroupBy1_navigation_aggregate_key
         *      - GroupBy2_navigation_intact_key
         *      - GroupBy2_navigation_intact_element
         *      - GroupBy2_navigation_intact_key_element
         *      - GroupBy2_navigation_aggregate_key
         *      - GroupBy2_navigation_aggregate_element
         *      - GroupBy2_navigation_aggregate_key_element
         *      - GroupBy3_navigation_intact_key
         *      - GroupBy3_navigation_intact_element
         *      - GroupBy3_navigation_intact_key_element
         *      - GroupBy3_navigation_aggregate_key
         *      - GroupBy3_navigation_aggregate_element
         *      - GroupBy3_navigation_aggregate_key_element
         *      - GroupBy4_navigation_intact_key
         *      - GroupBy4_navigation_intact_element
         *      - GroupBy4_navigation_intact_result
         *      - GroupBy4_navigation_intact_key_element
         *      - GroupBy4_navigation_intact_key_result
         *      - GroupBy4_navigation_intact_element_result
         *      - GroupBy4_navigation_intact_key_element_result
         *      - GroupBy4_navigation_aggregate_key
         *      - GroupBy4_navigation_aggregate_element
         *      - GroupBy4_navigation_aggregate_result
         *      - GroupBy4_navigation_aggregate_key_element
         *      - GroupBy4_navigation_aggregate_key_result
         *      - GroupBy4_navigation_aggregate_element_result
         *      - GroupBy4_navigation_aggregate_key_element_result
         */

        [TestMethod]
        public void GroupBy1_navigation_intact_key()
        {
            var context = new QueryContext(impatient);

            var query = context.Orders.GroupBy(o => o.Customer.City);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[City] AS [Key], (
    SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
    FROM [dbo].[Orders] AS [o]
    INNER JOIN [dbo].[Customers] AS [c_0] ON [o].[CustomerID] = [c_0].[CustomerID]
    WHERE (([c].[City] IS NULL AND [c_0].[City] IS NULL) OR ([c].[City] = [c_0].[City]))
    FOR JSON PATH
) AS [Elements]
FROM [dbo].[Orders] AS [o_0]
INNER JOIN [dbo].[Customers] AS [c] ON [o_0].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[City]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy1_navigation_aggregate_key()
        {
            var context = new QueryContext(impatient);

            var query 
                = context.Orders
                    .GroupBy(o => o.Customer.City)
                    .Select(g => new
                    {
                        City = g.Key,
                        LastOrderDate = g.Max(o => o.OrderDate)
                    });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[City] AS [City], MAX([o].[OrderDate]) AS [LastOrderDate]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[City]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy2_navigation_intact_key()
        {
            var context = new QueryContext(impatient);
            
            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order.OrderDate,
                        d => new { d.Quantity });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderDate] AS [Key], (
    SELECT [d].[Quantity] AS [Quantity]
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d].[OrderID] = [o_0].[OrderID]
    WHERE (([o].[OrderDate] IS NULL AND [o_0].[OrderDate] IS NULL) OR ([o].[OrderDate] = [o_0].[OrderDate]))
    FOR JSON PATH
) AS [Elements]
FROM [dbo].[Order Details] AS [d_0]
INNER JOIN [dbo].[Orders] AS [o] ON [d_0].[OrderID] = [o].[OrderID]
GROUP BY [o].[OrderDate]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy2_navigation_intact_element()
        {
            var context = new QueryContext(impatient);
            
            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.ProductID,
                        d => new { d.Order.Customer.ContactName });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [d].[ProductID] AS [Key], (
    SELECT [c].[ContactName] AS [ContactName]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o] ON [d_0].[OrderID] = [o].[OrderID]
    INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    WHERE [d].[ProductID] = [d_0].[ProductID]
    FOR JSON PATH
) AS [Elements]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o_0] ON [d].[OrderID] = [o_0].[OrderID]
INNER JOIN [dbo].[Customers] AS [c_0] ON [o_0].[CustomerID] = [c_0].[CustomerID]
GROUP BY [d].[ProductID]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy2_navigation_intact_key_element()
        {
            var context = new QueryContext(impatient);
            
            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order.OrderDate,
                        d => new { d.Order.Customer.ContactName });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderDate] AS [Key], (
    SELECT [c].[ContactName] AS [ContactName]
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d].[OrderID] = [o_0].[OrderID]
    INNER JOIN [dbo].[Customers] AS [c] ON [o_0].[CustomerID] = [c].[CustomerID]
    WHERE (([o].[OrderDate] IS NULL AND [o_0].[OrderDate] IS NULL) OR ([o].[OrderDate] = [o_0].[OrderDate]))
    FOR JSON PATH
) AS [Elements]
FROM [dbo].[Order Details] AS [d_0]
INNER JOIN [dbo].[Orders] AS [o] ON [d_0].[OrderID] = [o].[OrderID]
INNER JOIN [dbo].[Customers] AS [c_0] ON [o].[CustomerID] = [c_0].[CustomerID]
GROUP BY [o].[OrderDate]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy2_navigation_aggregate_key()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order.OrderDate,
                        d => new { d.Quantity })
                    .Select(
                        g => new
                        {
                            g.Key,
                            max = g.Max(x => x.Quantity)
                        });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderDate] AS [Key], MAX([d].[Quantity]) AS [max]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
GROUP BY [o].[OrderDate]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy2_navigation_aggregate_element()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.ProductID,
                        d => new { d.Order.Customer.ContactName, d.Order.OrderDate })
                    .Select(
                        g => new
                        {
                            g.Key,
                            max = g.Max(x => x.OrderDate),
                        });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [d].[ProductID] AS [Key], MAX([o].[OrderDate]) AS [max]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [d].[ProductID]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy2_navigation_aggregate_key_element()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order.OrderDate,
                        d => new { d.Order.Customer.ContactName, d.Order.OrderDate })
                    .Select(
                        g => new
                        {
                            g.Key,
                            max = g.Max(x => x.OrderDate),
                        });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderDate] AS [Key], MAX([o].[OrderDate]) AS [max]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [o].[OrderDate]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy3_navigation_intact_key()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order,
                        (k, e) => new { k.OrderID, e });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [OrderID], [o].[OrderID] AS [e.Key.OrderID], [o].[CustomerID] AS [e.Key.CustomerID], [o].[EmployeeID] AS [e.Key.EmployeeID], [o].[OrderDate] AS [e.Key.OrderDate], [o].[RequiredDate] AS [e.Key.RequiredDate], [o].[ShippedDate] AS [e.Key.ShippedDate], [o].[ShipVia] AS [e.Key.ShipVia], [o].[Freight] AS [e.Key.Freight], [o].[ShipName] AS [e.Key.ShipName], [o].[ShipAddress] AS [e.Key.ShipAddress], [o].[ShipCity] AS [e.Key.ShipCity], [o].[ShipRegion] AS [e.Key.ShipRegion], [o].[ShipPostalCode] AS [e.Key.ShipPostalCode], [o].[ShipCountry] AS [e.Key.ShipCountry], (
    SELECT [d].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[UnitPrice] AS [UnitPrice], [d].[Quantity] AS [Quantity], [d].[Discount] AS [Discount]
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d].[OrderID] = [o_0].[OrderID]
    WHERE (((([o].[OrderID] = [o_0].[OrderID]) AND ((([o].[CustomerID] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[CustomerID] = [o_0].[CustomerID])))) AND (((([o].[EmployeeID] IS NULL AND [o_0].[EmployeeID] IS NULL) OR ([o].[EmployeeID] = [o_0].[EmployeeID]))) AND ((([o].[OrderDate] IS NULL AND [o_0].[OrderDate] IS NULL) OR ([o].[OrderDate] = [o_0].[OrderDate]))))) AND ((((([o].[RequiredDate] IS NULL AND [o_0].[RequiredDate] IS NULL) OR ([o].[RequiredDate] = [o_0].[RequiredDate]))) AND ((([o].[ShippedDate] IS NULL AND [o_0].[ShippedDate] IS NULL) OR ([o].[ShippedDate] = [o_0].[ShippedDate])))) AND (((([o].[ShipVia] IS NULL AND [o_0].[ShipVia] IS NULL) OR ([o].[ShipVia] = [o_0].[ShipVia]))) AND ((([o].[Freight] IS NULL AND [o_0].[Freight] IS NULL) OR ([o].[Freight] = [o_0].[Freight])))))) AND (((((([o].[ShipName] IS NULL AND [o_0].[ShipName] IS NULL) OR ([o].[ShipName] = [o_0].[ShipName]))) AND ((([o].[ShipAddress] IS NULL AND [o_0].[ShipAddress] IS NULL) OR ([o].[ShipAddress] = [o_0].[ShipAddress])))) AND (((([o].[ShipCity] IS NULL AND [o_0].[ShipCity] IS NULL) OR ([o].[ShipCity] = [o_0].[ShipCity]))) AND ((([o].[ShipRegion] IS NULL AND [o_0].[ShipRegion] IS NULL) OR ([o].[ShipRegion] = [o_0].[ShipRegion]))))) AND (((([o].[ShipPostalCode] IS NULL AND [o_0].[ShipPostalCode] IS NULL) OR ([o].[ShipPostalCode] = [o_0].[ShipPostalCode]))) AND ((([o].[ShipCountry] IS NULL AND [o_0].[ShipCountry] IS NULL) OR ([o].[ShipCountry] = [o_0].[ShipCountry])))))
    FOR JSON PATH
) AS [e.Elements]
FROM [dbo].[Order Details] AS [d_0]
INNER JOIN [dbo].[Orders] AS [o] ON [d_0].[OrderID] = [o].[OrderID]
GROUP BY [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o].[RequiredDate], [o].[ShippedDate], [o].[ShipVia], [o].[Freight], [o].[ShipName], [o].[ShipAddress], [o].[ShipCity], [o].[ShipRegion], [o].[ShipPostalCode], [o].[ShipCountry]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy3_navigation_intact_result()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d,
                        (k, e) => new { k.Order, e });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [Order.OrderID], [o].[CustomerID] AS [Order.CustomerID], [o].[EmployeeID] AS [Order.EmployeeID], [o].[OrderDate] AS [Order.OrderDate], [o].[RequiredDate] AS [Order.RequiredDate], [o].[ShippedDate] AS [Order.ShippedDate], [o].[ShipVia] AS [Order.ShipVia], [o].[Freight] AS [Order.Freight], [o].[ShipName] AS [Order.ShipName], [o].[ShipAddress] AS [Order.ShipAddress], [o].[ShipCity] AS [Order.ShipCity], [o].[ShipRegion] AS [Order.ShipRegion], [o].[ShipPostalCode] AS [Order.ShipPostalCode], [o].[ShipCountry] AS [Order.ShipCountry], [t].[Key.OrderID] AS [e.Key.OrderID], [t].[Key.ProductID] AS [e.Key.ProductID], [t].[Key.UnitPrice] AS [e.Key.UnitPrice], [t].[Key.Quantity] AS [e.Key.Quantity], [t].[Key.Discount] AS [e.Key.Discount], (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[ProductID] AS [ProductID], [o_0].[UnitPrice] AS [UnitPrice], [o_0].[Quantity] AS [Quantity], [o_0].[Discount] AS [Discount]
    FROM [dbo].[Order Details] AS [o_0]
    WHERE ((([t].[Key.OrderID] = [o_0].[OrderID]) AND ([t].[Key.ProductID] = [o_0].[ProductID])) AND (([t].[Key.UnitPrice] = [o_0].[UnitPrice]) AND ([t].[Key.Quantity] = [o_0].[Quantity]))) AND ([t].[Key.Discount] = [o_0].[Discount])
    FOR JSON PATH
) AS [e.Elements]
FROM (
    SELECT [o_1].[OrderID] AS [OrderID], [o_1].[ProductID] AS [ProductID], [o_1].[UnitPrice] AS [UnitPrice], [o_1].[Quantity] AS [Quantity], [o_1].[Discount] AS [Discount], [o_1].[OrderID] AS [Key.OrderID], [o_1].[ProductID] AS [Key.ProductID], [o_1].[UnitPrice] AS [Key.UnitPrice], [o_1].[Quantity] AS [Key.Quantity], [o_1].[Discount] AS [Key.Discount]
    FROM [dbo].[Order Details] AS [o_1]
    GROUP BY [o_1].[OrderID], [o_1].[ProductID], [o_1].[UnitPrice], [o_1].[Quantity], [o_1].[Discount]
) AS [t]
INNER JOIN [dbo].[Orders] AS [o] ON [t].[OrderID] = [o].[OrderID]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy3_navigation_intact_key_result()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order,
                        (k, e) => new { k.Customer, e });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [Customer.CustomerID], [c].[CompanyName] AS [Customer.CompanyName], [c].[ContactName] AS [Customer.ContactName], [c].[ContactTitle] AS [Customer.ContactTitle], [c].[Address] AS [Customer.Address], [c].[City] AS [Customer.City], [c].[Region] AS [Customer.Region], [c].[PostalCode] AS [Customer.PostalCode], [c].[Country] AS [Customer.Country], [c].[Phone] AS [Customer.Phone], [c].[Fax] AS [Customer.Fax], [t].[Key.OrderID] AS [e.Key.OrderID], [t].[Key.CustomerID] AS [e.Key.CustomerID], [t].[Key.EmployeeID] AS [e.Key.EmployeeID], [t].[Key.OrderDate] AS [e.Key.OrderDate], [t].[Key.RequiredDate] AS [e.Key.RequiredDate], [t].[Key.ShippedDate] AS [e.Key.ShippedDate], [t].[Key.ShipVia] AS [e.Key.ShipVia], [t].[Key.Freight] AS [e.Key.Freight], [t].[Key.ShipName] AS [e.Key.ShipName], [t].[Key.ShipAddress] AS [e.Key.ShipAddress], [t].[Key.ShipCity] AS [e.Key.ShipCity], [t].[Key.ShipRegion] AS [e.Key.ShipRegion], [t].[Key.ShipPostalCode] AS [e.Key.ShipPostalCode], [t].[Key.ShipCountry] AS [e.Key.ShipCountry], (
    SELECT [d].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[UnitPrice] AS [UnitPrice], [d].[Quantity] AS [Quantity], [d].[Discount] AS [Discount]
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
    WHERE (((([t].[Key.OrderID] = [o].[OrderID]) AND ([t].[Key.CustomerID] = [o].[CustomerID])) AND (([t].[Key.EmployeeID] = [o].[EmployeeID]) AND ([t].[Key.OrderDate] = [o].[OrderDate]))) AND ((([t].[Key.RequiredDate] = [o].[RequiredDate]) AND ([t].[Key.ShippedDate] = [o].[ShippedDate])) AND (([t].[Key.ShipVia] = [o].[ShipVia]) AND ([t].[Key.Freight] = [o].[Freight])))) AND (((([t].[Key.ShipName] = [o].[ShipName]) AND ([t].[Key.ShipAddress] = [o].[ShipAddress])) AND (([t].[Key.ShipCity] = [o].[ShipCity]) AND ([t].[Key.ShipRegion] = [o].[ShipRegion]))) AND (([t].[Key.ShipPostalCode] = [o].[ShipPostalCode]) AND ([t].[Key.ShipCountry] = [o].[ShipCountry])))
    FOR JSON PATH
) AS [e.Elements]
FROM (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate], [o_0].[RequiredDate] AS [RequiredDate], [o_0].[ShippedDate] AS [ShippedDate], [o_0].[ShipVia] AS [ShipVia], [o_0].[Freight] AS [Freight], [o_0].[ShipName] AS [ShipName], [o_0].[ShipAddress] AS [ShipAddress], [o_0].[ShipCity] AS [ShipCity], [o_0].[ShipRegion] AS [ShipRegion], [o_0].[ShipPostalCode] AS [ShipPostalCode], [o_0].[ShipCountry] AS [ShipCountry], [o_0].[OrderID] AS [Key.OrderID], [o_0].[CustomerID] AS [Key.CustomerID], [o_0].[EmployeeID] AS [Key.EmployeeID], [o_0].[OrderDate] AS [Key.OrderDate], [o_0].[RequiredDate] AS [Key.RequiredDate], [o_0].[ShippedDate] AS [Key.ShippedDate], [o_0].[ShipVia] AS [Key.ShipVia], [o_0].[Freight] AS [Key.Freight], [o_0].[ShipName] AS [Key.ShipName], [o_0].[ShipAddress] AS [Key.ShipAddress], [o_0].[ShipCity] AS [Key.ShipCity], [o_0].[ShipRegion] AS [Key.ShipRegion], [o_0].[ShipPostalCode] AS [Key.ShipPostalCode], [o_0].[ShipCountry] AS [Key.ShipCountry]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d_0].[OrderID] = [o_0].[OrderID]
    GROUP BY [o_0].[OrderID], [o_0].[CustomerID], [o_0].[EmployeeID], [o_0].[OrderDate], [o_0].[RequiredDate], [o_0].[ShippedDate], [o_0].[ShipVia], [o_0].[Freight], [o_0].[ShipName], [o_0].[ShipAddress], [o_0].[ShipCity], [o_0].[ShipRegion], [o_0].[ShipPostalCode], [o_0].[ShipCountry]
) AS [t]
INNER JOIN [dbo].[Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy3_navigation_aggregate_key()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order,
                        (k, e) => new { k.OrderID, e })
                    .Select(
                        g => new
                        {
                            g.OrderID,
                            max = g.e.Max(x => x.Quantity),
                        });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [OrderID], MAX([d].[Quantity]) AS [max]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
GROUP BY [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o].[RequiredDate], [o].[ShippedDate], [o].[ShipVia], [o].[Freight], [o].[ShipName], [o].[ShipAddress], [o].[ShipCity], [o].[ShipRegion], [o].[ShipPostalCode], [o].[ShipCountry]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy3_navigation_aggregate_result()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d,
                        (k, e) => new { k.Order, e })
                    .Select(
                        g => new
                        {
                            g.Order.OrderID,
                            max = g.e.Max(x => x.Quantity),
                        });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [OrderID], (
    SELECT MAX([o_0].[Quantity])
    FROM [dbo].[Order Details] AS [o_0]
    WHERE ((([t].[Key.OrderID] = [o_0].[OrderID]) AND ([t].[Key.ProductID] = [o_0].[ProductID])) AND (([t].[Key.UnitPrice] = [o_0].[UnitPrice]) AND ([t].[Key.Quantity] = [o_0].[Quantity]))) AND ([t].[Key.Discount] = [o_0].[Discount])
) AS [max]
FROM (
    SELECT [o_1].[OrderID] AS [OrderID], [o_1].[ProductID] AS [ProductID], [o_1].[UnitPrice] AS [UnitPrice], [o_1].[Quantity] AS [Quantity], [o_1].[Discount] AS [Discount], [o_1].[OrderID] AS [Key.OrderID], [o_1].[ProductID] AS [Key.ProductID], [o_1].[UnitPrice] AS [Key.UnitPrice], [o_1].[Quantity] AS [Key.Quantity], [o_1].[Discount] AS [Key.Discount]
    FROM [dbo].[Order Details] AS [o_1]
    GROUP BY [o_1].[OrderID], [o_1].[ProductID], [o_1].[UnitPrice], [o_1].[Quantity], [o_1].[Discount]
) AS [t]
INNER JOIN [dbo].[Orders] AS [o] ON [t].[OrderID] = [o].[OrderID]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy3_navigation_aggregate_key_result()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order,
                        (k, e) => new { k.Customer, e })
                    .Select(
                        g => new
                        {
                            g.Customer.CustomerID,
                            max = g.e.Max(x => x.Quantity),
                        });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [CustomerID], (
    SELECT MAX([d].[Quantity])
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
    WHERE (((([t].[Key.OrderID] = [o].[OrderID]) AND ([t].[Key.CustomerID] = [o].[CustomerID])) AND (([t].[Key.EmployeeID] = [o].[EmployeeID]) AND ([t].[Key.OrderDate] = [o].[OrderDate]))) AND ((([t].[Key.RequiredDate] = [o].[RequiredDate]) AND ([t].[Key.ShippedDate] = [o].[ShippedDate])) AND (([t].[Key.ShipVia] = [o].[ShipVia]) AND ([t].[Key.Freight] = [o].[Freight])))) AND (((([t].[Key.ShipName] = [o].[ShipName]) AND ([t].[Key.ShipAddress] = [o].[ShipAddress])) AND (([t].[Key.ShipCity] = [o].[ShipCity]) AND ([t].[Key.ShipRegion] = [o].[ShipRegion]))) AND (([t].[Key.ShipPostalCode] = [o].[ShipPostalCode]) AND ([t].[Key.ShipCountry] = [o].[ShipCountry])))
) AS [max]
FROM (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate], [o_0].[RequiredDate] AS [RequiredDate], [o_0].[ShippedDate] AS [ShippedDate], [o_0].[ShipVia] AS [ShipVia], [o_0].[Freight] AS [Freight], [o_0].[ShipName] AS [ShipName], [o_0].[ShipAddress] AS [ShipAddress], [o_0].[ShipCity] AS [ShipCity], [o_0].[ShipRegion] AS [ShipRegion], [o_0].[ShipPostalCode] AS [ShipPostalCode], [o_0].[ShipCountry] AS [ShipCountry], [o_0].[OrderID] AS [Key.OrderID], [o_0].[CustomerID] AS [Key.CustomerID], [o_0].[EmployeeID] AS [Key.EmployeeID], [o_0].[OrderDate] AS [Key.OrderDate], [o_0].[RequiredDate] AS [Key.RequiredDate], [o_0].[ShippedDate] AS [Key.ShippedDate], [o_0].[ShipVia] AS [Key.ShipVia], [o_0].[Freight] AS [Key.Freight], [o_0].[ShipName] AS [Key.ShipName], [o_0].[ShipAddress] AS [Key.ShipAddress], [o_0].[ShipCity] AS [Key.ShipCity], [o_0].[ShipRegion] AS [Key.ShipRegion], [o_0].[ShipPostalCode] AS [Key.ShipPostalCode], [o_0].[ShipCountry] AS [Key.ShipCountry]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d_0].[OrderID] = [o_0].[OrderID]
    GROUP BY [o_0].[OrderID], [o_0].[CustomerID], [o_0].[EmployeeID], [o_0].[OrderDate], [o_0].[RequiredDate], [o_0].[ShippedDate], [o_0].[ShipVia], [o_0].[Freight], [o_0].[ShipName], [o_0].[ShipAddress], [o_0].[ShipCity], [o_0].[ShipRegion], [o_0].[ShipPostalCode], [o_0].[ShipCountry]
) AS [t]
INNER JOIN [dbo].[Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_intact_key()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order,
                        d => d,
                        (x, y) => new { x, y });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [x.OrderID], [o].[CustomerID] AS [x.CustomerID], [o].[EmployeeID] AS [x.EmployeeID], [o].[OrderDate] AS [x.OrderDate], [o].[RequiredDate] AS [x.RequiredDate], [o].[ShippedDate] AS [x.ShippedDate], [o].[ShipVia] AS [x.ShipVia], [o].[Freight] AS [x.Freight], [o].[ShipName] AS [x.ShipName], [o].[ShipAddress] AS [x.ShipAddress], [o].[ShipCity] AS [x.ShipCity], [o].[ShipRegion] AS [x.ShipRegion], [o].[ShipPostalCode] AS [x.ShipPostalCode], [o].[ShipCountry] AS [x.ShipCountry], [o].[OrderID] AS [y.Key.OrderID], [o].[CustomerID] AS [y.Key.CustomerID], [o].[EmployeeID] AS [y.Key.EmployeeID], [o].[OrderDate] AS [y.Key.OrderDate], [o].[RequiredDate] AS [y.Key.RequiredDate], [o].[ShippedDate] AS [y.Key.ShippedDate], [o].[ShipVia] AS [y.Key.ShipVia], [o].[Freight] AS [y.Key.Freight], [o].[ShipName] AS [y.Key.ShipName], [o].[ShipAddress] AS [y.Key.ShipAddress], [o].[ShipCity] AS [y.Key.ShipCity], [o].[ShipRegion] AS [y.Key.ShipRegion], [o].[ShipPostalCode] AS [y.Key.ShipPostalCode], [o].[ShipCountry] AS [y.Key.ShipCountry], (
    SELECT [d].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[UnitPrice] AS [UnitPrice], [d].[Quantity] AS [Quantity], [d].[Discount] AS [Discount]
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d].[OrderID] = [o_0].[OrderID]
    WHERE (((([o].[OrderID] = [o_0].[OrderID]) AND ((([o].[CustomerID] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[CustomerID] = [o_0].[CustomerID])))) AND (((([o].[EmployeeID] IS NULL AND [o_0].[EmployeeID] IS NULL) OR ([o].[EmployeeID] = [o_0].[EmployeeID]))) AND ((([o].[OrderDate] IS NULL AND [o_0].[OrderDate] IS NULL) OR ([o].[OrderDate] = [o_0].[OrderDate]))))) AND ((((([o].[RequiredDate] IS NULL AND [o_0].[RequiredDate] IS NULL) OR ([o].[RequiredDate] = [o_0].[RequiredDate]))) AND ((([o].[ShippedDate] IS NULL AND [o_0].[ShippedDate] IS NULL) OR ([o].[ShippedDate] = [o_0].[ShippedDate])))) AND (((([o].[ShipVia] IS NULL AND [o_0].[ShipVia] IS NULL) OR ([o].[ShipVia] = [o_0].[ShipVia]))) AND ((([o].[Freight] IS NULL AND [o_0].[Freight] IS NULL) OR ([o].[Freight] = [o_0].[Freight])))))) AND (((((([o].[ShipName] IS NULL AND [o_0].[ShipName] IS NULL) OR ([o].[ShipName] = [o_0].[ShipName]))) AND ((([o].[ShipAddress] IS NULL AND [o_0].[ShipAddress] IS NULL) OR ([o].[ShipAddress] = [o_0].[ShipAddress])))) AND (((([o].[ShipCity] IS NULL AND [o_0].[ShipCity] IS NULL) OR ([o].[ShipCity] = [o_0].[ShipCity]))) AND ((([o].[ShipRegion] IS NULL AND [o_0].[ShipRegion] IS NULL) OR ([o].[ShipRegion] = [o_0].[ShipRegion]))))) AND (((([o].[ShipPostalCode] IS NULL AND [o_0].[ShipPostalCode] IS NULL) OR ([o].[ShipPostalCode] = [o_0].[ShipPostalCode]))) AND ((([o].[ShipCountry] IS NULL AND [o_0].[ShipCountry] IS NULL) OR ([o].[ShipCountry] = [o_0].[ShipCountry])))))
    FOR JSON PATH
) AS [y.Elements]
FROM [dbo].[Order Details] AS [d_0]
INNER JOIN [dbo].[Orders] AS [o] ON [d_0].[OrderID] = [o].[OrderID]
GROUP BY [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o].[RequiredDate], [o].[ShippedDate], [o].[ShipVia], [o].[Freight], [o].[ShipName], [o].[ShipAddress], [o].[ShipCity], [o].[ShipRegion], [o].[ShipPostalCode], [o].[ShipCountry]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_intact_element()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.ProductID,
                        d => d.Order.Customer,
                        (x, y) => new { x, y });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [d].[ProductID] AS [x], [d].[ProductID] AS [y.Key], (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Address] AS [Address], [c].[City] AS [City], [c].[Region] AS [Region], [c].[PostalCode] AS [PostalCode], [c].[Country] AS [Country], [c].[Phone] AS [Phone], [c].[Fax] AS [Fax]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o] ON [d_0].[OrderID] = [o].[OrderID]
    INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    WHERE [d].[ProductID] = [d_0].[ProductID]
    FOR JSON PATH
) AS [y.Elements]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o_0] ON [d].[OrderID] = [o_0].[OrderID]
INNER JOIN [dbo].[Customers] AS [c_0] ON [o_0].[CustomerID] = [c_0].[CustomerID]
GROUP BY [d].[ProductID]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_intact_result()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d,
                        d => d,
                        (x, y) => new { x.Order, y });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [Order.OrderID], [o].[CustomerID] AS [Order.CustomerID], [o].[EmployeeID] AS [Order.EmployeeID], [o].[OrderDate] AS [Order.OrderDate], [o].[RequiredDate] AS [Order.RequiredDate], [o].[ShippedDate] AS [Order.ShippedDate], [o].[ShipVia] AS [Order.ShipVia], [o].[Freight] AS [Order.Freight], [o].[ShipName] AS [Order.ShipName], [o].[ShipAddress] AS [Order.ShipAddress], [o].[ShipCity] AS [Order.ShipCity], [o].[ShipRegion] AS [Order.ShipRegion], [o].[ShipPostalCode] AS [Order.ShipPostalCode], [o].[ShipCountry] AS [Order.ShipCountry], [t].[Key.OrderID] AS [y.Key.OrderID], [t].[Key.ProductID] AS [y.Key.ProductID], [t].[Key.UnitPrice] AS [y.Key.UnitPrice], [t].[Key.Quantity] AS [y.Key.Quantity], [t].[Key.Discount] AS [y.Key.Discount], (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[ProductID] AS [ProductID], [o_0].[UnitPrice] AS [UnitPrice], [o_0].[Quantity] AS [Quantity], [o_0].[Discount] AS [Discount]
    FROM [dbo].[Order Details] AS [o_0]
    WHERE ((([t].[Key.OrderID] = [o_0].[OrderID]) AND ([t].[Key.ProductID] = [o_0].[ProductID])) AND (([t].[Key.UnitPrice] = [o_0].[UnitPrice]) AND ([t].[Key.Quantity] = [o_0].[Quantity]))) AND ([t].[Key.Discount] = [o_0].[Discount])
    FOR JSON PATH
) AS [y.Elements]
FROM (
    SELECT [o_1].[OrderID] AS [OrderID], [o_1].[ProductID] AS [ProductID], [o_1].[UnitPrice] AS [UnitPrice], [o_1].[Quantity] AS [Quantity], [o_1].[Discount] AS [Discount], [o_1].[OrderID] AS [Key.OrderID], [o_1].[ProductID] AS [Key.ProductID], [o_1].[UnitPrice] AS [Key.UnitPrice], [o_1].[Quantity] AS [Key.Quantity], [o_1].[Discount] AS [Key.Discount]
    FROM [dbo].[Order Details] AS [o_1]
    GROUP BY [o_1].[OrderID], [o_1].[ProductID], [o_1].[UnitPrice], [o_1].[Quantity], [o_1].[Discount]
) AS [t]
INNER JOIN [dbo].[Orders] AS [o] ON [t].[OrderID] = [o].[OrderID]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_intact_key_element()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order,
                        d => d.Order.Customer,
                        (x, y) => new { x, y });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [x.OrderID], [o].[CustomerID] AS [x.CustomerID], [o].[EmployeeID] AS [x.EmployeeID], [o].[OrderDate] AS [x.OrderDate], [o].[RequiredDate] AS [x.RequiredDate], [o].[ShippedDate] AS [x.ShippedDate], [o].[ShipVia] AS [x.ShipVia], [o].[Freight] AS [x.Freight], [o].[ShipName] AS [x.ShipName], [o].[ShipAddress] AS [x.ShipAddress], [o].[ShipCity] AS [x.ShipCity], [o].[ShipRegion] AS [x.ShipRegion], [o].[ShipPostalCode] AS [x.ShipPostalCode], [o].[ShipCountry] AS [x.ShipCountry], [o].[OrderID] AS [y.Key.OrderID], [o].[CustomerID] AS [y.Key.CustomerID], [o].[EmployeeID] AS [y.Key.EmployeeID], [o].[OrderDate] AS [y.Key.OrderDate], [o].[RequiredDate] AS [y.Key.RequiredDate], [o].[ShippedDate] AS [y.Key.ShippedDate], [o].[ShipVia] AS [y.Key.ShipVia], [o].[Freight] AS [y.Key.Freight], [o].[ShipName] AS [y.Key.ShipName], [o].[ShipAddress] AS [y.Key.ShipAddress], [o].[ShipCity] AS [y.Key.ShipCity], [o].[ShipRegion] AS [y.Key.ShipRegion], [o].[ShipPostalCode] AS [y.Key.ShipPostalCode], [o].[ShipCountry] AS [y.Key.ShipCountry], (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Address] AS [Address], [c].[City] AS [City], [c].[Region] AS [Region], [c].[PostalCode] AS [PostalCode], [c].[Country] AS [Country], [c].[Phone] AS [Phone], [c].[Fax] AS [Fax]
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d].[OrderID] = [o_0].[OrderID]
    INNER JOIN [dbo].[Customers] AS [c] ON [o_0].[CustomerID] = [c].[CustomerID]
    WHERE (((([o].[OrderID] = [o_0].[OrderID]) AND ((([o].[CustomerID] IS NULL AND [o_0].[CustomerID] IS NULL) OR ([o].[CustomerID] = [o_0].[CustomerID])))) AND (((([o].[EmployeeID] IS NULL AND [o_0].[EmployeeID] IS NULL) OR ([o].[EmployeeID] = [o_0].[EmployeeID]))) AND ((([o].[OrderDate] IS NULL AND [o_0].[OrderDate] IS NULL) OR ([o].[OrderDate] = [o_0].[OrderDate]))))) AND ((((([o].[RequiredDate] IS NULL AND [o_0].[RequiredDate] IS NULL) OR ([o].[RequiredDate] = [o_0].[RequiredDate]))) AND ((([o].[ShippedDate] IS NULL AND [o_0].[ShippedDate] IS NULL) OR ([o].[ShippedDate] = [o_0].[ShippedDate])))) AND (((([o].[ShipVia] IS NULL AND [o_0].[ShipVia] IS NULL) OR ([o].[ShipVia] = [o_0].[ShipVia]))) AND ((([o].[Freight] IS NULL AND [o_0].[Freight] IS NULL) OR ([o].[Freight] = [o_0].[Freight])))))) AND (((((([o].[ShipName] IS NULL AND [o_0].[ShipName] IS NULL) OR ([o].[ShipName] = [o_0].[ShipName]))) AND ((([o].[ShipAddress] IS NULL AND [o_0].[ShipAddress] IS NULL) OR ([o].[ShipAddress] = [o_0].[ShipAddress])))) AND (((([o].[ShipCity] IS NULL AND [o_0].[ShipCity] IS NULL) OR ([o].[ShipCity] = [o_0].[ShipCity]))) AND ((([o].[ShipRegion] IS NULL AND [o_0].[ShipRegion] IS NULL) OR ([o].[ShipRegion] = [o_0].[ShipRegion]))))) AND (((([o].[ShipPostalCode] IS NULL AND [o_0].[ShipPostalCode] IS NULL) OR ([o].[ShipPostalCode] = [o_0].[ShipPostalCode]))) AND ((([o].[ShipCountry] IS NULL AND [o_0].[ShipCountry] IS NULL) OR ([o].[ShipCountry] = [o_0].[ShipCountry])))))
    FOR JSON PATH
) AS [y.Elements]
FROM [dbo].[Order Details] AS [d_0]
INNER JOIN [dbo].[Orders] AS [o] ON [d_0].[OrderID] = [o].[OrderID]
INNER JOIN [dbo].[Customers] AS [c_0] ON [o].[CustomerID] = [c_0].[CustomerID]
GROUP BY [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o].[RequiredDate], [o].[ShippedDate], [o].[ShipVia], [o].[Freight], [o].[ShipName], [o].[ShipAddress], [o].[ShipCity], [o].[ShipRegion], [o].[ShipPostalCode], [o].[ShipCountry]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_intact_key_result()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order,
                        d => d,
                        (x, y) => new { x.Customer, y });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [Customer.CustomerID], [c].[CompanyName] AS [Customer.CompanyName], [c].[ContactName] AS [Customer.ContactName], [c].[ContactTitle] AS [Customer.ContactTitle], [c].[Address] AS [Customer.Address], [c].[City] AS [Customer.City], [c].[Region] AS [Customer.Region], [c].[PostalCode] AS [Customer.PostalCode], [c].[Country] AS [Customer.Country], [c].[Phone] AS [Customer.Phone], [c].[Fax] AS [Customer.Fax], [t].[Key.OrderID] AS [y.Key.OrderID], [t].[Key.CustomerID] AS [y.Key.CustomerID], [t].[Key.EmployeeID] AS [y.Key.EmployeeID], [t].[Key.OrderDate] AS [y.Key.OrderDate], [t].[Key.RequiredDate] AS [y.Key.RequiredDate], [t].[Key.ShippedDate] AS [y.Key.ShippedDate], [t].[Key.ShipVia] AS [y.Key.ShipVia], [t].[Key.Freight] AS [y.Key.Freight], [t].[Key.ShipName] AS [y.Key.ShipName], [t].[Key.ShipAddress] AS [y.Key.ShipAddress], [t].[Key.ShipCity] AS [y.Key.ShipCity], [t].[Key.ShipRegion] AS [y.Key.ShipRegion], [t].[Key.ShipPostalCode] AS [y.Key.ShipPostalCode], [t].[Key.ShipCountry] AS [y.Key.ShipCountry], (
    SELECT [d].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[UnitPrice] AS [UnitPrice], [d].[Quantity] AS [Quantity], [d].[Discount] AS [Discount]
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
    WHERE (((([t].[Key.OrderID] = [o].[OrderID]) AND ([t].[Key.CustomerID] = [o].[CustomerID])) AND (([t].[Key.EmployeeID] = [o].[EmployeeID]) AND ([t].[Key.OrderDate] = [o].[OrderDate]))) AND ((([t].[Key.RequiredDate] = [o].[RequiredDate]) AND ([t].[Key.ShippedDate] = [o].[ShippedDate])) AND (([t].[Key.ShipVia] = [o].[ShipVia]) AND ([t].[Key.Freight] = [o].[Freight])))) AND (((([t].[Key.ShipName] = [o].[ShipName]) AND ([t].[Key.ShipAddress] = [o].[ShipAddress])) AND (([t].[Key.ShipCity] = [o].[ShipCity]) AND ([t].[Key.ShipRegion] = [o].[ShipRegion]))) AND (([t].[Key.ShipPostalCode] = [o].[ShipPostalCode]) AND ([t].[Key.ShipCountry] = [o].[ShipCountry])))
    FOR JSON PATH
) AS [y.Elements]
FROM (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate], [o_0].[RequiredDate] AS [RequiredDate], [o_0].[ShippedDate] AS [ShippedDate], [o_0].[ShipVia] AS [ShipVia], [o_0].[Freight] AS [Freight], [o_0].[ShipName] AS [ShipName], [o_0].[ShipAddress] AS [ShipAddress], [o_0].[ShipCity] AS [ShipCity], [o_0].[ShipRegion] AS [ShipRegion], [o_0].[ShipPostalCode] AS [ShipPostalCode], [o_0].[ShipCountry] AS [ShipCountry], [o_0].[OrderID] AS [Key.OrderID], [o_0].[CustomerID] AS [Key.CustomerID], [o_0].[EmployeeID] AS [Key.EmployeeID], [o_0].[OrderDate] AS [Key.OrderDate], [o_0].[RequiredDate] AS [Key.RequiredDate], [o_0].[ShippedDate] AS [Key.ShippedDate], [o_0].[ShipVia] AS [Key.ShipVia], [o_0].[Freight] AS [Key.Freight], [o_0].[ShipName] AS [Key.ShipName], [o_0].[ShipAddress] AS [Key.ShipAddress], [o_0].[ShipCity] AS [Key.ShipCity], [o_0].[ShipRegion] AS [Key.ShipRegion], [o_0].[ShipPostalCode] AS [Key.ShipPostalCode], [o_0].[ShipCountry] AS [Key.ShipCountry]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d_0].[OrderID] = [o_0].[OrderID]
    GROUP BY [o_0].[OrderID], [o_0].[CustomerID], [o_0].[EmployeeID], [o_0].[OrderDate], [o_0].[RequiredDate], [o_0].[ShippedDate], [o_0].[ShipVia], [o_0].[Freight], [o_0].[ShipName], [o_0].[ShipAddress], [o_0].[ShipCity], [o_0].[ShipRegion], [o_0].[ShipPostalCode], [o_0].[ShipCountry]
) AS [t]
INNER JOIN [dbo].[Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_intact_element_result()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d,
                        d => d.Order.Customer,
                        (x, y) => new { x.Order, y });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [Order.OrderID], [o].[CustomerID] AS [Order.CustomerID], [o].[EmployeeID] AS [Order.EmployeeID], [o].[OrderDate] AS [Order.OrderDate], [o].[RequiredDate] AS [Order.RequiredDate], [o].[ShippedDate] AS [Order.ShippedDate], [o].[ShipVia] AS [Order.ShipVia], [o].[Freight] AS [Order.Freight], [o].[ShipName] AS [Order.ShipName], [o].[ShipAddress] AS [Order.ShipAddress], [o].[ShipCity] AS [Order.ShipCity], [o].[ShipRegion] AS [Order.ShipRegion], [o].[ShipPostalCode] AS [Order.ShipPostalCode], [o].[ShipCountry] AS [Order.ShipCountry], [t].[Key.OrderID] AS [y.Key.OrderID], [t].[Key.ProductID] AS [y.Key.ProductID], [t].[Key.UnitPrice] AS [y.Key.UnitPrice], [t].[Key.Quantity] AS [y.Key.Quantity], [t].[Key.Discount] AS [y.Key.Discount], (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Address] AS [Address], [c].[City] AS [City], [c].[Region] AS [Region], [c].[PostalCode] AS [PostalCode], [c].[Country] AS [Country], [c].[Phone] AS [Phone], [c].[Fax] AS [Fax]
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d].[OrderID] = [o_0].[OrderID]
    INNER JOIN [dbo].[Customers] AS [c] ON [o_0].[CustomerID] = [c].[CustomerID]
    WHERE ((([t].[Key.OrderID] = [d].[OrderID]) AND ([t].[Key.ProductID] = [d].[ProductID])) AND (([t].[Key.UnitPrice] = [d].[UnitPrice]) AND ([t].[Key.Quantity] = [d].[Quantity]))) AND ([t].[Key.Discount] = [d].[Discount])
    FOR JSON PATH
) AS [y.Elements]
FROM (
    SELECT [d_0].[OrderID] AS [OrderID], [d_0].[ProductID] AS [ProductID], [d_0].[UnitPrice] AS [UnitPrice], [d_0].[Quantity] AS [Quantity], [d_0].[Discount] AS [Discount], [d_0].[OrderID] AS [Key.OrderID], [d_0].[ProductID] AS [Key.ProductID], [d_0].[UnitPrice] AS [Key.UnitPrice], [d_0].[Quantity] AS [Key.Quantity], [d_0].[Discount] AS [Key.Discount]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_1] ON [d_0].[OrderID] = [o_1].[OrderID]
    INNER JOIN [dbo].[Customers] AS [c_0] ON [o_1].[CustomerID] = [c_0].[CustomerID]
    GROUP BY [d_0].[OrderID], [d_0].[ProductID], [d_0].[UnitPrice], [d_0].[Quantity], [d_0].[Discount]
) AS [t]
INNER JOIN [dbo].[Orders] AS [o] ON [t].[OrderID] = [o].[OrderID]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_intact_key_element_result()
        {
            var context = new QueryContext(impatient);

            var query 
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order, 
                        d => d.Order.Customer, 
                        (x, y) => new { x.Customer, y });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [Customer.CustomerID], [c].[CompanyName] AS [Customer.CompanyName], [c].[ContactName] AS [Customer.ContactName], [c].[ContactTitle] AS [Customer.ContactTitle], [c].[Address] AS [Customer.Address], [c].[City] AS [Customer.City], [c].[Region] AS [Customer.Region], [c].[PostalCode] AS [Customer.PostalCode], [c].[Country] AS [Customer.Country], [c].[Phone] AS [Customer.Phone], [c].[Fax] AS [Customer.Fax], [t].[Key.OrderID] AS [y.Key.OrderID], [t].[Key.CustomerID] AS [y.Key.CustomerID], [t].[Key.EmployeeID] AS [y.Key.EmployeeID], [t].[Key.OrderDate] AS [y.Key.OrderDate], [t].[Key.RequiredDate] AS [y.Key.RequiredDate], [t].[Key.ShippedDate] AS [y.Key.ShippedDate], [t].[Key.ShipVia] AS [y.Key.ShipVia], [t].[Key.Freight] AS [y.Key.Freight], [t].[Key.ShipName] AS [y.Key.ShipName], [t].[Key.ShipAddress] AS [y.Key.ShipAddress], [t].[Key.ShipCity] AS [y.Key.ShipCity], [t].[Key.ShipRegion] AS [y.Key.ShipRegion], [t].[Key.ShipPostalCode] AS [y.Key.ShipPostalCode], [t].[Key.ShipCountry] AS [y.Key.ShipCountry], (
    SELECT [c_0].[CustomerID] AS [CustomerID], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[Region] AS [Region], [c_0].[PostalCode] AS [PostalCode], [c_0].[Country] AS [Country], [c_0].[Phone] AS [Phone], [c_0].[Fax] AS [Fax]
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
    INNER JOIN [dbo].[Customers] AS [c_0] ON [o].[CustomerID] = [c_0].[CustomerID]
    WHERE (((([t].[Key.OrderID] = [o].[OrderID]) AND ([t].[Key.CustomerID] = [o].[CustomerID])) AND (([t].[Key.EmployeeID] = [o].[EmployeeID]) AND ([t].[Key.OrderDate] = [o].[OrderDate]))) AND ((([t].[Key.RequiredDate] = [o].[RequiredDate]) AND ([t].[Key.ShippedDate] = [o].[ShippedDate])) AND (([t].[Key.ShipVia] = [o].[ShipVia]) AND ([t].[Key.Freight] = [o].[Freight])))) AND (((([t].[Key.ShipName] = [o].[ShipName]) AND ([t].[Key.ShipAddress] = [o].[ShipAddress])) AND (([t].[Key.ShipCity] = [o].[ShipCity]) AND ([t].[Key.ShipRegion] = [o].[ShipRegion]))) AND (([t].[Key.ShipPostalCode] = [o].[ShipPostalCode]) AND ([t].[Key.ShipCountry] = [o].[ShipCountry])))
    FOR JSON PATH
) AS [y.Elements]
FROM (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate], [o_0].[RequiredDate] AS [RequiredDate], [o_0].[ShippedDate] AS [ShippedDate], [o_0].[ShipVia] AS [ShipVia], [o_0].[Freight] AS [Freight], [o_0].[ShipName] AS [ShipName], [o_0].[ShipAddress] AS [ShipAddress], [o_0].[ShipCity] AS [ShipCity], [o_0].[ShipRegion] AS [ShipRegion], [o_0].[ShipPostalCode] AS [ShipPostalCode], [o_0].[ShipCountry] AS [ShipCountry], [o_0].[OrderID] AS [Key.OrderID], [o_0].[CustomerID] AS [Key.CustomerID], [o_0].[EmployeeID] AS [Key.EmployeeID], [o_0].[OrderDate] AS [Key.OrderDate], [o_0].[RequiredDate] AS [Key.RequiredDate], [o_0].[ShippedDate] AS [Key.ShippedDate], [o_0].[ShipVia] AS [Key.ShipVia], [o_0].[Freight] AS [Key.Freight], [o_0].[ShipName] AS [Key.ShipName], [o_0].[ShipAddress] AS [Key.ShipAddress], [o_0].[ShipCity] AS [Key.ShipCity], [o_0].[ShipRegion] AS [Key.ShipRegion], [o_0].[ShipPostalCode] AS [Key.ShipPostalCode], [o_0].[ShipCountry] AS [Key.ShipCountry]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d_0].[OrderID] = [o_0].[OrderID]
    INNER JOIN [dbo].[Customers] AS [c_1] ON [o_0].[CustomerID] = [c_1].[CustomerID]
    GROUP BY [o_0].[OrderID], [o_0].[CustomerID], [o_0].[EmployeeID], [o_0].[OrderDate], [o_0].[RequiredDate], [o_0].[ShippedDate], [o_0].[ShipVia], [o_0].[Freight], [o_0].[ShipName], [o_0].[ShipAddress], [o_0].[ShipCity], [o_0].[ShipRegion], [o_0].[ShipPostalCode], [o_0].[ShipCountry]
) AS [t]
INNER JOIN [dbo].[Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_aggregate_key()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order,
                        d => d,
                        (x, y) => new { x, y = y.Max(z => z.Quantity) });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [x.OrderID], [o].[CustomerID] AS [x.CustomerID], [o].[EmployeeID] AS [x.EmployeeID], [o].[OrderDate] AS [x.OrderDate], [o].[RequiredDate] AS [x.RequiredDate], [o].[ShippedDate] AS [x.ShippedDate], [o].[ShipVia] AS [x.ShipVia], [o].[Freight] AS [x.Freight], [o].[ShipName] AS [x.ShipName], [o].[ShipAddress] AS [x.ShipAddress], [o].[ShipCity] AS [x.ShipCity], [o].[ShipRegion] AS [x.ShipRegion], [o].[ShipPostalCode] AS [x.ShipPostalCode], [o].[ShipCountry] AS [x.ShipCountry], MAX([d].[Quantity]) AS [y]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
GROUP BY [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o].[RequiredDate], [o].[ShippedDate], [o].[ShipVia], [o].[Freight], [o].[ShipName], [o].[ShipAddress], [o].[ShipCity], [o].[ShipRegion], [o].[ShipPostalCode], [o].[ShipCountry]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_aggregate_element()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.ProductID,
                        d => d.Order,
                        (x, y) => new { x, y = y.Max(z => z.OrderDate) });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [d].[ProductID] AS [x], MAX([o].[OrderDate]) AS [y]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
GROUP BY [d].[ProductID]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_aggregate_result()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order,
                        d => d,
                        (x, y) => new { x.Customer, y = y.Max(z => z.Quantity) });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [Customer.CustomerID], [c].[CompanyName] AS [Customer.CompanyName], [c].[ContactName] AS [Customer.ContactName], [c].[ContactTitle] AS [Customer.ContactTitle], [c].[Address] AS [Customer.Address], [c].[City] AS [Customer.City], [c].[Region] AS [Customer.Region], [c].[PostalCode] AS [Customer.PostalCode], [c].[Country] AS [Customer.Country], [c].[Phone] AS [Customer.Phone], [c].[Fax] AS [Customer.Fax], (
    SELECT MAX([d].[Quantity])
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
    WHERE (((([t].[Key.OrderID] = [o].[OrderID]) AND ([t].[Key.CustomerID] = [o].[CustomerID])) AND (([t].[Key.EmployeeID] = [o].[EmployeeID]) AND ([t].[Key.OrderDate] = [o].[OrderDate]))) AND ((([t].[Key.RequiredDate] = [o].[RequiredDate]) AND ([t].[Key.ShippedDate] = [o].[ShippedDate])) AND (([t].[Key.ShipVia] = [o].[ShipVia]) AND ([t].[Key.Freight] = [o].[Freight])))) AND (((([t].[Key.ShipName] = [o].[ShipName]) AND ([t].[Key.ShipAddress] = [o].[ShipAddress])) AND (([t].[Key.ShipCity] = [o].[ShipCity]) AND ([t].[Key.ShipRegion] = [o].[ShipRegion]))) AND (([t].[Key.ShipPostalCode] = [o].[ShipPostalCode]) AND ([t].[Key.ShipCountry] = [o].[ShipCountry])))
) AS [y]
FROM (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate], [o_0].[RequiredDate] AS [RequiredDate], [o_0].[ShippedDate] AS [ShippedDate], [o_0].[ShipVia] AS [ShipVia], [o_0].[Freight] AS [Freight], [o_0].[ShipName] AS [ShipName], [o_0].[ShipAddress] AS [ShipAddress], [o_0].[ShipCity] AS [ShipCity], [o_0].[ShipRegion] AS [ShipRegion], [o_0].[ShipPostalCode] AS [ShipPostalCode], [o_0].[ShipCountry] AS [ShipCountry], [o_0].[OrderID] AS [Key.OrderID], [o_0].[CustomerID] AS [Key.CustomerID], [o_0].[EmployeeID] AS [Key.EmployeeID], [o_0].[OrderDate] AS [Key.OrderDate], [o_0].[RequiredDate] AS [Key.RequiredDate], [o_0].[ShippedDate] AS [Key.ShippedDate], [o_0].[ShipVia] AS [Key.ShipVia], [o_0].[Freight] AS [Key.Freight], [o_0].[ShipName] AS [Key.ShipName], [o_0].[ShipAddress] AS [Key.ShipAddress], [o_0].[ShipCity] AS [Key.ShipCity], [o_0].[ShipRegion] AS [Key.ShipRegion], [o_0].[ShipPostalCode] AS [Key.ShipPostalCode], [o_0].[ShipCountry] AS [Key.ShipCountry]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d_0].[OrderID] = [o_0].[OrderID]
    GROUP BY [o_0].[OrderID], [o_0].[CustomerID], [o_0].[EmployeeID], [o_0].[OrderDate], [o_0].[RequiredDate], [o_0].[ShippedDate], [o_0].[ShipVia], [o_0].[Freight], [o_0].[ShipName], [o_0].[ShipAddress], [o_0].[ShipCity], [o_0].[ShipRegion], [o_0].[ShipPostalCode], [o_0].[ShipCountry]
) AS [t]
INNER JOIN [dbo].[Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_aggregate_key_element()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order.Customer,
                        d => d.Order,
                        (x, y) => new { x, y = y.Max(z => z.OrderDate) });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [x.CustomerID], [c].[CompanyName] AS [x.CompanyName], [c].[ContactName] AS [x.ContactName], [c].[ContactTitle] AS [x.ContactTitle], [c].[Address] AS [x.Address], [c].[City] AS [x.City], [c].[Region] AS [x.Region], [c].[PostalCode] AS [x.PostalCode], [c].[Country] AS [x.Country], [c].[Phone] AS [x.Phone], [c].[Fax] AS [x.Fax], MAX([o].[OrderDate]) AS [y]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[CustomerID], [c].[CompanyName], [c].[ContactName], [c].[ContactTitle], [c].[Address], [c].[City], [c].[Region], [c].[PostalCode], [c].[Country], [c].[Phone], [c].[Fax]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_aggregate_key_result()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order,
                        d => d,
                        (x, y) => new { x.Customer, y = y.Max(z => z.Quantity) });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [Customer.CustomerID], [c].[CompanyName] AS [Customer.CompanyName], [c].[ContactName] AS [Customer.ContactName], [c].[ContactTitle] AS [Customer.ContactTitle], [c].[Address] AS [Customer.Address], [c].[City] AS [Customer.City], [c].[Region] AS [Customer.Region], [c].[PostalCode] AS [Customer.PostalCode], [c].[Country] AS [Customer.Country], [c].[Phone] AS [Customer.Phone], [c].[Fax] AS [Customer.Fax], (
    SELECT MAX([d].[Quantity])
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
    WHERE (((([t].[Key.OrderID] = [o].[OrderID]) AND ([t].[Key.CustomerID] = [o].[CustomerID])) AND (([t].[Key.EmployeeID] = [o].[EmployeeID]) AND ([t].[Key.OrderDate] = [o].[OrderDate]))) AND ((([t].[Key.RequiredDate] = [o].[RequiredDate]) AND ([t].[Key.ShippedDate] = [o].[ShippedDate])) AND (([t].[Key.ShipVia] = [o].[ShipVia]) AND ([t].[Key.Freight] = [o].[Freight])))) AND (((([t].[Key.ShipName] = [o].[ShipName]) AND ([t].[Key.ShipAddress] = [o].[ShipAddress])) AND (([t].[Key.ShipCity] = [o].[ShipCity]) AND ([t].[Key.ShipRegion] = [o].[ShipRegion]))) AND (([t].[Key.ShipPostalCode] = [o].[ShipPostalCode]) AND ([t].[Key.ShipCountry] = [o].[ShipCountry])))
) AS [y]
FROM (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate], [o_0].[RequiredDate] AS [RequiredDate], [o_0].[ShippedDate] AS [ShippedDate], [o_0].[ShipVia] AS [ShipVia], [o_0].[Freight] AS [Freight], [o_0].[ShipName] AS [ShipName], [o_0].[ShipAddress] AS [ShipAddress], [o_0].[ShipCity] AS [ShipCity], [o_0].[ShipRegion] AS [ShipRegion], [o_0].[ShipPostalCode] AS [ShipPostalCode], [o_0].[ShipCountry] AS [ShipCountry], [o_0].[OrderID] AS [Key.OrderID], [o_0].[CustomerID] AS [Key.CustomerID], [o_0].[EmployeeID] AS [Key.EmployeeID], [o_0].[OrderDate] AS [Key.OrderDate], [o_0].[RequiredDate] AS [Key.RequiredDate], [o_0].[ShippedDate] AS [Key.ShippedDate], [o_0].[ShipVia] AS [Key.ShipVia], [o_0].[Freight] AS [Key.Freight], [o_0].[ShipName] AS [Key.ShipName], [o_0].[ShipAddress] AS [Key.ShipAddress], [o_0].[ShipCity] AS [Key.ShipCity], [o_0].[ShipRegion] AS [Key.ShipRegion], [o_0].[ShipPostalCode] AS [Key.ShipPostalCode], [o_0].[ShipCountry] AS [Key.ShipCountry]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d_0].[OrderID] = [o_0].[OrderID]
    GROUP BY [o_0].[OrderID], [o_0].[CustomerID], [o_0].[EmployeeID], [o_0].[OrderDate], [o_0].[RequiredDate], [o_0].[ShippedDate], [o_0].[ShipVia], [o_0].[Freight], [o_0].[ShipName], [o_0].[ShipAddress], [o_0].[ShipCity], [o_0].[ShipRegion], [o_0].[ShipPostalCode], [o_0].[ShipCountry]
) AS [t]
INNER JOIN [dbo].[Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_aggregate_element_result()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d,
                        d => d.Order,
                        (x, y) => new { x.Order, y = y.Max(z => z.OrderDate) });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [Order.OrderID], [o].[CustomerID] AS [Order.CustomerID], [o].[EmployeeID] AS [Order.EmployeeID], [o].[OrderDate] AS [Order.OrderDate], [o].[RequiredDate] AS [Order.RequiredDate], [o].[ShippedDate] AS [Order.ShippedDate], [o].[ShipVia] AS [Order.ShipVia], [o].[Freight] AS [Order.Freight], [o].[ShipName] AS [Order.ShipName], [o].[ShipAddress] AS [Order.ShipAddress], [o].[ShipCity] AS [Order.ShipCity], [o].[ShipRegion] AS [Order.ShipRegion], [o].[ShipPostalCode] AS [Order.ShipPostalCode], [o].[ShipCountry] AS [Order.ShipCountry], (
    SELECT MAX([o_0].[OrderDate])
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d].[OrderID] = [o_0].[OrderID]
    WHERE ((([t].[Key.OrderID] = [d].[OrderID]) AND ([t].[Key.ProductID] = [d].[ProductID])) AND (([t].[Key.UnitPrice] = [d].[UnitPrice]) AND ([t].[Key.Quantity] = [d].[Quantity]))) AND ([t].[Key.Discount] = [d].[Discount])
) AS [y]
FROM (
    SELECT [d_0].[OrderID] AS [OrderID], [d_0].[ProductID] AS [ProductID], [d_0].[UnitPrice] AS [UnitPrice], [d_0].[Quantity] AS [Quantity], [d_0].[Discount] AS [Discount], [d_0].[OrderID] AS [Key.OrderID], [d_0].[ProductID] AS [Key.ProductID], [d_0].[UnitPrice] AS [Key.UnitPrice], [d_0].[Quantity] AS [Key.Quantity], [d_0].[Discount] AS [Key.Discount]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_1] ON [d_0].[OrderID] = [o_1].[OrderID]
    GROUP BY [d_0].[OrderID], [d_0].[ProductID], [d_0].[UnitPrice], [d_0].[Quantity], [d_0].[Discount]
) AS [t]
INNER JOIN [dbo].[Orders] AS [o] ON [t].[OrderID] = [o].[OrderID]",
                SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_aggregate_key_element_result()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order,
                        d => d.Order,
                        (x, y) => new { x.Customer, y = y.Max(z => z.OrderDate) });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [Customer.CustomerID], [c].[CompanyName] AS [Customer.CompanyName], [c].[ContactName] AS [Customer.ContactName], [c].[ContactTitle] AS [Customer.ContactTitle], [c].[Address] AS [Customer.Address], [c].[City] AS [Customer.City], [c].[Region] AS [Customer.Region], [c].[PostalCode] AS [Customer.PostalCode], [c].[Country] AS [Customer.Country], [c].[Phone] AS [Customer.Phone], [c].[Fax] AS [Customer.Fax], (
    SELECT MAX([o].[OrderDate])
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
    WHERE (((([t].[Key.OrderID] = [o].[OrderID]) AND ([t].[Key.CustomerID] = [o].[CustomerID])) AND (([t].[Key.EmployeeID] = [o].[EmployeeID]) AND ([t].[Key.OrderDate] = [o].[OrderDate]))) AND ((([t].[Key.RequiredDate] = [o].[RequiredDate]) AND ([t].[Key.ShippedDate] = [o].[ShippedDate])) AND (([t].[Key.ShipVia] = [o].[ShipVia]) AND ([t].[Key.Freight] = [o].[Freight])))) AND (((([t].[Key.ShipName] = [o].[ShipName]) AND ([t].[Key.ShipAddress] = [o].[ShipAddress])) AND (([t].[Key.ShipCity] = [o].[ShipCity]) AND ([t].[Key.ShipRegion] = [o].[ShipRegion]))) AND (([t].[Key.ShipPostalCode] = [o].[ShipPostalCode]) AND ([t].[Key.ShipCountry] = [o].[ShipCountry])))
) AS [y]
FROM (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate], [o_0].[RequiredDate] AS [RequiredDate], [o_0].[ShippedDate] AS [ShippedDate], [o_0].[ShipVia] AS [ShipVia], [o_0].[Freight] AS [Freight], [o_0].[ShipName] AS [ShipName], [o_0].[ShipAddress] AS [ShipAddress], [o_0].[ShipCity] AS [ShipCity], [o_0].[ShipRegion] AS [ShipRegion], [o_0].[ShipPostalCode] AS [ShipPostalCode], [o_0].[ShipCountry] AS [ShipCountry], [o_0].[OrderID] AS [Key.OrderID], [o_0].[CustomerID] AS [Key.CustomerID], [o_0].[EmployeeID] AS [Key.EmployeeID], [o_0].[OrderDate] AS [Key.OrderDate], [o_0].[RequiredDate] AS [Key.RequiredDate], [o_0].[ShippedDate] AS [Key.ShippedDate], [o_0].[ShipVia] AS [Key.ShipVia], [o_0].[Freight] AS [Key.Freight], [o_0].[ShipName] AS [Key.ShipName], [o_0].[ShipAddress] AS [Key.ShipAddress], [o_0].[ShipCity] AS [Key.ShipCity], [o_0].[ShipRegion] AS [Key.ShipRegion], [o_0].[ShipPostalCode] AS [Key.ShipPostalCode], [o_0].[ShipCountry] AS [Key.ShipCountry]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d_0].[OrderID] = [o_0].[OrderID]
    GROUP BY [o_0].[OrderID], [o_0].[CustomerID], [o_0].[EmployeeID], [o_0].[OrderDate], [o_0].[RequiredDate], [o_0].[ShippedDate], [o_0].[ShipVia], [o_0].[Freight], [o_0].[ShipName], [o_0].[ShipAddress], [o_0].[ShipCity], [o_0].[ShipRegion], [o_0].[ShipPostalCode], [o_0].[ShipCountry]
) AS [t]
INNER JOIN [dbo].[Customers] AS [c] ON [t].[CustomerID] = [c].[CustomerID]",
                SqlLog);
        }

        // Extra GroupBy tests: navigations within aggregations, etc.

        [TestMethod]
        public void GroupBy1_navigation_aggregate_navigation()
        {
            var context = new QueryContext(impatient);

            var query
                = context.OrderDetails
                    .GroupBy(d => d.Order.Customer.City)
                    .Select(g => new
                    {
                        City = g.Key,
                        LastOrderDate = g.Max(d => d.Order.OrderDate)
                    });

            query.ToList();

            // TODO: Eliminate redundant join in subquery by detecting and retargeting before adding it
            // TODO: Figure out how to reduce down to MAX([o].[OrderDate]) AS [LastOrderDate]
            Assert.AreEqual(
                @"SELECT [c].[City] AS [City], (
    SELECT MAX([o].[OrderDate])
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d].[OrderID] = [o_0].[OrderID]
    INNER JOIN [dbo].[Customers] AS [c_0] ON [o_0].[CustomerID] = [c_0].[CustomerID]
    INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
    WHERE (([c].[City] IS NULL AND [c_0].[City] IS NULL) OR ([c].[City] = [c_0].[City]))
) AS [LastOrderDate]
FROM [dbo].[Order Details] AS [d_0]
INNER JOIN [dbo].[Orders] AS [o_1] ON [d_0].[OrderID] = [o_1].[OrderID]
INNER JOIN [dbo].[Customers] AS [c] ON [o_1].[CustomerID] = [c].[CustomerID]
GROUP BY [c].[City]",
                SqlLog);
        }

        [TestMethod]
        public void Zip_navigation()
        {
            var context = new QueryContext(impatient);

            var query 
                = context.OrderDetails
                    .Take(10)
                    .Zip(
                        context.Orders.Take(10), 
                        (d, o) => new
                        {
                            c1 = d.Order.Customer.City,
                            c2 = o.Customer.City,
                        });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [t].[City] AS [c1], [t_0].[City] AS [c2]
FROM (
    SELECT [o].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[UnitPrice] AS [UnitPrice], [d].[Quantity] AS [Quantity], [d].[Discount] AS [Discount], [c].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Address] AS [Address], [c].[City] AS [City], [c].[Region] AS [Region], [c].[PostalCode] AS [PostalCode], [c].[Country] AS [Country], [c].[Phone] AS [Phone], [c].[Fax] AS [Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
    FROM (
        SELECT TOP (10) [d_0].[OrderID] AS [OrderID], [d_0].[ProductID] AS [ProductID], [d_0].[UnitPrice] AS [UnitPrice], [d_0].[Quantity] AS [Quantity], [d_0].[Discount] AS [Discount]
        FROM [dbo].[Order Details] AS [d_0]
    ) AS [d]
    INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
    INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
) AS [t]
INNER JOIN (
    SELECT [o_0].[OrderID] AS [OrderID], [c_0].[CustomerID] AS [CustomerID], [o_0].[EmployeeID] AS [EmployeeID], [o_0].[OrderDate] AS [OrderDate], [o_0].[RequiredDate] AS [RequiredDate], [o_0].[ShippedDate] AS [ShippedDate], [o_0].[ShipVia] AS [ShipVia], [o_0].[Freight] AS [Freight], [o_0].[ShipName] AS [ShipName], [o_0].[ShipAddress] AS [ShipAddress], [o_0].[ShipCity] AS [ShipCity], [o_0].[ShipRegion] AS [ShipRegion], [o_0].[ShipPostalCode] AS [ShipPostalCode], [o_0].[ShipCountry] AS [ShipCountry], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[Region] AS [Region], [c_0].[PostalCode] AS [PostalCode], [c_0].[Country] AS [Country], [c_0].[Phone] AS [Phone], [c_0].[Fax] AS [Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
    FROM (
        SELECT TOP (10) [o_1].[OrderID] AS [OrderID], [o_1].[CustomerID] AS [CustomerID], [o_1].[EmployeeID] AS [EmployeeID], [o_1].[OrderDate] AS [OrderDate], [o_1].[RequiredDate] AS [RequiredDate], [o_1].[ShippedDate] AS [ShippedDate], [o_1].[ShipVia] AS [ShipVia], [o_1].[Freight] AS [Freight], [o_1].[ShipName] AS [ShipName], [o_1].[ShipAddress] AS [ShipAddress], [o_1].[ShipCity] AS [ShipCity], [o_1].[ShipRegion] AS [ShipRegion], [o_1].[ShipPostalCode] AS [ShipPostalCode], [o_1].[ShipCountry] AS [ShipCountry]
        FROM [dbo].[Orders] AS [o_1]
    ) AS [o_0]
    INNER JOIN [dbo].[Customers] AS [c_0] ON [o_0].[CustomerID] = [c_0].[CustomerID]
) AS [t_0] ON [t].[$rownumber] = [t_0].[$rownumber]",
                SqlLog);
        }
    }
}
