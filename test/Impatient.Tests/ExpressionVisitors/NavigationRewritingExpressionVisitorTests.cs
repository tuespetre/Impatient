using Impatient.Metadata;
using Impatient.Query;
using Impatient.Tests.Northwind;
using Impatient.Tests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;
using static Impatient.Tests.Utilities.QueryExpressionHelper;

namespace Impatient.Tests
{
    [TestClass]
    public class NavigationRewritingExpressionVisitorTests
    {
        private static IServiceProvider services;

        private static NorthwindQueryContext context => services.GetService<NorthwindQueryContext>();

        static NavigationRewritingExpressionVisitorTests()
        {
            var navigationDescriptors = new[]
            {
                new NavigationDescriptor(
                    typeof(Customer),
                    typeof(Customer).GetRuntimeProperty(nameof(Customer.Orders)),
                    GetExpression((Customer c) => c.CustomerID),
                    GetExpression((Order o) => o.CustomerID),
                    false,
                    CreateQueryExpression<Order>()),
                new NavigationDescriptor(
                    typeof(Order),
                    typeof(Order).GetRuntimeProperty(nameof(Order.Customer)),
                    GetExpression((Order o) => o.CustomerID),
                    GetExpression((Customer c) => c.CustomerID),
                    false,
                    CreateQueryExpression<Customer>()),
                new NavigationDescriptor(
                    typeof(Order),
                    typeof(Order).GetRuntimeProperty(nameof(Order.OrderDetails)),
                    GetExpression((Order o) => o.OrderID),
                    GetExpression((OrderDetail d) => d.OrderID),
                    false,
                    CreateQueryExpression<OrderDetail>()),
                new NavigationDescriptor(
                    typeof(OrderDetail),
                    typeof(OrderDetail).GetRuntimeProperty(nameof(OrderDetail.Order)),
                    GetExpression((OrderDetail d) => d.OrderID),
                    GetExpression((Order o) => o.OrderID),
                    false,
                    CreateQueryExpression<Order>()),
            };

            var primaryKeyDescriptors = new[]
            {
                new PrimaryKeyDescriptor(
                    typeof(Customer),
                    GetExpression((Customer c) => c.CustomerID)),
                new PrimaryKeyDescriptor(
                    typeof(Order),
                    GetExpression((Order o) => o.OrderID)),
                new PrimaryKeyDescriptor(
                    typeof(OrderDetail),
                    GetExpression((OrderDetail d) => new { d.OrderID, d.ProductID })),
            };

            services 
                = ExtensionMethods.CreateServiceProvider(
                    descriptorSet: new DescriptorSet(primaryKeyDescriptors, navigationDescriptors),
                    connectionString: @"Server=.\sqlexpress; Database=NORTHWND; Trusted_Connection=True");
        }

        [TestCleanup]
        public void Cleanup()
        {
            context.ClearLog();
        }

        [TestMethod]
        public void Where_navigation_m1r()
        {
            var query = from o in context.Orders
                        where o.Customer.City == "Berlin"
                        select o;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[City] = N'Berlin'",
                context.SqlLog);
        }

        [TestMethod]
        public void Where_navigation_m1r_m1r()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void Where_navigation_m1r_m1r_repeated_access()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void Where2_navigation()
        {
            var query = context.Orders.Where((o, i) => o.Customer.City == "Berlin");

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[City] = N'Berlin'",
                context.SqlLog);
        }

        [TestMethod]
        public void Select_navigation_m1r()
        {
            var query = from o in context.Orders
                        select o.Customer;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [CustomerID], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Address] AS [Address], [c].[City] AS [City], [c].[Region] AS [Region], [c].[PostalCode] AS [PostalCode], [c].[Country] AS [Country], [c].[Phone] AS [Phone], [c].[Fax] AS [Fax]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]",
                context.SqlLog);
        }

        [TestMethod]
        public void Select_navigation_m1r_repeated_access()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void Select_navigation_m1r_m1r()
        {
            var query = from d in context.OrderDetails
                        select d.Order.Customer;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [CustomerID], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Address] AS [Address], [c].[City] AS [City], [c].[Region] AS [Region], [c].[PostalCode] AS [PostalCode], [c].[Country] AS [Country], [c].[Phone] AS [Phone], [c].[Fax] AS [Fax]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]",
                context.SqlLog);
        }

        [TestMethod]
        public void Select_navigation_12m_scalar()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void SelectMany_cross_join_same_table_Where_same_navigation()
        {
            var query = from o1 in context.Orders
                        from o2 in context.Orders
                        where o1.Customer.City == o2.Customer.City
                        select new { x = o1.OrderID, y = o2.OrderID };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o1].[OrderID] AS [x], [o].[OrderID] AS [y]
FROM [dbo].[Orders] AS [o1]
CROSS JOIN [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o1].[CustomerID] = [c].[CustomerID]
INNER JOIN [dbo].[Customers] AS [c_0] ON [o].[CustomerID] = [c_0].[CustomerID]
WHERE (([c].[City] IS NULL AND [c_0].[City] IS NULL) OR ([c].[City] = [c_0].[City]))",
                context.SqlLog);
        }

        [TestMethod]
        public void SelectMany_navigation_12m()
        {
            var query = from c in context.Customers
                        from o in c.Orders
                        select new { c.CustomerID, o.OrderID };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [CustomerID], [o].[OrderID] AS [OrderID]
FROM [dbo].[Customers] AS [c]
INNER JOIN [dbo].[Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]",
                context.SqlLog);
        }

        [TestMethod]
        public void SelectMany_navigation_12m_2()
        {
            var query = context.Customers.SelectMany(c => c.Orders).Select(o => o.OrderID);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID]
