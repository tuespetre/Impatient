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
SELECT TOP (1) [o].[OrderID] AS [o.OrderID], [o].[CustomerID] AS [o.CustomerID], [o].[EmployeeID] AS [o.EmployeeID], [o].[Freight] AS [o.Freight], [o].[OrderDate] AS [o.OrderDate], [o].[RequiredDate] AS [o.RequiredDate], [o].[ShipAddress] AS [o.ShipAddress], [o].[ShipCity] AS [o.ShipCity], [o].[ShipCountry] AS [o.ShipCountry], [o].[ShipName] AS [o.ShipName], [o].[ShipPostalCode] AS [o.ShipPostalCode], [o].[ShipRegion] AS [o.ShipRegion], [o].[ShipVia] AS [o.ShipVia], [o].[ShippedDate] AS [o.ShippedDate], [c].[CustomerID] AS [Customer.CustomerID], [c].[Address] AS [Customer.Address], [c].[City] AS [Customer.City], [c].[CompanyName] AS [Customer.CompanyName], [c].[ContactName] AS [Customer.ContactName], [c].[ContactTitle] AS [Customer.ContactTitle], [c].[Country] AS [Customer.Country], [c].[Fax] AS [Customer.Fax], [c].[Phone] AS [Customer.Phone], [c].[PostalCode] AS [Customer.PostalCode], [c].[Region] AS [Customer.Region]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
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

                var entries = context.ChangeTracker.Entries();

                Assert.AreEqual(1, entries.Count());
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
SELECT TOP (1) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[Freight] AS [Freight], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipCountry] AS [ShipCountry], [o].[ShipName] AS [ShipName], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipRegion] AS [ShipRegion], [o].[ShipVia] AS [ShipVia], [o].[ShippedDate] AS [ShippedDate]
FROM [dbo].[Orders] AS [o]
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
                var details
                    = (from d in context.Set<OrderDetail>().AsNoTracking()
                       where d.OrderID == 10252
                       from d2 in d.Order.OrderDetails
                       select d2).ToList();

                Assert.AreEqual(0, context.ChangeTracker.Entries().Count());
                Assert.AreNotEqual(details.Count, details.Distinct().Count());

                Assert.AreEqual(@"
SELECT [d2].[OrderID] AS [OrderID], [d2].[ProductID] AS [ProductID], [d2].[Discount] AS [Discount], [d2].[Quantity] AS [Quantity], [d2].[UnitPrice] AS [UnitPrice]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
INNER JOIN (
    SELECT [d_0].[OrderID] AS [OrderID], [d_0].[ProductID] AS [ProductID], [d_0].[Discount] AS [Discount], [d_0].[Quantity] AS [Quantity], [d_0].[UnitPrice] AS [UnitPrice]
    FROM [dbo].[Order Details] AS [d_0]
    WHERE [d_0].[UnitPrice] >= 5.0
) AS [d2] ON [o].[OrderID] = [d2].[OrderID]
WHERE ([d].[UnitPrice] >= 5.0) AND ([d].[OrderID] = 10252)
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
SELECT TOP (1) [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[Freight] AS [Freight], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipCountry] AS [ShipCountry], [o].[ShipName] AS [ShipName], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipRegion] AS [ShipRegion], [o].[ShipVia] AS [ShipVia], [o].[ShippedDate] AS [ShippedDate], [c].[CustomerID] AS [Customer.CustomerID], [c].[Address] AS [Customer.Address], [c].[City] AS [Customer.City], [c].[CompanyName] AS [Customer.CompanyName], [c].[ContactName] AS [Customer.ContactName], [c].[ContactTitle] AS [Customer.ContactTitle], [c].[Country] AS [Customer.Country], [c].[Fax] AS [Customer.Fax], [c].[Phone] AS [Customer.Phone], [c].[PostalCode] AS [Customer.PostalCode], [c].[Region] AS [Customer.Region]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
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
SELECT TOP (1) [d].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[Discount] AS [Discount], [d].[Quantity] AS [Quantity], [d].[UnitPrice] AS [UnitPrice], [o].[OrderID] AS [Order.OrderID], [o].[CustomerID] AS [Order.CustomerID], [o].[EmployeeID] AS [Order.EmployeeID], [o].[Freight] AS [Order.Freight], [o].[OrderDate] AS [Order.OrderDate], [o].[RequiredDate] AS [Order.RequiredDate], [o].[ShipAddress] AS [Order.ShipAddress], [o].[ShipCity] AS [Order.ShipCity], [o].[ShipCountry] AS [Order.ShipCountry], [o].[ShipName] AS [Order.ShipName], [o].[ShipPostalCode] AS [Order.ShipPostalCode], [o].[ShipRegion] AS [Order.ShipRegion], [o].[ShipVia] AS [Order.ShipVia], [o].[ShippedDate] AS [Order.ShippedDate], [c].[CustomerID] AS [Order.Customer.CustomerID], [c].[Address] AS [Order.Customer.Address], [c].[City] AS [Order.Customer.City], [c].[CompanyName] AS [Order.Customer.CompanyName], [c].[ContactName] AS [Order.Customer.ContactName], [c].[ContactTitle] AS [Order.Customer.ContactTitle], [c].[Country] AS [Order.Customer.Country], [c].[Fax] AS [Order.Customer.Fax], [c].[Phone] AS [Order.Customer.Phone], [c].[PostalCode] AS [Order.Customer.PostalCode], [c].[Region] AS [Order.Customer.Region]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
INNER JOIN [dbo].[Customers] AS [c] ON [o].[CustomerID] = [c].[CustomerID]
WHERE [d].[UnitPrice] >= 5.0
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
SELECT TOP (1) [d].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[Discount] AS [Discount], [d].[Quantity] AS [Quantity], [d].[UnitPrice] AS [UnitPrice], [o].[OrderID] AS [Order.OrderID], [o].[CustomerID] AS [Order.CustomerID], [o].[EmployeeID] AS [Order.EmployeeID], [o].[Freight] AS [Order.Freight], [o].[OrderDate] AS [Order.OrderDate], [o].[RequiredDate] AS [Order.RequiredDate], [o].[ShipAddress] AS [Order.ShipAddress], [o].[ShipCity] AS [Order.ShipCity], [o].[ShipCountry] AS [Order.ShipCountry], [o].[ShipName] AS [Order.ShipName], [o].[ShipPostalCode] AS [Order.ShipPostalCode], [o].[ShipRegion] AS [Order.ShipRegion], [o].[ShipVia] AS [Order.ShipVia], [o].[ShippedDate] AS [Order.ShippedDate], [p].[Item1] AS [Product.Item1], [p].[Item2] AS [Product.Item2], [p].[Item3] AS [Product.Item3], [p].[Item4] AS [Product.Item4], [p].[Item5] AS [Product.Item5], [p].[Item6] AS [Product.Item6], [p].[Item7] AS [Product.Item7], [p].[Rest.Item1] AS [Product.Rest.Item1], [p].[Rest.Item2] AS [Product.Rest.Item2], [p].[Rest.Item3] AS [Product.Rest.Item3]
FROM [dbo].[Order Details] AS [d]
INNER JOIN [dbo].[Orders] AS [o] ON [d].[OrderID] = [o].[OrderID]
INNER JOIN (
    SELECT [p_0].[ProductID] AS [Item1], [p_0].[CategoryID] AS [Item2], [p_0].[Discontinued] AS [Item3], [p_0].[ProductName] AS [Item4], [p_0].[SupplierID] AS [Item5], [p_0].[QuantityPerUnit] AS [Item6], [p_0].[ReorderLevel] AS [Item7], [p_0].[UnitPrice] AS [Rest.Item1], [p_0].[UnitsInStock] AS [Rest.Item2], [p_0].[UnitsOnOrder] AS [Rest.Item3]
    FROM [dbo].[Products] AS [p_0]
    WHERE [p_0].[Discontinued] IN (0, 1)
) AS [p] ON [d].[ProductID] = [p].[Item1]
WHERE [d].[UnitPrice] >= 5.0
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
SELECT TOP (1) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region], (
    SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[Freight] AS [Freight], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipCountry] AS [ShipCountry], [o].[ShipName] AS [ShipName], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipRegion] AS [ShipRegion], [o].[ShipVia] AS [ShipVia], [o].[ShippedDate] AS [ShippedDate]
    FROM [dbo].[Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    FOR JSON PATH
) AS [Orders]
FROM [dbo].[Customers] AS [c]
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
SELECT TOP (1) [c].[CustomerID] AS [CustomerID], [c].[Address] AS [Address], [c].[City] AS [City], [c].[CompanyName] AS [CompanyName], [c].[ContactName] AS [ContactName], [c].[ContactTitle] AS [ContactTitle], [c].[Country] AS [Country], [c].[Fax] AS [Fax], [c].[Phone] AS [Phone], [c].[PostalCode] AS [PostalCode], [c].[Region] AS [Region], (
    SELECT [o].[OrderID] AS [OrderID], [o].[CustomerID] AS [CustomerID], [o].[EmployeeID] AS [EmployeeID], [o].[Freight] AS [Freight], [o].[OrderDate] AS [OrderDate], [o].[RequiredDate] AS [RequiredDate], [o].[ShipAddress] AS [ShipAddress], [o].[ShipCity] AS [ShipCity], [o].[ShipCountry] AS [ShipCountry], [o].[ShipName] AS [ShipName], [o].[ShipPostalCode] AS [ShipPostalCode], [o].[ShipRegion] AS [ShipRegion], [o].[ShipVia] AS [ShipVia], [o].[ShippedDate] AS [ShippedDate], (
        SELECT [d].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[Discount] AS [Discount], [d].[Quantity] AS [Quantity], [d].[UnitPrice] AS [UnitPrice]
        FROM [dbo].[Order Details] AS [d]
        WHERE ([d].[UnitPrice] >= 5.0) AND ([o].[OrderID] = [d].[OrderID])
        FOR JSON PATH
    ) AS [OrderDetails]
    FROM [dbo].[Orders] AS [o]
    WHERE [c].[CustomerID] = [o].[CustomerID]
    FOR JSON PATH
) AS [Orders]
FROM [dbo].[Customers] AS [c]
".Trim(), log.ToString().Trim());
            });
        }

        private static Customer ForceNonTranslatable(NorthwindDbContext context, Order order)
        {
            return context.Set<Customer>().Single(c => c.CustomerID == order.CustomerID);
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
WHERE ([d].[UnitPrice] >= 5.0) AND ([d].[OrderID] = 10252)
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

                Assert.AreEqual(@"
SELECT [d].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[Discount] AS [Discount], [d].[Quantity] AS [Quantity], [d].[UnitPrice] AS [UnitPrice]
FROM [dbo].[Orders] AS [o]
INNER JOIN (
    SELECT [d_0].[OrderID] AS [OrderID], [d_0].[ProductID] AS [ProductID], [d_0].[Discount] AS [Discount], [d_0].[Quantity] AS [Quantity], [d_0].[UnitPrice] AS [UnitPrice]
    FROM [dbo].[Order Details] AS [d_0]
    WHERE [d_0].[UnitPrice] >= 5.0
) AS [d] ON [o].[OrderID] = [d].[OrderID]
WHERE [o].[OrderID] = 10252
".Trim(), log.ToString().Trim());

                Assert.IsFalse(results.Any(r => r.UnitPrice < 5.00m));
                Assert.AreEqual(2, results.Count);
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
SELECT [d].[OrderID] AS [OrderID], [d].[ProductID] AS [ProductID], [d].[Discount] AS [Discount], [d].[Quantity] AS [Quantity], [d].[UnitPrice] AS [UnitPrice]
FROM [dbo].[Orders] AS [o]
INNER JOIN [dbo].[Order Details] AS [d] ON [o].[OrderID] = [d].[OrderID]
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
SELECT [p].[ProductID] AS [Item1], [p].[CategoryID] AS [Item2], [p].[Discontinued] AS [Item3], [p].[ProductName] AS [Item4], [p].[SupplierID] AS [Item5], [p].[QuantityPerUnit] AS [Item6], [p].[ReorderLevel] AS [Item7], [p].[UnitPrice] AS [Rest.Item1], [p].[UnitsInStock] AS [Rest.Item2], [p].[UnitsOnOrder] AS [Rest.Item3]
FROM [dbo].[Products] AS [p]
WHERE [p].[Discontinued] IN (0, 1)
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
SELECT [p].[ProductID] AS [Item1], [p].[CategoryID] AS [Item2], [p].[Discontinued] AS [Item3], [p].[ProductName] AS [Item4], [p].[SupplierID] AS [Item5], [p].[QuantityPerUnit] AS [Item6], [p].[ReorderLevel] AS [Item7], [p].[UnitPrice] AS [Rest.Item1], [p].[UnitsInStock] AS [Rest.Item2], [p].[UnitsOnOrder] AS [Rest.Item3]
FROM [dbo].[Products] AS [p]
WHERE [p].[Discontinued] IN (0, 1) AND ([p].[Discontinued] = 1)
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
SELECT TOP (1) [p].[ProductID] AS [Item1], [p].[CategoryID] AS [Item2], [p].[Discontinued] AS [Item3], [p].[ProductName] AS [Item4], [p].[SupplierID] AS [Item5], [p].[QuantityPerUnit] AS [Item6], [p].[ReorderLevel] AS [Item7], [p].[UnitPrice] AS [Rest.Item1], [p].[UnitsInStock] AS [Rest.Item2], [p].[UnitsOnOrder] AS [Rest.Item3]
FROM [dbo].[Products] AS [p]
WHERE [p].[Discontinued] IN (0, 1)
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void EFProperty_translated()
        {
            EfCoreTestCase((context, log) =>
            {
                var result = (from p in context.Set<Product>()
                              select EF.Property<short?>(p.ProductStats, "ReorderLevel")).FirstOrDefault();

                Assert.AreEqual(@"
SELECT TOP (1) [p].[ReorderLevel]
FROM [dbo].[Products] AS [p]
WHERE [p].[Discontinued] IN (0, 1)
".Trim(), log.ToString().Trim());
            });
        }

        private static bool ClientPredicate<TArg>(TArg arg)
        {
            return true;
        }

        [TestMethod]
        public void EFProperty_at_client()
        {
            EfCoreTestCase((context, log) =>
            {
                var result = (from p in context.Set<Product>()
                              where ClientPredicate(p)
                              select EF.Property<short?>(p.ProductStats, "ReorderLevel")).FirstOrDefault();

                Assert.AreEqual(@"
SELECT [p].[ProductID] AS [Item1], [p].[CategoryID] AS [Item2], [p].[Discontinued] AS [Item3], [p].[ProductName] AS [Item4], [p].[SupplierID] AS [Item5], [p].[QuantityPerUnit] AS [Item6], [p].[ReorderLevel] AS [Item7], [p].[UnitPrice] AS [Rest.Item1], [p].[UnitsInStock] AS [Rest.Item2], [p].[UnitsOnOrder] AS [Rest.Item3]
FROM [dbo].[Products] AS [p]
WHERE [p].[Discontinued] IN (0, 1)
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void NestedReaders()
        {
            var services = new ServiceCollection();
            var loggerProvider = new TestConnectionLoggerProvider();

            services.AddLogging(log =>
            {
                log
                    .AddProvider(loggerProvider)
                    .SetMinimumLevel(LogLevel.Debug)
                    .AddFilter((category, level) => category.StartsWith(DbLoggerCategory.Database.Name));
            });

            services.AddDbContext<NorthwindDbContext>(options =>
            {
                options
                    .UseSqlServer(@"Server=.\sqlexpress; Database=NORTHWND; Trusted_Connection=true")
                    .UseImpatient();
            });

            var provider = services.BuildServiceProvider();

            using (var scope = provider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<NorthwindDbContext>())
            {
                var result = (from o in context.Set<Order>()
                              select new
                              {
                                  o,
                                  Customer = context.Set<Customer>().Where(ClientPredicate).FirstOrDefault()
                              }).Take(5).ToArray();

                Assert.AreEqual(@"Opening connection t
Opened connection to
Executing DbCommand 
Executed DbCommand (
Executing DbCommand 
Executed DbCommand (
Executing DbCommand 
Executed DbCommand (
Executing DbCommand 
Executed DbCommand (
Executing DbCommand 
Executed DbCommand (
Executing DbCommand 
Executed DbCommand (
Closing connection t
Closed connection to
", loggerProvider.LoggerInstance.StringBuilder.ToString());
            }
        }

        [TestMethod]
        public void TrickyJson_ProperChangeTracking()
        {
            EfCoreTestCase((context, log) =>
            {
                var customers = context.Set<Customer>();
                var orders = context.Set<Order>();
                var details = context.Set<OrderDetail>();

                var query = from t in (from c in customers
                                       from t in (from o in orders
                                                  where o.CustomerID == c.CustomerID
                                                  let d = from d in details
                                                          where d.OrderID == o.OrderID
                                                          select d
                                                  select new { o, d }).Where(x => x.o.Freight == null)
                                       select new { c, t.o, t.d }).Take(10)
                            from d in t.d
                            select new { t.c, t.o, d };

                var results = query.ToList();

                Assert.IsTrue(results.All(r => r.c == r.o.Customer));
                Assert.IsTrue(results.All(r => r.o == r.d.Order));

                Assert.AreEqual(@"
SELECT [t].[c.CustomerID] AS [c.CustomerID], [t].[c.Address] AS [c.Address], [t].[c.City] AS [c.City], [t].[c.CompanyName] AS [c.CompanyName], [t].[c.ContactName] AS [c.ContactName], [t].[c.ContactTitle] AS [c.ContactTitle], [t].[c.Country] AS [c.Country], [t].[c.Fax] AS [c.Fax], [t].[c.Phone] AS [c.Phone], [t].[c.PostalCode] AS [c.PostalCode], [t].[c.Region] AS [c.Region], [t].[o.OrderID] AS [o.OrderID], [t].[o.CustomerID] AS [o.CustomerID], [t].[o.EmployeeID] AS [o.EmployeeID], [t].[o.Freight] AS [o.Freight], [t].[o.OrderDate] AS [o.OrderDate], [t].[o.RequiredDate] AS [o.RequiredDate], [t].[o.ShipAddress] AS [o.ShipAddress], [t].[o.ShipCity] AS [o.ShipCity], [t].[o.ShipCountry] AS [o.ShipCountry], [t].[o.ShipName] AS [o.ShipName], [t].[o.ShipPostalCode] AS [o.ShipPostalCode], [t].[o.ShipRegion] AS [o.ShipRegion], [t].[o.ShipVia] AS [o.ShipVia], [t].[o.ShippedDate] AS [o.ShippedDate], [d].[value] AS [d]
FROM (
    SELECT TOP (10) [c].[CustomerID] AS [c.CustomerID], [c].[Address] AS [c.Address], [c].[City] AS [c.City], [c].[CompanyName] AS [c.CompanyName], [c].[ContactName] AS [c.ContactName], [c].[ContactTitle] AS [c.ContactTitle], [c].[Country] AS [c.Country], [c].[Fax] AS [c.Fax], [c].[Phone] AS [c.Phone], [c].[PostalCode] AS [c.PostalCode], [c].[Region] AS [c.Region], [t_0].[o.OrderID] AS [o.OrderID], [t_0].[o.CustomerID] AS [o.CustomerID], [t_0].[o.EmployeeID] AS [o.EmployeeID], [t_0].[o.Freight] AS [o.Freight], [t_0].[o.OrderDate] AS [o.OrderDate], [t_0].[o.RequiredDate] AS [o.RequiredDate], [t_0].[o.ShipAddress] AS [o.ShipAddress], [t_0].[o.ShipCity] AS [o.ShipCity], [t_0].[o.ShipCountry] AS [o.ShipCountry], [t_0].[o.ShipName] AS [o.ShipName], [t_0].[o.ShipPostalCode] AS [o.ShipPostalCode], [t_0].[o.ShipRegion] AS [o.ShipRegion], [t_0].[o.ShipVia] AS [o.ShipVia], [t_0].[o.ShippedDate] AS [o.ShippedDate], [t_0].[d] AS [d]
    FROM [dbo].[Customers] AS [c]
    CROSS APPLY (
        SELECT [o].[OrderID] AS [o.OrderID], [o].[CustomerID] AS [o.CustomerID], [o].[EmployeeID] AS [o.EmployeeID], [o].[Freight] AS [o.Freight], [o].[OrderDate] AS [o.OrderDate], [o].[RequiredDate] AS [o.RequiredDate], [o].[ShipAddress] AS [o.ShipAddress], [o].[ShipCity] AS [o.ShipCity], [o].[ShipCountry] AS [o.ShipCountry], [o].[ShipName] AS [o.ShipName], [o].[ShipPostalCode] AS [o.ShipPostalCode], [o].[ShipRegion] AS [o.ShipRegion], [o].[ShipVia] AS [o.ShipVia], [o].[ShippedDate] AS [o.ShippedDate], (
            SELECT [d_0].[OrderID] AS [OrderID], [d_0].[ProductID] AS [ProductID], [d_0].[Discount] AS [Discount], [d_0].[Quantity] AS [Quantity], [d_0].[UnitPrice] AS [UnitPrice]
            FROM [dbo].[Order Details] AS [d_0]
            WHERE ([d_0].[UnitPrice] >= 5.0) AND ([d_0].[OrderID] = [o].[OrderID])
            FOR JSON PATH
        ) AS [d]
        FROM [dbo].[Orders] AS [o]
        WHERE ([o].[CustomerID] = [c].[CustomerID]) AND ([o].[Freight] IS NULL)
    ) AS [t_0]
) AS [t]
CROSS APPLY (
    SELECT [j].[value]
    FROM OPENJSON([t].[d]) AS [j]
) AS [d]
".Trim(), log.ToString().Trim());
            });
        }

        [TestMethod]
        public void QueryType_AsTracking()
        {
            EfCoreTestCase((context, log) =>
            {
                // Load the customers first
                context.Set<Customer>().AsTracking().ToList();

                // Load the query type with the navigation to customers
                var qos 
                    = context
                        .Query<QuarterlyOrders>()
                        .AsTracking()
                        .ToList();
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
                    .UseImpatient();
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
                    .UseImpatient();
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
            public TestSqlLogger LoggerInstance { get; } = new TestSqlLogger();

            public ILogger CreateLogger(string categoryName)
            {
                return LoggerInstance;
            }

            public void Dispose()
            {
            }
        }

        public class TestConnectionLoggerProvider : ILoggerProvider
        {
            public TestConnectionLogger LoggerInstance { get; } = new TestConnectionLogger();

            public ILogger CreateLogger(string categoryName)
            {
                return LoggerInstance;
            }

            public void Dispose()
            {
            }
        }

        public class TestSqlLogger : ILogger
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

        public class TestConnectionLogger : ILogger
        {
            public StringBuilder StringBuilder { get; } = new StringBuilder();

            public IDisposable BeginScope<TState>(TState state) => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter(state, exception);

                StringBuilder.AppendLine(message.Substring(0, 20));
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
            modelBuilder.Entity<Customer>(e =>
            {
                e.HasKey(d => d.CustomerID);
            });

            modelBuilder.Entity<Order>(e =>
            {
                e.HasKey(d => d.OrderID);

                e.HasOne(o => o.Customer).WithMany(c => c.Orders).IsRequired();
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
                    d.Property(f => f.UnitPrice).HasColumnName(nameof(ProductStats.UnitPrice));
                    d.Property(f => f.UnitsInStock).HasColumnName(nameof(ProductStats.UnitsInStock));
                    d.Property(f => f.UnitsOnOrder).HasColumnName(nameof(ProductStats.UnitsOnOrder));

                    d.Property<short?>("ReorderLevel").HasColumnName("ReorderLevel");
                });

                e.HasDiscriminator(p => p.Discontinued).HasValue(false);
            });

            modelBuilder.Entity<DiscontinuedProduct>(e =>
            {
                e.HasDiscriminator(p => p.Discontinued).HasValue(true);
            });

            modelBuilder.Query<QuarterlyOrders>(q =>
            {
                q.ToView("Quarterly Orders", "dbo");

                q.Property(o => o.Id).HasColumnName("CustomerID");

                q.HasOne(o => o.Customer).WithMany().HasForeignKey(o => o.Id);
            });
        }
    }
}
