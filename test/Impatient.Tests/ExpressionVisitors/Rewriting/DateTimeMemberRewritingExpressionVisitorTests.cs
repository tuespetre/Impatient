using Impatient.Query;
using Impatient.Tests.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Impatient.Tests.ExpressionVisitors.Rewriting
{
    [TestClass]
    public class DateTimeMemberRewritingExpressionVisitorTests
    {
        private static NorthwindQueryContext context;

        static DateTimeMemberRewritingExpressionVisitorTests()
        {
            var impatient
                = new ImpatientQueryProvider(
                    new TestImpatientConnectionFactory(
                        @"Server=.\sqlexpress; Database=NORTHWND; Trusted_Connection=True"),
                    new DefaultImpatientQueryCache(),
                    new DefaultImpatientExpressionVisitorProvider());

            context = new NorthwindQueryContext(impatient);
        }

        [TestCleanup]
        public void Cleanup()
        {
            context.ClearLog();
        }

        [TestMethod]
        public void DateTime_Now()
        {
            var query = (from o in context.Orders
                         select DateTime.Now).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) GETDATE()
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_UtcNow()
        {
            var query = (from o in context.Orders
                         select DateTime.UtcNow).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) GETUTCDATE()
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_Date()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value.Date).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) CAST([o].[OrderDate] AS date)
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_Day()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value.Day).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) DATEPART(day, [o].[OrderDate])
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_DayOfYear()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value.DayOfYear).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) DATEPART(dayofyear, [o].[OrderDate])
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_Hour()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value.Hour).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) DATEPART(hour, [o].[OrderDate])
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_Millisecond()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value.Millisecond).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) DATEPART(millisecond, [o].[OrderDate])
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_Minute()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value.Minute).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) DATEPART(minute, [o].[OrderDate])
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_Month()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value.Month).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) DATEPART(month, [o].[OrderDate])
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_Second()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value.Second).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) DATEPART(second, [o].[OrderDate])
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_Year()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value.Year).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) DATEPART(year, [o].[OrderDate])
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_AddDays()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value.AddDays(1)).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) DATEADD(day, 1, [o].[OrderDate])
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_AddHours()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value.AddHours(1)).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) DATEADD(hour, 1, [o].[OrderDate])
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_AddMilliseconds()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value.AddMilliseconds(1)).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) DATEADD(millisecond, 1, [o].[OrderDate])
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_AddMinutes()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value.AddMinutes(1)).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) DATEADD(minute, 1, [o].[OrderDate])
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_AddMonths()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value.AddMonths(1)).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) DATEADD(month, 1, [o].[OrderDate])
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_AddSeconds()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value.AddSeconds(1)).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) DATEADD(second, 1, [o].[OrderDate])
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }

        [TestMethod]
        public void DateTime_AddYears()
        {
            var query = (from o in context.Orders
                         select o.OrderDate.Value.AddYears(1)).Take(10);

            query.ToList();

            Assert.AreEqual(
                @"SELECT TOP (10) DATEADD(year, 1, [o].[OrderDate])
FROM [dbo].[Orders] AS [o]",
                context.SqlLog);
        }
    }
}