FROM [dbo].[Customers] AS [c]
INNER JOIN [dbo].[Orders] AS [o] ON [c].[CustomerID] = [o].[CustomerID]",
                context.SqlLog);
        }

        [TestMethod]
        public void SelectMany_navigation_12m_12m()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void SelectMany_navigation_12m_DefaultIfEmpty()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void SelectMany_navigation_12m_DefaultIfEmpty_12m_DefaultIfEmpty()
        {
            var query = from c in context.Customers
                        from o in c.Orders.DefaultIfEmpty()
                        from d in o.OrderDetails.DefaultIfEmpty()
                        select new { c.CustomerID, o.OrderID };

            query.ToList();

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
) AS [d_0] ON [o].[OrderID] = [d_0].[OrderID]",
                context.SqlLog);
        }

        [TestMethod]
        public void OrderBy_navigation()
        {
            var query = from o in context.Orders
                        orderby o.Customer.ContactName
                        select o;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[ContactName] ASC",
                context.SqlLog);
        }

        [TestMethod]
        public void OrderByDescending_navigation()
        {
            var query = from o in context.Orders
                        orderby o.Customer.ContactName descending
                        select o;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[ContactName] DESC",
                context.SqlLog);
        }

        [TestMethod]
        public void ThenBy_navigation()
        {
            var query = from o in context.Orders
                        orderby o.Customer.City, o.Customer.ContactName
                        select o;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[City] ASC, [c].[ContactName] ASC",
                context.SqlLog);
        }

        [TestMethod]
        public void ThenByDescending_navigation()
        {
            var query = from o in context.Orders
                        orderby o.Customer.City, o.Customer.ContactName descending
                        select o;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
ORDER BY [c].[City] ASC, [c].[ContactName] DESC",
                context.SqlLog);
        }

        [TestMethod]
        public void SkipWhile1_navigation()
        {
            var query = context.Orders.SkipWhile(o => o.Customer.City != "Berlin");

            query.ToList();

            Assert.AreEqual(
                @"SELECT [t].[$outer.OrderID] AS [OrderID], [t].[$outer.CustomerID] AS [CustomerID], [t].[$outer.EmployeeID] AS [EmployeeID], [t].[$outer.OrderDate] AS [OrderDate], [t].[$outer.RequiredDate] AS [RequiredDate], [t].[$outer.ShippedDate] AS [ShippedDate], [t].[$outer.ShipVia] AS [ShipVia], [t].[$outer.Freight] AS [Freight], [t].[$outer.ShipName] AS [ShipName], [t].[$outer.ShipAddress] AS [ShipAddress], [t].[$outer.ShipCity] AS [ShipCity], [t].[$outer.ShipRegion] AS [ShipRegion], [t].[$outer.ShipPostalCode] AS [ShipPostalCode], [t].[$outer.ShipCountry] AS [ShipCountry]
FROM (
    SELECT [o].[OrderID] AS [$outer.OrderID], [o].[CustomerID] AS [$outer.CustomerID], [o].[EmployeeID] AS [$outer.EmployeeID], [o].[OrderDate] AS [$outer.OrderDate], [o].[RequiredDate] AS [$outer.RequiredDate], [o].[ShippedDate] AS [$outer.ShippedDate], [o].[ShipVia] AS [$outer.ShipVia], [o].[Freight] AS [$outer.Freight], [o].[ShipName] AS [$outer.ShipName], [o].[ShipAddress] AS [$outer.ShipAddress], [o].[ShipCity] AS [$outer.ShipCity], [o].[ShipRegion] AS [$outer.ShipRegion], [o].[ShipPostalCode] AS [$outer.ShipPostalCode], [o].[ShipCountry] AS [$outer.ShipCountry], [c].[CustomerID] AS [$inner.CustomerID], [c].[CompanyName] AS [$inner.CompanyName], [c].[ContactName] AS [$inner.ContactName], [c].[ContactTitle] AS [$inner.ContactTitle], [c].[Address] AS [$inner.Address], [c].[City] AS [$inner.City], [c].[Region] AS [$inner.Region], [c].[PostalCode] AS [$inner.PostalCode], [c].[Country] AS [$inner.Country], [c].[Phone] AS [$inner.Phone], [c].[Fax] AS [$inner.Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
    FROM [dbo].[Orders] AS [o]
    INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
) AS [t]
WHERE [t].[$rownumber] >= (
    SELECT COALESCE(MIN([t_0].[$rownumber]), 0)
    FROM (
        SELECT [o_0].[OrderID] AS [$outer.OrderID], [o_0].[CustomerID] AS [$outer.CustomerID], [o_0].[EmployeeID] AS [$outer.EmployeeID], [o_0].[OrderDate] AS [$outer.OrderDate], [o_0].[RequiredDate] AS [$outer.RequiredDate], [o_0].[ShippedDate] AS [$outer.ShippedDate], [o_0].[ShipVia] AS [$outer.ShipVia], [o_0].[Freight] AS [$outer.Freight], [o_0].[ShipName] AS [$outer.ShipName], [o_0].[ShipAddress] AS [$outer.ShipAddress], [o_0].[ShipCity] AS [$outer.ShipCity], [o_0].[ShipRegion] AS [$outer.ShipRegion], [o_0].[ShipPostalCode] AS [$outer.ShipPostalCode], [o_0].[ShipCountry] AS [$outer.ShipCountry], [c_0].[CustomerID] AS [$inner.CustomerID], [c_0].[CompanyName] AS [$inner.CompanyName], [c_0].[ContactName] AS [$inner.ContactName], [c_0].[ContactTitle] AS [$inner.ContactTitle], [c_0].[Address] AS [$inner.Address], [c_0].[City] AS [$inner.City], [c_0].[Region] AS [$inner.Region], [c_0].[PostalCode] AS [$inner.PostalCode], [c_0].[Country] AS [$inner.Country], [c_0].[Phone] AS [$inner.Phone], [c_0].[Fax] AS [$inner.Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
        FROM [dbo].[Orders] AS [o_0]
        INNER JOIN [dbo].[Customers] AS [c_0] ON [o_0].[CustomerID] = [c_0].[CustomerID]
    ) AS [t_0]
    WHERE [t_0].[$inner.City] = N'Berlin'
)",
                context.SqlLog);
        }

        [TestMethod]
        public void SkipWhile2_navigation()
        {
            var query = context.Orders.SkipWhile((o, i) => o.Customer.City != "Berlin");

            query.ToList();

            Assert.AreEqual(
                @"SELECT [t].[$outer.OrderID] AS [OrderID], [t].[$outer.CustomerID] AS [CustomerID], [t].[$outer.EmployeeID] AS [EmployeeID], [t].[$outer.OrderDate] AS [OrderDate], [t].[$outer.RequiredDate] AS [RequiredDate], [t].[$outer.ShippedDate] AS [ShippedDate], [t].[$outer.ShipVia] AS [ShipVia], [t].[$outer.Freight] AS [Freight], [t].[$outer.ShipName] AS [ShipName], [t].[$outer.ShipAddress] AS [ShipAddress], [t].[$outer.ShipCity] AS [ShipCity], [t].[$outer.ShipRegion] AS [ShipRegion], [t].[$outer.ShipPostalCode] AS [ShipPostalCode], [t].[$outer.ShipCountry] AS [ShipCountry]
FROM (
    SELECT [o].[OrderID] AS [$outer.OrderID], [o].[CustomerID] AS [$outer.CustomerID], [o].[EmployeeID] AS [$outer.EmployeeID], [o].[OrderDate] AS [$outer.OrderDate], [o].[RequiredDate] AS [$outer.RequiredDate], [o].[ShippedDate] AS [$outer.ShippedDate], [o].[ShipVia] AS [$outer.ShipVia], [o].[Freight] AS [$outer.Freight], [o].[ShipName] AS [$outer.ShipName], [o].[ShipAddress] AS [$outer.ShipAddress], [o].[ShipCity] AS [$outer.ShipCity], [o].[ShipRegion] AS [$outer.ShipRegion], [o].[ShipPostalCode] AS [$outer.ShipPostalCode], [o].[ShipCountry] AS [$outer.ShipCountry], [c].[CustomerID] AS [$inner.CustomerID], [c].[CompanyName] AS [$inner.CompanyName], [c].[ContactName] AS [$inner.ContactName], [c].[ContactTitle] AS [$inner.ContactTitle], [c].[Address] AS [$inner.Address], [c].[City] AS [$inner.City], [c].[Region] AS [$inner.Region], [c].[PostalCode] AS [$inner.PostalCode], [c].[Country] AS [$inner.Country], [c].[Phone] AS [$inner.Phone], [c].[Fax] AS [$inner.Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
    FROM [dbo].[Orders] AS [o]
    INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
) AS [t]
WHERE [t].[$rownumber] >= (
    SELECT COALESCE(MIN([t_0].[$rownumber]), 0)
    FROM (
        SELECT [o_0].[OrderID] AS [$outer.OrderID], [o_0].[CustomerID] AS [$outer.CustomerID], [o_0].[EmployeeID] AS [$outer.EmployeeID], [o_0].[OrderDate] AS [$outer.OrderDate], [o_0].[RequiredDate] AS [$outer.RequiredDate], [o_0].[ShippedDate] AS [$outer.ShippedDate], [o_0].[ShipVia] AS [$outer.ShipVia], [o_0].[Freight] AS [$outer.Freight], [o_0].[ShipName] AS [$outer.ShipName], [o_0].[ShipAddress] AS [$outer.ShipAddress], [o_0].[ShipCity] AS [$outer.ShipCity], [o_0].[ShipRegion] AS [$outer.ShipRegion], [o_0].[ShipPostalCode] AS [$outer.ShipPostalCode], [o_0].[ShipCountry] AS [$outer.ShipCountry], [c_0].[CustomerID] AS [$inner.CustomerID], [c_0].[CompanyName] AS [$inner.CompanyName], [c_0].[ContactName] AS [$inner.ContactName], [c_0].[ContactTitle] AS [$inner.ContactTitle], [c_0].[Address] AS [$inner.Address], [c_0].[City] AS [$inner.City], [c_0].[Region] AS [$inner.Region], [c_0].[PostalCode] AS [$inner.PostalCode], [c_0].[Country] AS [$inner.Country], [c_0].[Phone] AS [$inner.Phone], [c_0].[Fax] AS [$inner.Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
        FROM [dbo].[Orders] AS [o_0]
        INNER JOIN [dbo].[Customers] AS [c_0] ON [o_0].[CustomerID] = [c_0].[CustomerID]
    ) AS [t_0]
    WHERE [t_0].[$inner.City] = N'Berlin'
)",
                context.SqlLog);
        }

        [TestMethod]
        public void TakeWhile1_navigation()
        {
            var query = context.Orders.TakeWhile(o => o.Customer.City != "Berlin");

            query.ToList();

            Assert.AreEqual(
                @"SELECT [t].[$outer.OrderID] AS [OrderID], [t].[$outer.CustomerID] AS [CustomerID], [t].[$outer.EmployeeID] AS [EmployeeID], [t].[$outer.OrderDate] AS [OrderDate], [t].[$outer.RequiredDate] AS [RequiredDate], [t].[$outer.ShippedDate] AS [ShippedDate], [t].[$outer.ShipVia] AS [ShipVia], [t].[$outer.Freight] AS [Freight], [t].[$outer.ShipName] AS [ShipName], [t].[$outer.ShipAddress] AS [ShipAddress], [t].[$outer.ShipCity] AS [ShipCity], [t].[$outer.ShipRegion] AS [ShipRegion], [t].[$outer.ShipPostalCode] AS [ShipPostalCode], [t].[$outer.ShipCountry] AS [ShipCountry]
FROM (
    SELECT [o].[OrderID] AS [$outer.OrderID], [o].[CustomerID] AS [$outer.CustomerID], [o].[EmployeeID] AS [$outer.EmployeeID], [o].[OrderDate] AS [$outer.OrderDate], [o].[RequiredDate] AS [$outer.RequiredDate], [o].[ShippedDate] AS [$outer.ShippedDate], [o].[ShipVia] AS [$outer.ShipVia], [o].[Freight] AS [$outer.Freight], [o].[ShipName] AS [$outer.ShipName], [o].[ShipAddress] AS [$outer.ShipAddress], [o].[ShipCity] AS [$outer.ShipCity], [o].[ShipRegion] AS [$outer.ShipRegion], [o].[ShipPostalCode] AS [$outer.ShipPostalCode], [o].[ShipCountry] AS [$outer.ShipCountry], [c].[CustomerID] AS [$inner.CustomerID], [c].[CompanyName] AS [$inner.CompanyName], [c].[ContactName] AS [$inner.ContactName], [c].[ContactTitle] AS [$inner.ContactTitle], [c].[Address] AS [$inner.Address], [c].[City] AS [$inner.City], [c].[Region] AS [$inner.Region], [c].[PostalCode] AS [$inner.PostalCode], [c].[Country] AS [$inner.Country], [c].[Phone] AS [$inner.Phone], [c].[Fax] AS [$inner.Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
    FROM [dbo].[Orders] AS [o]
    INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
) AS [t]
WHERE [t].[$rownumber] < (
    SELECT COALESCE(MIN([t_0].[$rownumber]), [t].[$rownumber] + 1)
    FROM (
        SELECT [o_0].[OrderID] AS [$outer.OrderID], [o_0].[CustomerID] AS [$outer.CustomerID], [o_0].[EmployeeID] AS [$outer.EmployeeID], [o_0].[OrderDate] AS [$outer.OrderDate], [o_0].[RequiredDate] AS [$outer.RequiredDate], [o_0].[ShippedDate] AS [$outer.ShippedDate], [o_0].[ShipVia] AS [$outer.ShipVia], [o_0].[Freight] AS [$outer.Freight], [o_0].[ShipName] AS [$outer.ShipName], [o_0].[ShipAddress] AS [$outer.ShipAddress], [o_0].[ShipCity] AS [$outer.ShipCity], [o_0].[ShipRegion] AS [$outer.ShipRegion], [o_0].[ShipPostalCode] AS [$outer.ShipPostalCode], [o_0].[ShipCountry] AS [$outer.ShipCountry], [c_0].[CustomerID] AS [$inner.CustomerID], [c_0].[CompanyName] AS [$inner.CompanyName], [c_0].[ContactName] AS [$inner.ContactName], [c_0].[ContactTitle] AS [$inner.ContactTitle], [c_0].[Address] AS [$inner.Address], [c_0].[City] AS [$inner.City], [c_0].[Region] AS [$inner.Region], [c_0].[PostalCode] AS [$inner.PostalCode], [c_0].[Country] AS [$inner.Country], [c_0].[Phone] AS [$inner.Phone], [c_0].[Fax] AS [$inner.Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
        FROM [dbo].[Orders] AS [o_0]
        INNER JOIN [dbo].[Customers] AS [c_0] ON [o_0].[CustomerID] = [c_0].[CustomerID]
    ) AS [t_0]
    WHERE [t_0].[$inner.City] = N'Berlin'
)",
                context.SqlLog);
        }

        [TestMethod]
        public void TakeWhile2_navigation()
        {
            var query = context.Orders.TakeWhile((o, i) => o.Customer.City != "Berlin");

            query.ToList();

            Assert.AreEqual(
                @"SELECT [t].[$outer.OrderID] AS [OrderID], [t].[$outer.CustomerID] AS [CustomerID], [t].[$outer.EmployeeID] AS [EmployeeID], [t].[$outer.OrderDate] AS [OrderDate], [t].[$outer.RequiredDate] AS [RequiredDate], [t].[$outer.ShippedDate] AS [ShippedDate], [t].[$outer.ShipVia] AS [ShipVia], [t].[$outer.Freight] AS [Freight], [t].[$outer.ShipName] AS [ShipName], [t].[$outer.ShipAddress] AS [ShipAddress], [t].[$outer.ShipCity] AS [ShipCity], [t].[$outer.ShipRegion] AS [ShipRegion], [t].[$outer.ShipPostalCode] AS [ShipPostalCode], [t].[$outer.ShipCountry] AS [ShipCountry]
FROM (
    SELECT [o].[OrderID] AS [$outer.OrderID], [o].[CustomerID] AS [$outer.CustomerID], [o].[EmployeeID] AS [$outer.EmployeeID], [o].[OrderDate] AS [$outer.OrderDate], [o].[RequiredDate] AS [$outer.RequiredDate], [o].[ShippedDate] AS [$outer.ShippedDate], [o].[ShipVia] AS [$outer.ShipVia], [o].[Freight] AS [$outer.Freight], [o].[ShipName] AS [$outer.ShipName], [o].[ShipAddress] AS [$outer.ShipAddress], [o].[ShipCity] AS [$outer.ShipCity], [o].[ShipRegion] AS [$outer.ShipRegion], [o].[ShipPostalCode] AS [$outer.ShipPostalCode], [o].[ShipCountry] AS [$outer.ShipCountry], [c].[CustomerID] AS [$inner.CustomerID], [c].[CompanyName] AS [$inner.CompanyName], [c].[ContactName] AS [$inner.ContactName], [c].[ContactTitle] AS [$inner.ContactTitle], [c].[Address] AS [$inner.Address], [c].[City] AS [$inner.City], [c].[Region] AS [$inner.Region], [c].[PostalCode] AS [$inner.PostalCode], [c].[Country] AS [$inner.Country], [c].[Phone] AS [$inner.Phone], [c].[Fax] AS [$inner.Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
    FROM [dbo].[Orders] AS [o]
    INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
) AS [t]
WHERE [t].[$rownumber] < (
    SELECT COALESCE(MIN([t_0].[$rownumber]), [t].[$rownumber] + 1)
    FROM (
        SELECT [o_0].[OrderID] AS [$outer.OrderID], [o_0].[CustomerID] AS [$outer.CustomerID], [o_0].[EmployeeID] AS [$outer.EmployeeID], [o_0].[OrderDate] AS [$outer.OrderDate], [o_0].[RequiredDate] AS [$outer.RequiredDate], [o_0].[ShippedDate] AS [$outer.ShippedDate], [o_0].[ShipVia] AS [$outer.ShipVia], [o_0].[Freight] AS [$outer.Freight], [o_0].[ShipName] AS [$outer.ShipName], [o_0].[ShipAddress] AS [$outer.ShipAddress], [o_0].[ShipCity] AS [$outer.ShipCity], [o_0].[ShipRegion] AS [$outer.ShipRegion], [o_0].[ShipPostalCode] AS [$outer.ShipPostalCode], [o_0].[ShipCountry] AS [$outer.ShipCountry], [c_0].[CustomerID] AS [$inner.CustomerID], [c_0].[CompanyName] AS [$inner.CompanyName], [c_0].[ContactName] AS [$inner.ContactName], [c_0].[ContactTitle] AS [$inner.ContactTitle], [c_0].[Address] AS [$inner.Address], [c_0].[City] AS [$inner.City], [c_0].[Region] AS [$inner.Region], [c_0].[PostalCode] AS [$inner.PostalCode], [c_0].[Country] AS [$inner.Country], [c_0].[Phone] AS [$inner.Phone], [c_0].[Fax] AS [$inner.Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
        FROM [dbo].[Orders] AS [o_0]
        INNER JOIN [dbo].[Customers] AS [c_0] ON [o_0].[CustomerID] = [c_0].[CustomerID]
    ) AS [t_0]
    WHERE [t_0].[$inner.City] = N'Berlin'
)",
                context.SqlLog);
        }

        [TestMethod]
        public void All_navigation()
        {
            var result = context.Orders.All(o => o.Customer.ContactName != null);

            Assert.AreEqual(
                @"SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM [dbo].[Orders] AS [o]
    INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    WHERE [c].[ContactName] IS NULL
) THEN 0 ELSE 1 END) AS bit)",
                context.SqlLog);
        }

        [TestMethod]
        public void Any2_navigation()
        {
            var result = context.Orders.Any(o => o.Customer.ContactName != null);

            Assert.AreEqual(
                @"SELECT CAST((CASE WHEN EXISTS (
    SELECT 1
    FROM [dbo].[Orders] AS [o]
    INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
    WHERE [c].[ContactName] IS NOT NULL
) THEN 1 ELSE 0 END) AS bit)",
                context.SqlLog);
        }

        [TestMethod]
        public void Count2_navigation()
        {
            var result = context.Orders.Count(o => o.Customer.ContactName != null);

            Assert.AreEqual(
                @"SELECT COUNT(*)
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[ContactName] IS NOT NULL",
                context.SqlLog);
        }

        [TestMethod]
        public void LongCount2_navigation()
        {
            var result = context.Orders.LongCount(o => o.Customer.ContactName != null);

            Assert.AreEqual(
                @"SELECT COUNT_BIG(*)
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[ContactName] IS NOT NULL",
                context.SqlLog);
        }

        [TestMethod]
        public void First2_navigation()
        {
            var result = context.Orders.First(o => o.Customer.ContactName != null);

            Assert.AreEqual(
                @"SELECT TOP (1) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[ContactName] IS NOT NULL",
                context.SqlLog);
        }

        [TestMethod]
        public void FirstOrDefault2_navigation()
        {
            var result = context.Orders.FirstOrDefault(o => o.Customer.ContactName != null);

            Assert.AreEqual(
                @"SELECT TOP (1) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[ContactName] IS NOT NULL",
                context.SqlLog);
        }

        [TestMethod]
        public void Last2_navigation()
        {
            var result = context.Orders.OrderBy(o => o.OrderDate).Last(o => o.Customer.ContactName != null);

            Assert.AreEqual(
                @"SELECT TOP (1) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[ContactName] IS NOT NULL
ORDER BY [o].[OrderDate] DESC",
                context.SqlLog);
        }

        [TestMethod]
        public void LastOrDefault2_navigation()
        {
            var result = context.Orders.OrderBy(o => o.OrderDate).LastOrDefault(o => o.Customer.ContactName != null);

            Assert.AreEqual(
                @"SELECT TOP (1) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShippedDate] AS [ShippedDate], [o].[ShipVia] AS [ShipVia], [o].[Freight] AS [Freight], [o].[ShipName] AS [ShipName], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipRegion] AS [ShipRegion], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipCountry] AS [ShipCountry]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [c].[ContactName] IS NOT NULL
ORDER BY [o].[OrderDate] DESC",
                context.SqlLog);
        }

        [TestMethod]
        public void Single2_navigation()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void SingleOrDefault2_navigation()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void Average_navigation()
        {
            var result = context.OrderDetails.Average(d => d.Order.Freight);

            Assert.AreEqual(
                @"SELECT AVG(CAST([o].[Freight] AS decimal))
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]",
                context.SqlLog);
        }

        [TestMethod]
        public void Max_navigation()
        {
            var result = context.OrderDetails.Max(d => d.Order.Freight);

            Assert.AreEqual(
                @"SELECT MAX([o].[Freight])
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]",
                context.SqlLog);
        }

        [TestMethod]
        public void Min_navigation()
        {
            var result = context.OrderDetails.Min(d => d.Order.Freight);

            Assert.AreEqual(
                @"SELECT MIN([o].[Freight])
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]",
                context.SqlLog);
        }

        [TestMethod]
        public void Sum_navigation()
        {
            var result = context.OrderDetails.Sum(d => d.Order.Freight);

            Assert.AreEqual(
                @"SELECT SUM([o].[Freight])
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]",
                context.SqlLog);
        }

        [TestMethod]
        public void Cast_navigation()
        {
            var query = context.OrderDetails.Select(d => d.Order).Cast<Order>().Select(o => o.OrderDate);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderDate]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]",
                context.SqlLog);
        }

        [TestMethod]
        public void OfType_navigation()
        {
            var query = context.OrderDetails.Select(d => d.Order).Cast<Order>().Select(o => o.OrderDate);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderDate]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]",
                context.SqlLog);
        }

        [TestMethod]
        public void Reverse_navigation()
        {
            var query = context.OrderDetails.OrderBy(d => d.UnitPrice).Select(d => d.Order).Reverse().Select(o => o.OrderDate);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderDate]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
