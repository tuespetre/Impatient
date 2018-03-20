using Impatient.Tests.Northwind;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Impatient.Tests
{
    [TestClass]
    public class EFCoreTests
    {
        [TestMethod]
        public void TestEfCore_Basic()
        {
            EfCoreTestCase((context, log) =>
            {
                var result = (from o in context.Set<Order>()
                              select new { o, o.Customer }).FirstOrDefault();

                Assert.IsNotNull(result);
                Assert.IsNotNull(result.o);
                Assert.IsNotNull(result.Customer);

                Assert.AreEqual(@"
SELECT TOP (1) [o].[OrderID] AS [o.OrderID], [o].[CustomerID] AS [o.CustomerID], [o].[EmployeeID] AS [o.EmployeeID], [o].[Freight] AS [o.Freight], [o].[OrderDate] AS [o.OrderDate], [o].[RequiredDate] AS [o.RequiredDate], [o].[ShipAddress] AS [o.ShipAddress], [o].[ShipCity] AS [o.ShipCity], [o].[ShipCountry] AS [o.ShipCountry], [o].[ShipName] AS [o.ShipName], [o].[ShipPostalCode] AS [o.ShipPostalCode], [o].[ShipRegion] AS [o.ShipRegion], [o].[ShipVia] AS [o.ShipVia], [o].[ShippedDate] AS [o.ShippedDate], [t].[CustomerID] AS [Customer.CustomerID], [t].[Address] AS [Customer.Address], [t].[City] AS [Customer.City], [t].[CompanyName] AS [Customer.CompanyName], [t].[ContactName] AS [Customer.ContactName], [t].[ContactTitle] AS [Customer.ContactTitle], [t].[Country] AS [Customer.Country], [t].[Fax] AS [Customer.Fax], [t].[Phone] AS [Customer.Phone], [t].[PostalCode] AS [Customer.PostalCode], [t].[Region] AS [Customer.Region]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [t] ON [o].[CustomerID] = [t].[CustomerID]
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void Tracking_Basic()
        {
            EfCoreTestCase((context, log) =>
            {
                var order1 = context.Set<Order>().FirstOrDefault();
                var order2 = context.Set<Order>().FirstOrDefault();

                Assert.AreEqual(1, context.ChangeTracker.Entries().Count());
                Assert.IsNotNull(order1);
                Assert.IsNotNull(order2);
                Assert.AreEqual(order1, order2);
            });
        }

        [TestMethod]
        public async Task FirstOrDefaultAsync()
        {
            await EfCoreTestCaseAsync(async (context, log) =>
            {
                var order = await context.Set<Order>().FirstOrDefaultAsync();

                Assert.IsNotNull(order);
                Assert.AreEqual(1, context.ChangeTracker.Entries().Count());

                Assert.AreEqual(@"
SELECT TOP (1) [t].[OrderID] AS [OrderID], [t].[CustomerID] AS [CustomerID], [t].[EmployeeID] AS [EmployeeID], [t].[Freight] AS [Freight], [t].[OrderDate] AS [OrderDate], [t].[RequiredDate] AS [RequiredDate], [t].[ShipAddress] AS [ShipAddress], [t].[ShipCity] AS [ShipCity], [t].[ShipCountry] AS [ShipCountry], [t].[ShipName] AS [ShipName], [t].[ShipPostalCode] AS [ShipPostalCode], [t].[ShipRegion] AS [ShipRegion], [t].[ShipVia] AS [ShipVia], [t].[ShippedDate] AS [ShippedDate]
FROM [dbo].[Orders] AS [t]
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public async Task FirstOrDefaultAsync_Predicate()
        {
            await EfCoreTestCaseAsync(async (context, log) =>
            {
                var order = await context.Set<Order>().FirstOrDefaultAsync(o => o.OrderID == 10252);

                Assert.IsNotNull(order);
                Assert.AreEqual(1, context.ChangeTracker.Entries().Count());

                Assert.AreEqual(@"
SELECT TOP (1) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[Freight] AS [Freight], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipCountry] AS [ShipCountry], [o].[ShipName] AS [ShipName], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipRegion] AS [ShipRegion], [o].[ShipVia] AS [ShipVia], [o].[ShippedDate] AS [ShippedDate]
FROM [dbo].[Orders] AS [o]
WHERE [o].[OrderID] = 10252
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public async Task ToListAsync()
        {
            await EfCoreTestCaseAsync(async (context, log) =>
            {
                var orders = await context.Set<Order>().Where(o => o.OrderID == 10252).ToListAsync();

                Assert.AreEqual(1, orders.Count);
                Assert.AreEqual(1, context.ChangeTracker.Entries().Count());

                Assert.AreEqual(@"
SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[Freight] AS [Freight], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipCountry] AS [ShipCountry], [o].[ShipName] AS [ShipName], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipRegion] AS [ShipRegion], [o].[ShipVia] AS [ShipVia], [o].[ShippedDate] AS [ShippedDate]
FROM [dbo].[Orders] AS [o]
WHERE [o].[OrderID] = 10252
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void Tracking_Nested_Complex()
        {
            EfCoreTestCase((context, log) =>
            {
                var results = (from o in context.Set<Order>()
                               let cs = context.Set<Customer>().Where(c => c.CustomerID.StartsWith("A")).Take(1).ToArray()
                               select new { o, cs }).Take(2).ToArray();

                Assert.AreEqual(3, context.ChangeTracker.Entries().Count());
                Assert.AreEqual(results[0].cs.Single(), results[1].cs.Single());
            });
        }

        [TestMethod]
        public void Tracking_Basic_AsNoTracking()
        {
            EfCoreTestCase((context, log) =>
            {
                var order1 = context.Set<Order>().AsNoTracking().Where(o => o.OrderID == 10252).FirstOrDefault();
                var order2 = context.Set<Order>().AsNoTracking().Where(o => o.OrderID == 10252).FirstOrDefault();

                Assert.AreEqual(0, context.ChangeTracker.Entries().Count());
                Assert.IsNotNull(order1);
                Assert.IsNotNull(order2);
                Assert.AreEqual(order1, order2);

                Assert.AreEqual(@"
SELECT TOP (1) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[Freight] AS [Freight], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipCountry] AS [ShipCountry], [o].[ShipName] AS [ShipName], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipRegion] AS [ShipRegion], [o].[ShipVia] AS [ShipVia], [o].[ShippedDate] AS [ShippedDate]
FROM [dbo].[Orders] AS [o]
WHERE [o].[OrderID] = 10252

SELECT TOP (1) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[Freight] AS [Freight], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipCountry] AS [ShipCountry], [o].[ShipName] AS [ShipName], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipRegion] AS [ShipRegion], [o].[ShipVia] AS [ShipVia], [o].[ShippedDate] AS [ShippedDate]
FROM [dbo].[Orders] AS [o]
WHERE [o].[OrderID] = 10252
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void Include_ManyToOne()
        {
            EfCoreTestCase((context, log) =>
            {
                var result = context.Set<Order>().Include(o => o.Customer).FirstOrDefault();

                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Customer);

                Assert.AreEqual(@"
SELECT TOP (1) [t].[OrderID] AS [OrderID], [t].[CustomerID] AS [CustomerID], [t].[EmployeeID] AS [EmployeeID], [t].[Freight] AS [Freight], [t].[OrderDate] AS [OrderDate], [t].[RequiredDate] AS [RequiredDate], [t].[ShipAddress] AS [ShipAddress], [t].[ShipCity] AS [ShipCity], [t].[ShipCountry] AS [ShipCountry], [t].[ShipName] AS [ShipName], [t].[ShipPostalCode] AS [ShipPostalCode], [t].[ShipRegion] AS [ShipRegion], [t].[ShipVia] AS [ShipVia], [t].[ShippedDate] AS [ShippedDate], [t_0].[CustomerID] AS [Customer.CustomerID], [t_0].[Address] AS [Customer.Address], [t_0].[City] AS [Customer.City], [t_0].[CompanyName] AS [Customer.CompanyName], [t_0].[ContactName] AS [Customer.ContactName], [t_0].[ContactTitle] AS [Customer.ContactTitle], [t_0].[Country] AS [Customer.Country], [t_0].[Fax] AS [Customer.Fax], [t_0].[Phone] AS [Customer.Phone], [t_0].[PostalCode] AS [Customer.PostalCode], [t_0].[Region] AS [Customer.Region]
FROM [dbo].[Orders] AS [t]
INNER JOIN [dbo].[Customers] AS [t_0] ON [t].[CustomerID] = [t_0].[CustomerID]
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void Include_ManyToOne_ThenInclude_ManyToOne()
        {
            EfCoreTestCase((context, log) =>
            {
                var result = context.Set<OrderDetail>().Include(d => d.Order).ThenInclude(o => o.Customer).FirstOrDefault();

                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Order?.Customer);

                Assert.AreEqual(@"
SELECT TOP (1) [t].[OrderID] AS [OrderID], [t].[ProductID] AS [ProductID], [t].[Discount] AS [Discount], [t].[Quantity] AS [Quantity], [t].[UnitPrice] AS [UnitPrice], [t_0].[OrderID] AS [Order.OrderID], [t_0].[CustomerID] AS [Order.CustomerID], [t_0].[EmployeeID] AS [Order.EmployeeID], [t_0].[Freight] AS [Order.Freight], [t_0].[OrderDate] AS [Order.OrderDate], [t_0].[RequiredDate] AS [Order.RequiredDate], [t_0].[ShipAddress] AS [Order.ShipAddress], [t_0].[ShipCity] AS [Order.ShipCity], [t_0].[ShipCountry] AS [Order.ShipCountry], [t_0].[ShipName] AS [Order.ShipName], [t_0].[ShipPostalCode] AS [Order.ShipPostalCode], [t_0].[ShipRegion] AS [Order.ShipRegion], [t_0].[ShipVia] AS [Order.ShipVia], [t_0].[ShippedDate] AS [Order.ShippedDate], [t_1].[CustomerID] AS [Order.Customer.CustomerID], [t_1].[Address] AS [Order.Customer.Address], [t_1].[City] AS [Order.Customer.City], [t_1].[CompanyName] AS [Order.Customer.CompanyName], [t_1].[ContactName] AS [Order.Customer.ContactName], [t_1].[ContactTitle] AS [Order.Customer.ContactTitle], [t_1].[Country] AS [Order.Customer.Country], [t_1].[Fax] AS [Order.Customer.Fax], [t_1].[Phone] AS [Order.Customer.Phone], [t_1].[PostalCode] AS [Order.Customer.PostalCode], [t_1].[Region] AS [Order.Customer.Region]
FROM [dbo].[Order Details] AS [t]
INNER JOIN [dbo].[Orders] AS [t_0] ON [t].[OrderID] = [t_0].[OrderID]
INNER JOIN [dbo].[Customers] AS [t_1] ON [t_0].[CustomerID] = [t_1].[CustomerID]
WHERE [t].[UnitPrice] >= 5.00
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void Include_ManyToOne_Include_ManyToOne()
        {
            EfCoreTestCase((context, log) =>
            {
                var result = context.Set<OrderDetail>().Include(d => d.Order).Include(d => d.Product).FirstOrDefault();

                Assert.IsNotNull(result);
                Assert.IsNotNull(result.Order);
                Assert.IsNotNull(result.Product);

                Assert.AreEqual(@"
SELECT TOP (1) [t].[OrderID] AS [OrderID], [t].[ProductID] AS [ProductID], [t].[Discount] AS [Discount], [t].[Quantity] AS [Quantity], [t].[UnitPrice] AS [UnitPrice], [t_0].[OrderID] AS [Order.OrderID], [t_0].[CustomerID] AS [Order.CustomerID], [t_0].[EmployeeID] AS [Order.EmployeeID], [t_0].[Freight] AS [Order.Freight], [t_0].[OrderDate] AS [Order.OrderDate], [t_0].[RequiredDate] AS [Order.RequiredDate], [t_0].[ShipAddress] AS [Order.ShipAddress], [t_0].[ShipCity] AS [Order.ShipCity], [t_0].[ShipCountry] AS [Order.ShipCountry], [t_0].[ShipName] AS [Order.ShipName], [t_0].[ShipPostalCode] AS [Order.ShipPostalCode], [t_0].[ShipRegion] AS [Order.ShipRegion], [t_0].[ShipVia] AS [Order.ShipVia], [t_0].[ShippedDate] AS [Order.ShippedDate], [t_1].[ProductID] AS [Product.Item1], [t_1].[CategoryID] AS [Product.Item2], [t_1].[Discontinued] AS [Product.Item3], [t_1].[ProductName] AS [Product.Item4], [t_1].[SupplierID] AS [Product.Item5], [t_1].[QuantityPerUnit] AS [Product.Item6], [t_1].[ReorderLevel] AS [Product.Item7], [t_1].[UnitPrice] AS [Product.Rest.Item1], [t_1].[UnitsInStock] AS [Product.Rest.Item2], [t_1].[UnitsOnOrder] AS [Product.Rest.Item3]
FROM [dbo].[Order Details] AS [t]
INNER JOIN [dbo].[Orders] AS [t_0] ON [t].[OrderID] = [t_0].[OrderID]
INNER JOIN [dbo].[Products] AS [t_1] ON [t].[ProductID] = [t_1].[ProductID]
WHERE [t].[UnitPrice] >= 5.00
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void Include_OneToMany()
        {
            EfCoreTestCase((context, log) =>
            {
                var result = context.Set<Customer>().Include(c => c.Orders).FirstOrDefault();

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Orders.Any());

                Assert.AreEqual(@"
SELECT TOP (1) [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region], (
    SELECT [t_0].[OrderID] AS [OrderID], [t_0].[CustomerID] AS [CustomerID], [t_0].[EmployeeID] AS [EmployeeID], [t_0].[Freight] AS [Freight], [t_0].[OrderDate] AS [OrderDate], [t_0].[RequiredDate] AS [RequiredDate], [t_0].[ShipAddress] AS [ShipAddress], [t_0].[ShipCity] AS [ShipCity], [t_0].[ShipCountry] AS [ShipCountry], [t_0].[ShipName] AS [ShipName], [t_0].[ShipPostalCode] AS [ShipPostalCode], [t_0].[ShipRegion] AS [ShipRegion], [t_0].[ShipVia] AS [ShipVia], [t_0].[ShippedDate] AS [ShippedDate]
    FROM [dbo].[Orders] AS [t_0]
    WHERE [t].[CustomerID] = [t_0].[CustomerID]
    FOR JSON PATH, INCLUDE_NULL_VALUES
) AS [Orders]
FROM [dbo].[Customers] AS [t]
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void Include_OneToMany_ThenInclude_OneToMany()
        {
            EfCoreTestCase((context, log) =>
            {
                var result = context.Set<Customer>().Include(c => c.Orders).ThenInclude(o => o.OrderDetails).FirstOrDefault();

                Assert.IsNotNull(result);
                Assert.IsTrue(result.Orders.Any());

                Assert.AreEqual(@"
SELECT TOP (1) [t].[CustomerID] AS [CustomerID], [t].[Address] AS [Address], [t].[City] AS [City], [t].[CompanyName] AS [CompanyName], [t].[ContactName] AS [ContactName], [t].[ContactTitle] AS [ContactTitle], [t].[Country] AS [Country], [t].[Fax] AS [Fax], [t].[Phone] AS [Phone], [t].[PostalCode] AS [PostalCode], [t].[Region] AS [Region], (
    SELECT [t_0].[OrderID] AS [OrderID], [t_0].[CustomerID] AS [CustomerID], [t_0].[EmployeeID] AS [EmployeeID], [t_0].[Freight] AS [Freight], [t_0].[OrderDate] AS [OrderDate], [t_0].[RequiredDate] AS [RequiredDate], [t_0].[ShipAddress] AS [ShipAddress], [t_0].[ShipCity] AS [ShipCity], [t_0].[ShipCountry] AS [ShipCountry], [t_0].[ShipName] AS [ShipName], [t_0].[ShipPostalCode] AS [ShipPostalCode], [t_0].[ShipRegion] AS [ShipRegion], [t_0].[ShipVia] AS [ShipVia], [t_0].[ShippedDate] AS [ShippedDate], (
        SELECT [t_1].[OrderID] AS [OrderID], [t_1].[ProductID] AS [ProductID], [t_1].[Discount] AS [Discount], [t_1].[Quantity] AS [Quantity], [t_1].[UnitPrice] AS [UnitPrice]
        FROM [dbo].[Order Details] AS [t_1]
        WHERE ([t_1].[UnitPrice] >= 5.00) AND ([t_0].[OrderID] = [t_1].[OrderID])
        FOR JSON PATH, INCLUDE_NULL_VALUES
    ) AS [OrderDetails]
    FROM [dbo].[Orders] AS [t_0]
    WHERE [t].[CustomerID] = [t_0].[CustomerID]
    FOR JSON PATH, INCLUDE_NULL_VALUES
) AS [Orders]
FROM [dbo].[Customers] AS [t]
".Trim(), log.ToString().Trim());
            });
        }

        private static Customer ForceNonTranslatable(NorthwindDbContext context, Order order)
        {
            return context.Set<Customer>().Single(c => c.CustomerID == order.CustomerID);
        }

        [TestMethod]
        public void TestEfCore_DoubleQuery()
        {
            EfCoreTestCase((context, log) =>
            {
                var result = (from o in context.Set<Order>()
                              select new { o, Customer = ForceNonTranslatable(context, o) }).Take(5).ToArray();

                Assert.AreEqual(@"
SELECT TOP (5) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[Freight] AS [Freight], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipCountry] AS [ShipCountry], [o].[ShipName] AS [ShipName], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipRegion] AS [ShipRegion], [o].[ShipVia] AS [ShipVia], [o].[ShippedDate] AS [ShippedDate]
FROM [dbo].[Orders] AS [o]

SELECT TOP (2) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [dbo].[Customers] AS [c]
WHERE [c].[CustomerID] = @p0

SELECT TOP (2) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [dbo].[Customers] AS [c]
WHERE [c].[CustomerID] = @p0

SELECT TOP (2) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [dbo].[Customers] AS [c]
WHERE [c].[CustomerID] = @p0

SELECT TOP (2) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [dbo].[Customers] AS [c]
WHERE [c].[CustomerID] = @p0

SELECT TOP (2) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region]
FROM [dbo].[Customers] AS [c]
WHERE [c].[CustomerID] = @p0
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void QueryFilter_Via_TopLevel()
        {
            EfCoreTestCase((context, log) =>
            {
                var results = context.Set<OrderDetail>().Where(d => d.OrderID == 10252).ToList();

                Assert.IsFalse(results.Any(r => r.UnitPrice < 5.00m));
                Assert.AreEqual(2, results.Count);

                Assert.AreEqual(@"
SELECT [d].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[Discount] AS [Discount], [d].[Quantity] AS [Quantity], [d].[UnitPrice] AS [UnitPrice]
FROM [dbo].[Order Details] AS [d]
WHERE ([d].[UnitPrice] >= 5.00) AND ([d].[OrderID] = 10252)
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void QueryFilter_Via_Navigation()
        {
            EfCoreTestCase((context, log) =>
            {
                var results = (from o in context.Set<Order>()
                               where o.OrderID == 10252
                               from d in o.OrderDetails
                               select d).ToList();

                Assert.IsFalse(results.Any(r => r.UnitPrice < 5.00m));
                Assert.AreEqual(2, results.Count);

                Assert.AreEqual(@"
SELECT [d].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[Discount] AS [Discount], [d].[Quantity] AS [Quantity], [d].[UnitPrice] AS [UnitPrice]
FROM [dbo].[Orders] AS [o]
INNER JOIN (
    SELECT [t].[OrderID] AS [OrderID], [t].[ProductID] AS [ProductID], [t].[Discount] AS [Discount], [t].[Quantity] AS [Quantity], [t].[UnitPrice] AS [UnitPrice]
    FROM [dbo].[Order Details] AS [t]
    WHERE [t].[UnitPrice] >= 5.00
) AS [d] ON [o].[OrderID] = [d].[OrderID]
WHERE [o].[OrderID] = 10252
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void QueryFilter_Via_TopLevel_IgnoreQueryFilters()
        {
            EfCoreTestCase((context, log) =>
            {
                var results = context.Set<OrderDetail>().IgnoreQueryFilters().Where(d => d.OrderID == 10252).ToList();

                Assert.IsTrue(results.Any(r => r.UnitPrice < 5.00m));
                Assert.AreEqual(3, results.Count);

                Assert.AreEqual(@"
SELECT [d].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[Discount] AS [Discount], [d].[Quantity] AS [Quantity], [d].[UnitPrice] AS [UnitPrice]
FROM [dbo].[Order Details] AS [d]
WHERE [d].[OrderID] = 10252
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void QueryFilter_Via_Navigation_IgnoreQueryFilters()
        {
            EfCoreTestCase((context, log) =>
            {
                var results = (from o in context.Set<Order>()
                               where o.OrderID == 10252
                               from d in o.OrderDetails
                               select d).IgnoreQueryFilters().ToList();

                Assert.IsTrue(results.Any(r => r.UnitPrice < 5.00m));
                Assert.AreEqual(3, results.Count);

                Assert.AreEqual(@"
SELECT [t].[OrderID] AS [OrderID], [t].[ProductID] AS [ProductID], [t].[Discount] AS [Discount], [t].[Quantity] AS [Quantity], [t].[UnitPrice] AS [UnitPrice]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Order Details] AS [t] ON [o].[OrderID] = [t].[OrderID]
WHERE [o].[OrderID] = 10252
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void Inheritance_TPH_Simple()
        {
            EfCoreTestCase((context, log) =>
            {
                var results = context.Set<Product>().ToList();

                Assert.IsTrue(results.Where(r => !r.Discontinued).All(r => r is Product && !(r is DiscontinuedProduct)));
                Assert.IsTrue(results.Where(r => r.Discontinued).All(r => r is DiscontinuedProduct));

                Assert.AreEqual(@"
SELECT [t].[ProductID] AS [Item1], [t].[CategoryID] AS [Item2], [t].[Discontinued] AS [Item3], [t].[ProductName] AS [Item4], [t].[SupplierID] AS [Item5], [t].[QuantityPerUnit] AS [Item6], [t].[ReorderLevel] AS [Item7], [t].[UnitPrice] AS [Rest.Item1], [t].[UnitsInStock] AS [Rest.Item2], [t].[UnitsOnOrder] AS [Rest.Item3]
FROM [dbo].[Products] AS [t]
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void Inheritance_TPH_OfType()
        {
            EfCoreTestCase((context, log) =>
            {
                var results = context.Set<Product>().OfType<DiscontinuedProduct>().ToList();

                Assert.IsTrue(results.All(r => r is DiscontinuedProduct && r.Discontinued));

                Assert.AreEqual(@"
SELECT [t].[ProductID] AS [Item1], [t].[CategoryID] AS [Item2], [t].[Discontinued] AS [Item3], [t].[ProductName] AS [Item4], [t].[SupplierID] AS [Item5], [t].[QuantityPerUnit] AS [Item6], [t].[ReorderLevel] AS [Item7], [t].[UnitPrice] AS [Rest.Item1], [t].[UnitsInStock] AS [Rest.Item2], [t].[UnitsOnOrder] AS [Rest.Item3]
FROM [dbo].[Products] AS [t]
WHERE [t].[Discontinued] = 1
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void OwnedEntity_OneLevelDeep()
        {
            EfCoreTestCase((context, log) =>
            {
                var product = context.Set<Product>().First();

                Assert.IsNotNull(product.ProductStats);

                Assert.AreEqual(@"
SELECT TOP (1) [t].[ProductID] AS [Item1], [t].[CategoryID] AS [Item2], [t].[Discontinued] AS [Item3], [t].[ProductName] AS [Item4], [t].[SupplierID] AS [Item5], [t].[QuantityPerUnit] AS [Item6], [t].[ReorderLevel] AS [Item7], [t].[UnitPrice] AS [Rest.Item1], [t].[UnitsInStock] AS [Rest.Item2], [t].[UnitsOnOrder] AS [Rest.Item3]
FROM [dbo].[Products] AS [t]
".Trim(), log.ToString().Trim());
            });
        }

        private void EfCoreTestCase(Action<NorthwindDbContext, StringBuilder> action)
        {
            var services = new ServiceCollection();
            var loggerProvider = new TestLoggerProvider();

            services.AddLogging(log =>
            {
                log.AddProvider(loggerProvider);
            });

            services.AddDbContext<NorthwindDbContext>(options =>
            {
                options
                    .UseSqlServer(@"Server=.\sqlexpress; Database=NORTHWND; Trusted_Connection=true; MultipleActiveResultSets=True")
                    .UseImpatientQueryCompiler();
            });

            var provider = services.BuildServiceProvider();

            using (var scope = provider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<NorthwindDbContext>())
            {
                action(context, loggerProvider.LoggerInstance.StringBuilder);
            }
        }

        private async Task EfCoreTestCaseAsync(Func<NorthwindDbContext, StringBuilder, Task> action)
        {
            var services = new ServiceCollection();
            var loggerProvider = new TestLoggerProvider();

            services.AddLogging(log =>
            {
                log.AddProvider(loggerProvider);
            });

            services.AddDbContext<NorthwindDbContext>(options =>
            {
                options
                    .UseSqlServer(@"Server=.\sqlexpress; Database=NORTHWND; Trusted_Connection=true; MultipleActiveResultSets=True")
                    .UseImpatientQueryCompiler();
            });

            var provider = services.BuildServiceProvider();

            using (var scope = provider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<NorthwindDbContext>())
            {
                await action(context, loggerProvider.LoggerInstance.StringBuilder);
            }
        }

        public class TestLoggerProvider : ILoggerProvider
        {
            public TestLogger LoggerInstance = new TestLogger();

            public ILogger CreateLogger(string categoryName)
            {
                return LoggerInstance;
            }

            public void Dispose()
            {
            }
        }

        public class TestLogger : ILogger
        {
            public StringBuilder StringBuilder { get; } = new StringBuilder();

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (state is IReadOnlyList<KeyValuePair<string, object>> dict)
                {
                    var commandText = dict.FirstOrDefault(i => i.Key == "commandText").Value;

                    if (commandText != null)
                    {
                        if (StringBuilder.Length > 0)
                        {
                            StringBuilder.AppendLine().AppendLine();
                        }

                        StringBuilder.Append(commandText);
                    }
                }
            }
        }
    }

    internal class NorthwindDbContext : DbContext
    {
        public NorthwindDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // TODO: Document that a default schema is required.
            modelBuilder.HasDefaultSchema("dbo");

            modelBuilder.Entity<Customer>(e =>
            {
                e.HasKey(d => d.CustomerID);
            });

            modelBuilder.Entity<Order>(e =>
            {
                e.HasKey(d => d.OrderID);
            });

            modelBuilder.Entity<OrderDetail>(e =>
            {
                e.HasKey(d => new { d.OrderID, d.ProductID });

                e.HasOne(d => d.Product).WithMany().HasForeignKey(d => d.ProductID);

                e.HasQueryFilter(d => d.UnitPrice >= 5.00m);
            });

            modelBuilder.Entity<Product>(e =>
            {
                e.HasKey(d => d.ProductID);

                e.OwnsOne(d => d.ProductStats, d =>
                {
                    d.Property(f => f.QuantityPerUnit).HasColumnName(nameof(ProductStats.QuantityPerUnit));
                    d.Property(f => f.ReorderLevel).HasColumnName(nameof(ProductStats.ReorderLevel));
                    d.Property(f => f.UnitPrice).HasColumnName(nameof(ProductStats.UnitPrice));
                    d.Property(f => f.UnitsInStock).HasColumnName(nameof(ProductStats.UnitsInStock));
                    d.Property(f => f.UnitsOnOrder).HasColumnName(nameof(ProductStats.UnitsOnOrder));
                });

                e.HasDiscriminator(p => p.Discontinued).HasValue(false);
            });

            modelBuilder.Entity<DiscontinuedProduct>(e =>
            {
                e.HasDiscriminator(p => p.Discontinued).HasValue(true);
            });
        }
    }
}
