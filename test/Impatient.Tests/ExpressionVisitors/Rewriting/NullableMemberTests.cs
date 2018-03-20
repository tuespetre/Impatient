using Impatient.Tests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Impatient.Tests.ExpressionVisitors.Rewriting
{
    [TestClass]
    public class NullableMemberTests
    {
        private static NorthwindQueryContext context;

        static NullableMemberTests()
        {
            context
                = ExtensionMethods
                    .CreateServiceProvider(
                        connectionString: @"Server=.\sqlexpress; Database=NORTHWND; Trusted_Connection=True")
                    .GetService<NorthwindQueryContext>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            context.ClearLog();
        }

        [TestMethod]
        public void Nullable_Value()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) [o].[OrderDate]
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void Nullable_HasValue()
        {
            var query = from o in context.Orders
                        where !o.OrderDate.HasValue
                        select o.OrderID;

            query.ToList();

            Assert.AreEqual(
                @"SELECT [o].[OrderID]
FROM [dbo].[Orders] AS [o]
WHERE [o].[OrderDate] IS NULL",
                context.SqlLog);
        }

        [TestMethod]
        public void Nullable_GetValueOrDefault1()
        {
            var query = from o in context.Orders
                        select o.Freight.GetValueOrDefault();

            query.ToList();

            Assert.AreEqual(
                @"SELECT COALESCE([o].[Freight], 0)
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void Nullable_GetValueOrDefault2()
        {
            var query = from o in context.Orders
                        select o.Freight.GetValueOrDefault(6.97m);

            query.ToList();

            Assert.AreEqual(
                @"SELECT COALESCE([o].[Freight], 6.97)
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }
    }
}