ORDER BY [d].[UnitPrice] DESC",
                context.SqlLog);
        }

        [TestMethod]
        public void Skip_navigation()
        {
            var query = context.OrderDetails.OrderBy(d => d.UnitPrice).Select(d => d.Order).Skip(1).Select(o => o.OrderDate);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderDate]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
ORDER BY [d].[UnitPrice] ASC
OFFSET 1 ROWS",
                context.SqlLog);
        }

        [TestMethod]
        public void Take_navigation()
        {
            var query = context.OrderDetails.Select(d => d.Order).Take(1).Select(o => o.OrderDate);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (1) [o].[OrderDate]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]",
                context.SqlLog);
        }

        [TestMethod]
        public void Join_navigation()
        {
            var query = from o1 in context.Orders
                        join o2 in context.Orders on o1.Customer.City equals o2.Customer.City
                        select new { o1, o2 };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o1].[OrderID] AS [o1.OrderID], [o1].[CustomerID] AS [o1.CustomerID], [o1].[EmployeeID] AS [o1.EmployeeID], [o1].[OrderDate] AS [o1.OrderDate], [o1].[RequiredDate] AS [o1.RequiredDate], [o1].[ShippedDate] AS [o1.ShippedDate], [o1].[ShipVia] AS [o1.ShipVia], [o1].[Freight] AS [o1.Freight], [o1].[ShipName] AS [o1.ShipName], [o1].[ShipAddress] AS [o1.ShipAddress], [o1].[ShipCity] AS [o1.ShipCity], [o1].[ShipRegion] AS [o1.ShipRegion], [o1].[ShipPostalCode] AS [o1.ShipPostalCode], [o1].[ShipCountry] AS [o1.ShipCountry], [t].[$outer.OrderID] AS [o2.OrderID], [t].[$outer.CustomerID] AS [o2.CustomerID], [t].[$outer.EmployeeID] AS [o2.EmployeeID], [t].[$outer.OrderDate] AS [o2.OrderDate], [t].[$outer.RequiredDate] AS [o2.RequiredDate], [t].[$outer.ShippedDate] AS [o2.ShippedDate], [t].[$outer.ShipVia] AS [o2.ShipVia], [t].[$outer.Freight] AS [o2.Freight], [t].[$outer.ShipName] AS [o2.ShipName], [t].[$outer.ShipAddress] AS [o2.ShipAddress], [t].[$outer.ShipCity] AS [o2.ShipCity], [t].[$outer.ShipRegion] AS [o2.ShipRegion], [t].[$outer.ShipPostalCode] AS [o2.ShipPostalCode], [t].[$outer.ShipCountry] AS [o2.ShipCountry]
FROM [dbo].[Orders] AS [o1]
INNER JOIN [dbo].[Customers] AS [c] ON [o1].[CustomerID] = [c].[CustomerID]
INNER JOIN (
    SELECT [o2].[OrderID] AS [$outer.OrderID], [o2].[CustomerID] AS [$outer.CustomerID], [o2].[EmployeeID] AS [$outer.EmployeeID], [o2].[OrderDate] AS [$outer.OrderDate], [o2].[RequiredDate] AS [$outer.RequiredDate], [o2].[ShippedDate] AS [$outer.ShippedDate], [o2].[ShipVia] AS [$outer.ShipVia], [o2].[Freight] AS [$outer.Freight], [o2].[ShipName] AS [$outer.ShipName], [o2].[ShipAddress] AS [$outer.ShipAddress], [o2].[ShipCity] AS [$outer.ShipCity], [o2].[ShipRegion] AS [$outer.ShipRegion], [o2].[ShipPostalCode] AS [$outer.ShipPostalCode], [o2].[ShipCountry] AS [$outer.ShipCountry], [c_0].[CustomerID] AS [$inner.CustomerID], [c_0].[CompanyName] AS [$inner.CompanyName], [c_0].[ContactName] AS [$inner.ContactName], [c_0].[ContactTitle] AS [$inner.ContactTitle], [c_0].[Address] AS [$inner.Address], [c_0].[City] AS [$inner.City], [c_0].[Region] AS [$inner.Region], [c_0].[PostalCode] AS [$inner.PostalCode], [c_0].[Country] AS [$inner.Country], [c_0].[Phone] AS [$inner.Phone], [c_0].[Fax] AS [$inner.Fax]
    FROM [dbo].[Orders] AS [o2]
    INNER JOIN [dbo].[Customers] AS [c_0] ON [o2].[CustomerID] = [c_0].[CustomerID]
) AS [t] ON [c].[City] = [t].[$inner.City]",
                context.SqlLog);
        }

        [TestMethod]
        public void Join_navigation_repeated_access()
        {
            var query = from o1 in context.Orders
                        join o2 in context.Orders on o1.Customer.City equals o2.Customer.City
                        select new { c1 = o1.Customer.CustomerID, c2 = o2.Customer.CustomerID };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [c1], [t].[$inner.CustomerID] AS [c2]
FROM [dbo].[Orders] AS [o1]
INNER JOIN [dbo].[Customers] AS [c] ON [o1].[CustomerID] = [c].[CustomerID]
INNER JOIN (
    SELECT [o2].[OrderID] AS [$outer.OrderID], [o2].[CustomerID] AS [$outer.CustomerID], [o2].[EmployeeID] AS [$outer.EmployeeID], [o2].[OrderDate] AS [$outer.OrderDate], [o2].[RequiredDate] AS [$outer.RequiredDate], [o2].[ShippedDate] AS [$outer.ShippedDate], [o2].[ShipVia] AS [$outer.ShipVia], [o2].[Freight] AS [$outer.Freight], [o2].[ShipName] AS [$outer.ShipName], [o2].[ShipAddress] AS [$outer.ShipAddress], [o2].[ShipCity] AS [$outer.ShipCity], [o2].[ShipRegion] AS [$outer.ShipRegion], [o2].[ShipPostalCode] AS [$outer.ShipPostalCode], [o2].[ShipCountry] AS [$outer.ShipCountry], [c_0].[CustomerID] AS [$inner.CustomerID], [c_0].[CompanyName] AS [$inner.CompanyName], [c_0].[ContactName] AS [$inner.ContactName], [c_0].[ContactTitle] AS [$inner.ContactTitle], [c_0].[Address] AS [$inner.Address], [c_0].[City] AS [$inner.City], [c_0].[Region] AS [$inner.Region], [c_0].[PostalCode] AS [$inner.PostalCode], [c_0].[Country] AS [$inner.Country], [c_0].[Phone] AS [$inner.Phone], [c_0].[Fax] AS [$inner.Fax]
    FROM [dbo].[Orders] AS [o2]
    INNER JOIN [dbo].[Customers] AS [c_0] ON [o2].[CustomerID] = [c_0].[CustomerID]
) AS [t] ON [c].[City] = [t].[$inner.City]",
                context.SqlLog);
        }

        [TestMethod]
        public void GroupJoin_navigation()
        {
            var query = from o1 in context.Orders
                        join o2 in context.Orders on o1.Customer.City equals o2.Customer.City into o2g
                        select new { o1, count = o2g.Count() };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o1].[OrderID] AS [o1.OrderID], [o1].[CustomerID] AS [o1.CustomerID], [o1].[EmployeeID] AS [o1.EmployeeID], [o1].[OrderDate] AS [o1.OrderDate], [o1].[RequiredDate] AS [o1.RequiredDate], [o1].[ShippedDate] AS [o1.ShippedDate], [o1].[ShipVia] AS [o1.ShipVia], [o1].[Freight] AS [o1.Freight], [o1].[ShipName] AS [o1.ShipName], [o1].[ShipAddress] AS [o1.ShipAddress], [o1].[ShipCity] AS [o1.ShipCity], [o1].[ShipRegion] AS [o1.ShipRegion], [o1].[ShipPostalCode] AS [o1.ShipPostalCode], [o1].[ShipCountry] AS [o1.ShipCountry], (
    SELECT COUNT(*)
    FROM [dbo].[Orders] AS [o2]
    INNER JOIN [dbo].[Customers] AS [c] ON [o2].[CustomerID] = [c].[CustomerID]
    WHERE [c_0].[City] = [c].[City]
) AS [count]
FROM [dbo].[Orders] AS [o1]
INNER JOIN [dbo].[Customers] AS [c_0] ON [o1].[CustomerID] = [c_0].[CustomerID]",
                context.SqlLog);
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
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy1_navigation_aggregate_key()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy2_navigation_intact_key()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy2_navigation_intact_element()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy2_navigation_intact_key_element()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy2_navigation_aggregate_key()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy2_navigation_aggregate_element()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy2_navigation_aggregate_key_element()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy3_navigation_intact_key()
        {
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
    WHERE [o].[OrderID] = [o_0].[OrderID]
    FOR JSON PATH
) AS [e.Elements]
FROM [dbo].[Order Details] AS [d_0]
INNER JOIN [dbo].[Orders] AS [o] ON [d_0].[OrderID] = [o].[OrderID]
GROUP BY [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o].[RequiredDate], [o].[ShippedDate], [o].[ShipVia], [o].[Freight], [o].[ShipName], [o].[ShipAddress], [o].[ShipCity], [o].[ShipRegion], [o].[ShipPostalCode], [o].[ShipCountry]",
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy3_navigation_intact_result()
        {
            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d,
                        (k, e) => new { k.Order, e });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [Order.OrderID], [o].[CustomerID] AS [Order.CustomerID], [o].[EmployeeID] AS [Order.EmployeeID], [o].[OrderDate] AS [Order.OrderDate], [o].[RequiredDate] AS [Order.RequiredDate], [o].[ShippedDate] AS [Order.ShippedDate], [o].[ShipVia] AS [Order.ShipVia], [o].[Freight] AS [Order.Freight], [o].[ShipName] AS [Order.ShipName], [o].[ShipAddress] AS [Order.ShipAddress], [o].[ShipCity] AS [Order.ShipCity], [o].[ShipRegion] AS [Order.ShipRegion], [o].[ShipPostalCode] AS [Order.ShipPostalCode], [o].[ShipCountry] AS [Order.ShipCountry], [t].[$inner.Key.OrderID] AS [e.Key.OrderID], [t].[$inner.Key.ProductID] AS [e.Key.ProductID], [t].[$inner.Key.UnitPrice] AS [e.Key.UnitPrice], [t].[$inner.Key.Quantity] AS [e.Key.Quantity], [t].[$inner.Key.Discount] AS [e.Key.Discount], (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[ProductID] AS [ProductID], [o_0].[UnitPrice] AS [UnitPrice], [o_0].[Quantity] AS [Quantity], [o_0].[Discount] AS [Discount]
    FROM [dbo].[Order Details] AS [o_0]
    WHERE ([t].[$inner.Key.OrderID] = [o_0].[OrderID]) AND ([t].[$inner.Key.ProductID] = [o_0].[ProductID])
    FOR JSON PATH
) AS [e.Elements]
FROM (
    SELECT [o_1].[OrderID] AS [$outer.OrderID], [o_1].[ProductID] AS [$outer.ProductID], [o_1].[UnitPrice] AS [$outer.UnitPrice], [o_1].[Quantity] AS [$outer.Quantity], [o_1].[Discount] AS [$outer.Discount], [o_1].[OrderID] AS [$inner.Key.OrderID], [o_1].[ProductID] AS [$inner.Key.ProductID], [o_1].[UnitPrice] AS [$inner.Key.UnitPrice], [o_1].[Quantity] AS [$inner.Key.Quantity], [o_1].[Discount] AS [$inner.Key.Discount]
    FROM [dbo].[Order Details] AS [o_1]
    GROUP BY [o_1].[OrderID], [o_1].[ProductID], [o_1].[UnitPrice], [o_1].[Quantity], [o_1].[Discount]
) AS [t]
INNER JOIN [dbo].[Orders] AS [o] ON [t].[$outer.OrderID] = [o].[OrderID]",
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy3_navigation_intact_key_result()
        {
            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order,
                        (k, e) => new { k.Customer, e });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [Customer.CustomerID], [c].[CompanyName] AS [Customer.CompanyName], [c].[ContactName] AS [Customer.ContactName], [c].[ContactTitle] AS [Customer.ContactTitle], [c].[Address] AS [Customer.Address], [c].[City] AS [Customer.City], [c].[Region] AS [Customer.Region], [c].[PostalCode] AS [Customer.PostalCode], [c].[Country] AS [Customer.Country], [c].[Phone] AS [Customer.Phone], [c].[Fax] AS [Customer.Fax], [t].[$inner.Key.OrderID] AS [e.Key.OrderID], [t].[$inner.Key.CustomerID] AS [e.Key.CustomerID], [t].[$inner.Key.EmployeeID] AS [e.Key.EmployeeID], [t].[$inner.Key.OrderDate] AS [e.Key.OrderDate], [t].[$inner.Key.RequiredDate] AS [e.Key.RequiredDate], [t].[$inner.Key.ShippedDate] AS [e.Key.ShippedDate], [t].[$inner.Key.ShipVia] AS [e.Key.ShipVia], [t].[$inner.Key.Freight] AS [e.Key.Freight], [t].[$inner.Key.ShipName] AS [e.Key.ShipName], [t].[$inner.Key.ShipAddress] AS [e.Key.ShipAddress], [t].[$inner.Key.ShipCity] AS [e.Key.ShipCity], [t].[$inner.Key.ShipRegion] AS [e.Key.ShipRegion], [t].[$inner.Key.ShipPostalCode] AS [e.Key.ShipPostalCode], [t].[$inner.Key.ShipCountry] AS [e.Key.ShipCountry], (
    SELECT [d].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[UnitPrice] AS [UnitPrice], [d].[Quantity] AS [Quantity], [d].[Discount] AS [Discount]
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
    WHERE [t].[$inner.Key.OrderID] = [o].[OrderID]
    FOR JSON PATH
) AS [e.Elements]
FROM (
    SELECT [o_0].[OrderID] AS [$outer.OrderID], [o_0].[CustomerID] AS [$outer.CustomerID], [o_0].[EmployeeID] AS [$outer.EmployeeID], [o_0].[OrderDate] AS [$outer.OrderDate], [o_0].[RequiredDate] AS [$outer.RequiredDate], [o_0].[ShippedDate] AS [$outer.ShippedDate], [o_0].[ShipVia] AS [$outer.ShipVia], [o_0].[Freight] AS [$outer.Freight], [o_0].[ShipName] AS [$outer.ShipName], [o_0].[ShipAddress] AS [$outer.ShipAddress], [o_0].[ShipCity] AS [$outer.ShipCity], [o_0].[ShipRegion] AS [$outer.ShipRegion], [o_0].[ShipPostalCode] AS [$outer.ShipPostalCode], [o_0].[ShipCountry] AS [$outer.ShipCountry], [o_0].[OrderID] AS [$inner.Key.OrderID], [o_0].[CustomerID] AS [$inner.Key.CustomerID], [o_0].[EmployeeID] AS [$inner.Key.EmployeeID], [o_0].[OrderDate] AS [$inner.Key.OrderDate], [o_0].[RequiredDate] AS [$inner.Key.RequiredDate], [o_0].[ShippedDate] AS [$inner.Key.ShippedDate], [o_0].[ShipVia] AS [$inner.Key.ShipVia], [o_0].[Freight] AS [$inner.Key.Freight], [o_0].[ShipName] AS [$inner.Key.ShipName], [o_0].[ShipAddress] AS [$inner.Key.ShipAddress], [o_0].[ShipCity] AS [$inner.Key.ShipCity], [o_0].[ShipRegion] AS [$inner.Key.ShipRegion], [o_0].[ShipPostalCode] AS [$inner.Key.ShipPostalCode], [o_0].[ShipCountry] AS [$inner.Key.ShipCountry]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d_0].[OrderID] = [o_0].[OrderID]
    GROUP BY [o_0].[OrderID], [o_0].[CustomerID], [o_0].[EmployeeID], [o_0].[OrderDate], [o_0].[RequiredDate], [o_0].[ShippedDate], [o_0].[ShipVia], [o_0].[Freight], [o_0].[ShipName], [o_0].[ShipAddress], [o_0].[ShipCity], [o_0].[ShipRegion], [o_0].[ShipPostalCode], [o_0].[ShipCountry]
) AS [t]
INNER JOIN [dbo].[Customers] AS [c] ON [t].[$outer.CustomerID] = [c].[CustomerID]",
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy3_navigation_aggregate_key()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy3_navigation_aggregate_result()
        {
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
    WHERE ([t].[$inner.Key.OrderID] = [o_0].[OrderID]) AND ([t].[$inner.Key.ProductID] = [o_0].[ProductID])
) AS [max]
FROM (
    SELECT [o_1].[OrderID] AS [$outer.OrderID], [o_1].[ProductID] AS [$outer.ProductID], [o_1].[UnitPrice] AS [$outer.UnitPrice], [o_1].[Quantity] AS [$outer.Quantity], [o_1].[Discount] AS [$outer.Discount], [o_1].[OrderID] AS [$inner.Key.OrderID], [o_1].[ProductID] AS [$inner.Key.ProductID], [o_1].[UnitPrice] AS [$inner.Key.UnitPrice], [o_1].[Quantity] AS [$inner.Key.Quantity], [o_1].[Discount] AS [$inner.Key.Discount]
    FROM [dbo].[Order Details] AS [o_1]
    GROUP BY [o_1].[OrderID], [o_1].[ProductID], [o_1].[UnitPrice], [o_1].[Quantity], [o_1].[Discount]
) AS [t]
INNER JOIN [dbo].[Orders] AS [o] ON [t].[$outer.OrderID] = [o].[OrderID]",
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy3_navigation_aggregate_key_result()
        {
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
    WHERE [t].[$inner.Key.OrderID] = [o].[OrderID]
) AS [max]
FROM (
    SELECT [o_0].[OrderID] AS [$outer.OrderID], [o_0].[CustomerID] AS [$outer.CustomerID], [o_0].[EmployeeID] AS [$outer.EmployeeID], [o_0].[OrderDate] AS [$outer.OrderDate], [o_0].[RequiredDate] AS [$outer.RequiredDate], [o_0].[ShippedDate] AS [$outer.ShippedDate], [o_0].[ShipVia] AS [$outer.ShipVia], [o_0].[Freight] AS [$outer.Freight], [o_0].[ShipName] AS [$outer.ShipName], [o_0].[ShipAddress] AS [$outer.ShipAddress], [o_0].[ShipCity] AS [$outer.ShipCity], [o_0].[ShipRegion] AS [$outer.ShipRegion], [o_0].[ShipPostalCode] AS [$outer.ShipPostalCode], [o_0].[ShipCountry] AS [$outer.ShipCountry], [o_0].[OrderID] AS [$inner.Key.OrderID], [o_0].[CustomerID] AS [$inner.Key.CustomerID], [o_0].[EmployeeID] AS [$inner.Key.EmployeeID], [o_0].[OrderDate] AS [$inner.Key.OrderDate], [o_0].[RequiredDate] AS [$inner.Key.RequiredDate], [o_0].[ShippedDate] AS [$inner.Key.ShippedDate], [o_0].[ShipVia] AS [$inner.Key.ShipVia], [o_0].[Freight] AS [$inner.Key.Freight], [o_0].[ShipName] AS [$inner.Key.ShipName], [o_0].[ShipAddress] AS [$inner.Key.ShipAddress], [o_0].[ShipCity] AS [$inner.Key.ShipCity], [o_0].[ShipRegion] AS [$inner.Key.ShipRegion], [o_0].[ShipPostalCode] AS [$inner.Key.ShipPostalCode], [o_0].[ShipCountry] AS [$inner.Key.ShipCountry]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d_0].[OrderID] = [o_0].[OrderID]
    GROUP BY [o_0].[OrderID], [o_0].[CustomerID], [o_0].[EmployeeID], [o_0].[OrderDate], [o_0].[RequiredDate], [o_0].[ShippedDate], [o_0].[ShipVia], [o_0].[Freight], [o_0].[ShipName], [o_0].[ShipAddress], [o_0].[ShipCity], [o_0].[ShipRegion], [o_0].[ShipPostalCode], [o_0].[ShipCountry]
) AS [t]
INNER JOIN [dbo].[Customers] AS [c] ON [t].[$outer.CustomerID] = [c].[CustomerID]",
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_intact_key()
        {
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
    WHERE [o].[OrderID] = [o_0].[OrderID]
    FOR JSON PATH
) AS [y.Elements]
FROM [dbo].[Order Details] AS [d_0]
INNER JOIN [dbo].[Orders] AS [o] ON [d_0].[OrderID] = [o].[OrderID]
GROUP BY [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o].[RequiredDate], [o].[ShippedDate], [o].[ShipVia], [o].[Freight], [o].[ShipName], [o].[ShipAddress], [o].[ShipCity], [o].[ShipRegion], [o].[ShipPostalCode], [o].[ShipCountry]",
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_intact_element()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_intact_result()
        {
            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d,
                        d => d,
                        (x, y) => new { x.Order, y });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [Order.OrderID], [o].[CustomerID] AS [Order.CustomerID], [o].[EmployeeID] AS [Order.EmployeeID], [o].[OrderDate] AS [Order.OrderDate], [o].[RequiredDate] AS [Order.RequiredDate], [o].[ShippedDate] AS [Order.ShippedDate], [o].[ShipVia] AS [Order.ShipVia], [o].[Freight] AS [Order.Freight], [o].[ShipName] AS [Order.ShipName], [o].[ShipAddress] AS [Order.ShipAddress], [o].[ShipCity] AS [Order.ShipCity], [o].[ShipRegion] AS [Order.ShipRegion], [o].[ShipPostalCode] AS [Order.ShipPostalCode], [o].[ShipCountry] AS [Order.ShipCountry], [t].[$inner.Key.OrderID] AS [y.Key.OrderID], [t].[$inner.Key.ProductID] AS [y.Key.ProductID], [t].[$inner.Key.UnitPrice] AS [y.Key.UnitPrice], [t].[$inner.Key.Quantity] AS [y.Key.Quantity], [t].[$inner.Key.Discount] AS [y.Key.Discount], (
    SELECT [o_0].[OrderID] AS [OrderID], [o_0].[ProductID] AS [ProductID], [o_0].[UnitPrice] AS [UnitPrice], [o_0].[Quantity] AS [Quantity], [o_0].[Discount] AS [Discount]
    FROM [dbo].[Order Details] AS [o_0]
    WHERE ([t].[$inner.Key.OrderID] = [o_0].[OrderID]) AND ([t].[$inner.Key.ProductID] = [o_0].[ProductID])
    FOR JSON PATH
) AS [y.Elements]
FROM (
    SELECT [o_1].[OrderID] AS [$outer.OrderID], [o_1].[ProductID] AS [$outer.ProductID], [o_1].[UnitPrice] AS [$outer.UnitPrice], [o_1].[Quantity] AS [$outer.Quantity], [o_1].[Discount] AS [$outer.Discount], [o_1].[OrderID] AS [$inner.Key.OrderID], [o_1].[ProductID] AS [$inner.Key.ProductID], [o_1].[UnitPrice] AS [$inner.Key.UnitPrice], [o_1].[Quantity] AS [$inner.Key.Quantity], [o_1].[Discount] AS [$inner.Key.Discount]
    FROM [dbo].[Order Details] AS [o_1]
    GROUP BY [o_1].[OrderID], [o_1].[ProductID], [o_1].[UnitPrice], [o_1].[Quantity], [o_1].[Discount]
) AS [t]
INNER JOIN [dbo].[Orders] AS [o] ON [t].[$outer.OrderID] = [o].[OrderID]",
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_intact_key_element()
        {
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
    WHERE [o].[OrderID] = [o_0].[OrderID]
    FOR JSON PATH
) AS [y.Elements]
FROM [dbo].[Order Details] AS [d_0]
INNER JOIN [dbo].[Orders] AS [o] ON [d_0].[OrderID] = [o].[OrderID]
INNER JOIN [dbo].[Customers] AS [c_0] ON [o].[CustomerID] = [c_0].[CustomerID]
GROUP BY [o].[OrderID], [o].[CustomerID], [o].[EmployeeID], [o].[OrderDate], [o].[RequiredDate], [o].[ShippedDate], [o].[ShipVia], [o].[Freight], [o].[ShipName], [o].[ShipAddress], [o].[ShipCity], [o].[ShipRegion], [o].[ShipPostalCode], [o].[ShipCountry]",
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_intact_key_result()
        {
            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order,
                        d => d,
                        (x, y) => new { x.Customer, y });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [Customer.CustomerID], [c].[CompanyName] AS [Customer.CompanyName], [c].[ContactName] AS [Customer.ContactName], [c].[ContactTitle] AS [Customer.ContactTitle], [c].[Address] AS [Customer.Address], [c].[City] AS [Customer.City], [c].[Region] AS [Customer.Region], [c].[PostalCode] AS [Customer.PostalCode], [c].[Country] AS [Customer.Country], [c].[Phone] AS [Customer.Phone], [c].[Fax] AS [Customer.Fax], [t].[$inner.Key.OrderID] AS [y.Key.OrderID], [t].[$inner.Key.CustomerID] AS [y.Key.CustomerID], [t].[$inner.Key.EmployeeID] AS [y.Key.EmployeeID], [t].[$inner.Key.OrderDate] AS [y.Key.OrderDate], [t].[$inner.Key.RequiredDate] AS [y.Key.RequiredDate], [t].[$inner.Key.ShippedDate] AS [y.Key.ShippedDate], [t].[$inner.Key.ShipVia] AS [y.Key.ShipVia], [t].[$inner.Key.Freight] AS [y.Key.Freight], [t].[$inner.Key.ShipName] AS [y.Key.ShipName], [t].[$inner.Key.ShipAddress] AS [y.Key.ShipAddress], [t].[$inner.Key.ShipCity] AS [y.Key.ShipCity], [t].[$inner.Key.ShipRegion] AS [y.Key.ShipRegion], [t].[$inner.Key.ShipPostalCode] AS [y.Key.ShipPostalCode], [t].[$inner.Key.ShipCountry] AS [y.Key.ShipCountry], (
    SELECT [d].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[UnitPrice] AS [UnitPrice], [d].[Quantity] AS [Quantity], [d].[Discount] AS [Discount]
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
    WHERE [t].[$inner.Key.OrderID] = [o].[OrderID]
    FOR JSON PATH
) AS [y.Elements]
FROM (
    SELECT [o_0].[OrderID] AS [$outer.OrderID], [o_0].[CustomerID] AS [$outer.CustomerID], [o_0].[EmployeeID] AS [$outer.EmployeeID], [o_0].[OrderDate] AS [$outer.OrderDate], [o_0].[RequiredDate] AS [$outer.RequiredDate], [o_0].[ShippedDate] AS [$outer.ShippedDate], [o_0].[ShipVia] AS [$outer.ShipVia], [o_0].[Freight] AS [$outer.Freight], [o_0].[ShipName] AS [$outer.ShipName], [o_0].[ShipAddress] AS [$outer.ShipAddress], [o_0].[ShipCity] AS [$outer.ShipCity], [o_0].[ShipRegion] AS [$outer.ShipRegion], [o_0].[ShipPostalCode] AS [$outer.ShipPostalCode], [o_0].[ShipCountry] AS [$outer.ShipCountry], [o_0].[OrderID] AS [$inner.Key.OrderID], [o_0].[CustomerID] AS [$inner.Key.CustomerID], [o_0].[EmployeeID] AS [$inner.Key.EmployeeID], [o_0].[OrderDate] AS [$inner.Key.OrderDate], [o_0].[RequiredDate] AS [$inner.Key.RequiredDate], [o_0].[ShippedDate] AS [$inner.Key.ShippedDate], [o_0].[ShipVia] AS [$inner.Key.ShipVia], [o_0].[Freight] AS [$inner.Key.Freight], [o_0].[ShipName] AS [$inner.Key.ShipName], [o_0].[ShipAddress] AS [$inner.Key.ShipAddress], [o_0].[ShipCity] AS [$inner.Key.ShipCity], [o_0].[ShipRegion] AS [$inner.Key.ShipRegion], [o_0].[ShipPostalCode] AS [$inner.Key.ShipPostalCode], [o_0].[ShipCountry] AS [$inner.Key.ShipCountry]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d_0].[OrderID] = [o_0].[OrderID]
    GROUP BY [o_0].[OrderID], [o_0].[CustomerID], [o_0].[EmployeeID], [o_0].[OrderDate], [o_0].[RequiredDate], [o_0].[ShippedDate], [o_0].[ShipVia], [o_0].[Freight], [o_0].[ShipName], [o_0].[ShipAddress], [o_0].[ShipCity], [o_0].[ShipRegion], [o_0].[ShipPostalCode], [o_0].[ShipCountry]
) AS [t]
INNER JOIN [dbo].[Customers] AS [c] ON [t].[$outer.CustomerID] = [c].[CustomerID]",
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_intact_element_result()
        {
            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d,
                        d => d.Order.Customer,
                        (x, y) => new { x.Order, y });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID] AS [Order.OrderID], [o].[CustomerID] AS [Order.CustomerID], [o].[EmployeeID] AS [Order.EmployeeID], [o].[OrderDate] AS [Order.OrderDate], [o].[RequiredDate] AS [Order.RequiredDate], [o].[ShippedDate] AS [Order.ShippedDate], [o].[ShipVia] AS [Order.ShipVia], [o].[Freight] AS [Order.Freight], [o].[ShipName] AS [Order.ShipName], [o].[ShipAddress] AS [Order.ShipAddress], [o].[ShipCity] AS [Order.ShipCity], [o].[ShipRegion] AS [Order.ShipRegion], [o].[ShipPostalCode] AS [Order.ShipPostalCode], [o].[ShipCountry] AS [Order.ShipCountry], [t].[$inner.Key.OrderID] AS [y.Key.OrderID], [t].[$inner.Key.ProductID] AS [y.Key.ProductID], [t].[$inner.Key.UnitPrice] AS [y.Key.UnitPrice], [t].[$inner.Key.Quantity] AS [y.Key.Quantity], [t].[$inner.Key.Discount] AS [y.Key.Discount], (
    SELECT [c].[CustomerID] AS [CustomerID], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Address] AS [Address], [c].[City] AS [City], [c].[Region] AS [Region], [c].[PostalCode] AS [PostalCode], [c].[Country] AS [Country], [c].[Phone] AS [Phone], [c].[Fax] AS [Fax]
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d].[OrderID] = [o_0].[OrderID]
    INNER JOIN [dbo].[Customers] AS [c] ON [o_0].[CustomerID] = [c].[CustomerID]
    WHERE ([t].[$inner.Key.OrderID] = [d].[OrderID]) AND ([t].[$inner.Key.ProductID] = [d].[ProductID])
    FOR JSON PATH
) AS [y.Elements]
FROM (
    SELECT [d_0].[OrderID] AS [$outer.OrderID], [d_0].[ProductID] AS [$outer.ProductID], [d_0].[UnitPrice] AS [$outer.UnitPrice], [d_0].[Quantity] AS [$outer.Quantity], [d_0].[Discount] AS [$outer.Discount], [d_0].[OrderID] AS [$inner.Key.OrderID], [d_0].[ProductID] AS [$inner.Key.ProductID], [d_0].[UnitPrice] AS [$inner.Key.UnitPrice], [d_0].[Quantity] AS [$inner.Key.Quantity], [d_0].[Discount] AS [$inner.Key.Discount]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_1] ON [d_0].[OrderID] = [o_1].[OrderID]
    INNER JOIN [dbo].[Customers] AS [c_0] ON [o_1].[CustomerID] = [c_0].[CustomerID]
    GROUP BY [d_0].[OrderID], [d_0].[ProductID], [d_0].[UnitPrice], [d_0].[Quantity], [d_0].[Discount]
) AS [t]
INNER JOIN [dbo].[Orders] AS [o] ON [t].[$outer.OrderID] = [o].[OrderID]",
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_intact_key_element_result()
        {
            var query
                = context.OrderDetails
                    .GroupBy(
                        d => d.Order,
                        d => d.Order.Customer,
                        (x, y) => new { x.Customer, y });

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[CustomerID] AS [Customer.CustomerID], [c].[CompanyName] AS [Customer.CompanyName], [c].[ContactName] AS [Customer.ContactName], [c].[ContactTitle] AS [Customer.ContactTitle], [c].[Address] AS [Customer.Address], [c].[City] AS [Customer.City], [c].[Region] AS [Customer.Region], [c].[PostalCode] AS [Customer.PostalCode], [c].[Country] AS [Customer.Country], [c].[Phone] AS [Customer.Phone], [c].[Fax] AS [Customer.Fax], [t].[$inner.Key.OrderID] AS [y.Key.OrderID], [t].[$inner.Key.CustomerID] AS [y.Key.CustomerID], [t].[$inner.Key.EmployeeID] AS [y.Key.EmployeeID], [t].[$inner.Key.OrderDate] AS [y.Key.OrderDate], [t].[$inner.Key.RequiredDate] AS [y.Key.RequiredDate], [t].[$inner.Key.ShippedDate] AS [y.Key.ShippedDate], [t].[$inner.Key.ShipVia] AS [y.Key.ShipVia], [t].[$inner.Key.Freight] AS [y.Key.Freight], [t].[$inner.Key.ShipName] AS [y.Key.ShipName], [t].[$inner.Key.ShipAddress] AS [y.Key.ShipAddress], [t].[$inner.Key.ShipCity] AS [y.Key.ShipCity], [t].[$inner.Key.ShipRegion] AS [y.Key.ShipRegion], [t].[$inner.Key.ShipPostalCode] AS [y.Key.ShipPostalCode], [t].[$inner.Key.ShipCountry] AS [y.Key.ShipCountry], (
    SELECT [c_0].[CustomerID] AS [CustomerID], [c_0].[CompanyName] AS [CompanyName], [c_0].[ContactName] AS [ContactName], [c_0].[ContactTitle] AS [ContactTitle], [c_0].[Address] AS [Address], [c_0].[City] AS [City], [c_0].[Region] AS [Region], [c_0].[PostalCode] AS [PostalCode], [c_0].[Country] AS [Country], [c_0].[Phone] AS [Phone], [c_0].[Fax] AS [Fax]
    FROM [dbo].[Order Details] AS [d]
    INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
    INNER JOIN [dbo].[Customers] AS [c_0] ON [o].[CustomerID] = [c_0].[CustomerID]
    WHERE [t].[$inner.Key.OrderID] = [o].[OrderID]
    FOR JSON PATH
) AS [y.Elements]
FROM (
    SELECT [o_0].[OrderID] AS [$outer.OrderID], [o_0].[CustomerID] AS [$outer.CustomerID], [o_0].[EmployeeID] AS [$outer.EmployeeID], [o_0].[OrderDate] AS [$outer.OrderDate], [o_0].[RequiredDate] AS [$outer.RequiredDate], [o_0].[ShippedDate] AS [$outer.ShippedDate], [o_0].[ShipVia] AS [$outer.ShipVia], [o_0].[Freight] AS [$outer.Freight], [o_0].[ShipName] AS [$outer.ShipName], [o_0].[ShipAddress] AS [$outer.ShipAddress], [o_0].[ShipCity] AS [$outer.ShipCity], [o_0].[ShipRegion] AS [$outer.ShipRegion], [o_0].[ShipPostalCode] AS [$outer.ShipPostalCode], [o_0].[ShipCountry] AS [$outer.ShipCountry], [o_0].[OrderID] AS [$inner.Key.OrderID], [o_0].[CustomerID] AS [$inner.Key.CustomerID], [o_0].[EmployeeID] AS [$inner.Key.EmployeeID], [o_0].[OrderDate] AS [$inner.Key.OrderDate], [o_0].[RequiredDate] AS [$inner.Key.RequiredDate], [o_0].[ShippedDate] AS [$inner.Key.ShippedDate], [o_0].[ShipVia] AS [$inner.Key.ShipVia], [o_0].[Freight] AS [$inner.Key.Freight], [o_0].[ShipName] AS [$inner.Key.ShipName], [o_0].[ShipAddress] AS [$inner.Key.ShipAddress], [o_0].[ShipCity] AS [$inner.Key.ShipCity], [o_0].[ShipRegion] AS [$inner.Key.ShipRegion], [o_0].[ShipPostalCode] AS [$inner.Key.ShipPostalCode], [o_0].[ShipCountry] AS [$inner.Key.ShipCountry]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d_0].[OrderID] = [o_0].[OrderID]
    INNER JOIN [dbo].[Customers] AS [c_1] ON [o_0].[CustomerID] = [c_1].[CustomerID]
    GROUP BY [o_0].[OrderID], [o_0].[CustomerID], [o_0].[EmployeeID], [o_0].[OrderDate], [o_0].[RequiredDate], [o_0].[ShippedDate], [o_0].[ShipVia], [o_0].[Freight], [o_0].[ShipName], [o_0].[ShipAddress], [o_0].[ShipCity], [o_0].[ShipRegion], [o_0].[ShipPostalCode], [o_0].[ShipCountry]
) AS [t]
INNER JOIN [dbo].[Customers] AS [c] ON [t].[$outer.CustomerID] = [c].[CustomerID]",
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_aggregate_key()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_aggregate_element()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_aggregate_result()
        {
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
    WHERE [t].[$inner.Key.OrderID] = [o].[OrderID]
) AS [y]
FROM (
    SELECT [o_0].[OrderID] AS [$outer.OrderID], [o_0].[CustomerID] AS [$outer.CustomerID], [o_0].[EmployeeID] AS [$outer.EmployeeID], [o_0].[OrderDate] AS [$outer.OrderDate], [o_0].[RequiredDate] AS [$outer.RequiredDate], [o_0].[ShippedDate] AS [$outer.ShippedDate], [o_0].[ShipVia] AS [$outer.ShipVia], [o_0].[Freight] AS [$outer.Freight], [o_0].[ShipName] AS [$outer.ShipName], [o_0].[ShipAddress] AS [$outer.ShipAddress], [o_0].[ShipCity] AS [$outer.ShipCity], [o_0].[ShipRegion] AS [$outer.ShipRegion], [o_0].[ShipPostalCode] AS [$outer.ShipPostalCode], [o_0].[ShipCountry] AS [$outer.ShipCountry], [o_0].[OrderID] AS [$inner.Key.OrderID], [o_0].[CustomerID] AS [$inner.Key.CustomerID], [o_0].[EmployeeID] AS [$inner.Key.EmployeeID], [o_0].[OrderDate] AS [$inner.Key.OrderDate], [o_0].[RequiredDate] AS [$inner.Key.RequiredDate], [o_0].[ShippedDate] AS [$inner.Key.ShippedDate], [o_0].[ShipVia] AS [$inner.Key.ShipVia], [o_0].[Freight] AS [$inner.Key.Freight], [o_0].[ShipName] AS [$inner.Key.ShipName], [o_0].[ShipAddress] AS [$inner.Key.ShipAddress], [o_0].[ShipCity] AS [$inner.Key.ShipCity], [o_0].[ShipRegion] AS [$inner.Key.ShipRegion], [o_0].[ShipPostalCode] AS [$inner.Key.ShipPostalCode], [o_0].[ShipCountry] AS [$inner.Key.ShipCountry]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d_0].[OrderID] = [o_0].[OrderID]
    GROUP BY [o_0].[OrderID], [o_0].[CustomerID], [o_0].[EmployeeID], [o_0].[OrderDate], [o_0].[RequiredDate], [o_0].[ShippedDate], [o_0].[ShipVia], [o_0].[Freight], [o_0].[ShipName], [o_0].[ShipAddress], [o_0].[ShipCity], [o_0].[ShipRegion], [o_0].[ShipPostalCode], [o_0].[ShipCountry]
) AS [t]
INNER JOIN [dbo].[Customers] AS [c] ON [t].[$outer.CustomerID] = [c].[CustomerID]",
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_aggregate_key_element()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_aggregate_key_result()
        {
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
    WHERE [t].[$inner.Key.OrderID] = [o].[OrderID]
) AS [y]
FROM (
    SELECT [o_0].[OrderID] AS [$outer.OrderID], [o_0].[CustomerID] AS [$outer.CustomerID], [o_0].[EmployeeID] AS [$outer.EmployeeID], [o_0].[OrderDate] AS [$outer.OrderDate], [o_0].[RequiredDate] AS [$outer.RequiredDate], [o_0].[ShippedDate] AS [$outer.ShippedDate], [o_0].[ShipVia] AS [$outer.ShipVia], [o_0].[Freight] AS [$outer.Freight], [o_0].[ShipName] AS [$outer.ShipName], [o_0].[ShipAddress] AS [$outer.ShipAddress], [o_0].[ShipCity] AS [$outer.ShipCity], [o_0].[ShipRegion] AS [$outer.ShipRegion], [o_0].[ShipPostalCode] AS [$outer.ShipPostalCode], [o_0].[ShipCountry] AS [$outer.ShipCountry], [o_0].[OrderID] AS [$inner.Key.OrderID], [o_0].[CustomerID] AS [$inner.Key.CustomerID], [o_0].[EmployeeID] AS [$inner.Key.EmployeeID], [o_0].[OrderDate] AS [$inner.Key.OrderDate], [o_0].[RequiredDate] AS [$inner.Key.RequiredDate], [o_0].[ShippedDate] AS [$inner.Key.ShippedDate], [o_0].[ShipVia] AS [$inner.Key.ShipVia], [o_0].[Freight] AS [$inner.Key.Freight], [o_0].[ShipName] AS [$inner.Key.ShipName], [o_0].[ShipAddress] AS [$inner.Key.ShipAddress], [o_0].[ShipCity] AS [$inner.Key.ShipCity], [o_0].[ShipRegion] AS [$inner.Key.ShipRegion], [o_0].[ShipPostalCode] AS [$inner.Key.ShipPostalCode], [o_0].[ShipCountry] AS [$inner.Key.ShipCountry]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d_0].[OrderID] = [o_0].[OrderID]
    GROUP BY [o_0].[OrderID], [o_0].[CustomerID], [o_0].[EmployeeID], [o_0].[OrderDate], [o_0].[RequiredDate], [o_0].[ShippedDate], [o_0].[ShipVia], [o_0].[Freight], [o_0].[ShipName], [o_0].[ShipAddress], [o_0].[ShipCity], [o_0].[ShipRegion], [o_0].[ShipPostalCode], [o_0].[ShipCountry]
) AS [t]
INNER JOIN [dbo].[Customers] AS [c] ON [t].[$outer.CustomerID] = [c].[CustomerID]",
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_aggregate_element_result()
        {
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
    WHERE ([t].[$inner.Key.OrderID] = [d].[OrderID]) AND ([t].[$inner.Key.ProductID] = [d].[ProductID])
) AS [y]
FROM (
    SELECT [d_0].[OrderID] AS [$outer.OrderID], [d_0].[ProductID] AS [$outer.ProductID], [d_0].[UnitPrice] AS [$outer.UnitPrice], [d_0].[Quantity] AS [$outer.Quantity], [d_0].[Discount] AS [$outer.Discount], [d_0].[OrderID] AS [$inner.Key.OrderID], [d_0].[ProductID] AS [$inner.Key.ProductID], [d_0].[UnitPrice] AS [$inner.Key.UnitPrice], [d_0].[Quantity] AS [$inner.Key.Quantity], [d_0].[Discount] AS [$inner.Key.Discount]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_1] ON [d_0].[OrderID] = [o_1].[OrderID]
    GROUP BY [d_0].[OrderID], [d_0].[ProductID], [d_0].[UnitPrice], [d_0].[Quantity], [d_0].[Discount]
) AS [t]
INNER JOIN [dbo].[Orders] AS [o] ON [t].[$outer.OrderID] = [o].[OrderID]",
                context.SqlLog);
        }

        [TestMethod]
        public void GroupBy4_navigation_aggregate_key_element_result()
        {
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
    WHERE [t].[$inner.Key.OrderID] = [o].[OrderID]
) AS [y]
FROM (
    SELECT [o_0].[OrderID] AS [$outer.OrderID], [o_0].[CustomerID] AS [$outer.CustomerID], [o_0].[EmployeeID] AS [$outer.EmployeeID], [o_0].[OrderDate] AS [$outer.OrderDate], [o_0].[RequiredDate] AS [$outer.RequiredDate], [o_0].[ShippedDate] AS [$outer.ShippedDate], [o_0].[ShipVia] AS [$outer.ShipVia], [o_0].[Freight] AS [$outer.Freight], [o_0].[ShipName] AS [$outer.ShipName], [o_0].[ShipAddress] AS [$outer.ShipAddress], [o_0].[ShipCity] AS [$outer.ShipCity], [o_0].[ShipRegion] AS [$outer.ShipRegion], [o_0].[ShipPostalCode] AS [$outer.ShipPostalCode], [o_0].[ShipCountry] AS [$outer.ShipCountry], [o_0].[OrderID] AS [$inner.Key.OrderID], [o_0].[CustomerID] AS [$inner.Key.CustomerID], [o_0].[EmployeeID] AS [$inner.Key.EmployeeID], [o_0].[OrderDate] AS [$inner.Key.OrderDate], [o_0].[RequiredDate] AS [$inner.Key.RequiredDate], [o_0].[ShippedDate] AS [$inner.Key.ShippedDate], [o_0].[ShipVia] AS [$inner.Key.ShipVia], [o_0].[Freight] AS [$inner.Key.Freight], [o_0].[ShipName] AS [$inner.Key.ShipName], [o_0].[ShipAddress] AS [$inner.Key.ShipAddress], [o_0].[ShipCity] AS [$inner.Key.ShipCity], [o_0].[ShipRegion] AS [$inner.Key.ShipRegion], [o_0].[ShipPostalCode] AS [$inner.Key.ShipPostalCode], [o_0].[ShipCountry] AS [$inner.Key.ShipCountry]
    FROM [dbo].[Order Details] AS [d_0]
    INNER JOIN [dbo].[Orders] AS [o_0] ON [d_0].[OrderID] = [o_0].[OrderID]
    GROUP BY [o_0].[OrderID], [o_0].[CustomerID], [o_0].[EmployeeID], [o_0].[OrderDate], [o_0].[RequiredDate], [o_0].[ShippedDate], [o_0].[ShipVia], [o_0].[Freight], [o_0].[ShipName], [o_0].[ShipAddress], [o_0].[ShipCity], [o_0].[ShipRegion], [o_0].[ShipPostalCode], [o_0].[ShipCountry]
) AS [t]
INNER JOIN [dbo].[Customers] AS [c] ON [t].[$outer.CustomerID] = [c].[CustomerID]",
                context.SqlLog);
        }

        // Extra GroupBy tests: navigations within aggregations, etc.

        [TestMethod]
        public void GroupBy1_navigation_aggregate_navigation()
        {
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
                context.SqlLog);
        }

        [TestMethod]
        public void Zip_navigation()
        {
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
                @"SELECT [t].[$inner.City] AS [c1], [t_0].[$inner.City] AS [c2]
FROM (
    SELECT [d].[OrderID] AS [$outer.$outer.OrderID], [d].[ProductID] AS [$outer.$outer.ProductID], [d].[UnitPrice] AS [$outer.$outer.UnitPrice], [d].[Quantity] AS [$outer.$outer.Quantity], [d].[Discount] AS [$outer.$outer.Discount], [o].[OrderID] AS [$outer.$inner.OrderID], [o].[CustomerID] AS [$outer.$inner.CustomerID], [o].[EmployeeID] AS [$outer.$inner.EmployeeID], [o].[OrderDate] AS [$outer.$inner.OrderDate], [o].[RequiredDate] AS [$outer.$inner.RequiredDate], [o].[ShippedDate] AS [$outer.$inner.ShippedDate], [o].[ShipVia] AS [$outer.$inner.ShipVia], [o].[Freight] AS [$outer.$inner.Freight], [o].[ShipName] AS [$outer.$inner.ShipName], [o].[ShipAddress] AS [$outer.$inner.ShipAddress], [o].[ShipCity] AS [$outer.$inner.ShipCity], [o].[ShipRegion] AS [$outer.$inner.ShipRegion], [o].[ShipPostalCode] AS [$outer.$inner.ShipPostalCode], [o].[ShipCountry] AS [$outer.$inner.ShipCountry], [c].[CustomerID] AS [$inner.CustomerID], [c].[CompanyName] AS [$inner.CompanyName], [c].[ContactName] AS [$inner.ContactName], [c].[ContactTitle] AS [$inner.ContactTitle], [c].[Address] AS [$inner.Address], [c].[City] AS [$inner.City], [c].[Region] AS [$inner.Region], [c].[PostalCode] AS [$inner.PostalCode], [c].[Country] AS [$inner.Country], [c].[Phone] AS [$inner.Phone], [c].[Fax] AS [$inner.Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
    FROM (
        SELECT TOP (10) [d_0].[OrderID] AS [OrderID], [d_0].[ProductID] AS [ProductID], [d_0].[UnitPrice] AS [UnitPrice], [d_0].[Quantity] AS [Quantity], [d_0].[Discount] AS [Discount]
        FROM [dbo].[Order Details] AS [d_0]
    ) AS [d]
    INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
    INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
) AS [t]
INNER JOIN (
    SELECT [o_0].[OrderID] AS [$outer.OrderID], [o_0].[CustomerID] AS [$outer.CustomerID], [o_0].[EmployeeID] AS [$outer.EmployeeID], [o_0].[OrderDate] AS [$outer.OrderDate], [o_0].[RequiredDate] AS [$outer.RequiredDate], [o_0].[ShippedDate] AS [$outer.ShippedDate], [o_0].[ShipVia] AS [$outer.ShipVia], [o_0].[Freight] AS [$outer.Freight], [o_0].[ShipName] AS [$outer.ShipName], [o_0].[ShipAddress] AS [$outer.ShipAddress], [o_0].[ShipCity] AS [$outer.ShipCity], [o_0].[ShipRegion] AS [$outer.ShipRegion], [o_0].[ShipPostalCode] AS [$outer.ShipPostalCode], [o_0].[ShipCountry] AS [$outer.ShipCountry], [c_0].[CustomerID] AS [$inner.CustomerID], [c_0].[CompanyName] AS [$inner.CompanyName], [c_0].[ContactName] AS [$inner.ContactName], [c_0].[ContactTitle] AS [$inner.ContactTitle], [c_0].[Address] AS [$inner.Address], [c_0].[City] AS [$inner.City], [c_0].[Region] AS [$inner.Region], [c_0].[PostalCode] AS [$inner.PostalCode], [c_0].[Country] AS [$inner.Country], [c_0].[Phone] AS [$inner.Phone], [c_0].[Fax] AS [$inner.Fax], ROW_NUMBER() OVER(ORDER BY (SELECT 1) ASC) AS [$rownumber]
    FROM (
        SELECT TOP (10) [o_1].[OrderID] AS [OrderID], [o_1].[CustomerID] AS [CustomerID], [o_1].[EmployeeID] AS [EmployeeID], [o_1].[OrderDate] AS [OrderDate], [o_1].[RequiredDate] AS [RequiredDate], [o_1].[ShippedDate] AS [ShippedDate], [o_1].[ShipVia] AS [ShipVia], [o_1].[Freight] AS [Freight], [o_1].[ShipName] AS [ShipName], [o_1].[ShipAddress] AS [ShipAddress], [o_1].[ShipCity] AS [ShipCity], [o_1].[ShipRegion] AS [ShipRegion], [o_1].[ShipPostalCode] AS [ShipPostalCode], [o_1].[ShipCountry] AS [ShipCountry]
        FROM [dbo].[Orders] AS [o_1]
    ) AS [o_0]
    INNER JOIN [dbo].[Customers] AS [c_0] ON [o_0].[CustomerID] = [c_0].[CustomerID]
) AS [t_0] ON [t].[$rownumber] = [t_0].[$rownumber]",
                context.SqlLog);
        }
    }
}
