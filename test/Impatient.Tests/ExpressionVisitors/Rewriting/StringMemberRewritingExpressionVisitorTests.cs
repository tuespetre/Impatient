using Impatient.Tests.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Impatient.Tests.ExpressionVisitors.Rewriting
{
    [TestClass]
    public class StringMemberRewritingExpressionVisitorTests
    {
        private static NorthwindQueryContext context;

        static StringMemberRewritingExpressionVisitorTests()
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
        public void String_Length()
        {
            var query = from c in context.Customers
                        select c.ContactName.Length;

            query.ToList();

            Assert.AreEqual(
                @"SELECT LEN([c].[ContactName])
FROM [dbo].[Customers] AS [c]",
                context.SqlLog);
        }

        [TestMethod]
        public void String_Concat_string_x2()
        {
            var query = from c in context.Customers
                        select string.Concat(c.City, c.Address);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[City] + [c].[Address]
FROM [dbo].[Customers] AS [c]",
                context.SqlLog);
        }

        [TestMethod]
        public void String_Concat_string_x3()
        {
            var query = from c in context.Customers
                        select string.Concat(c.City, " ", c.Address);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[City] + N' ' + [c].[Address]
FROM [dbo].[Customers] AS [c]",
                context.SqlLog);
        }

        [TestMethod]
        public void String_Concat_string_x4()
        {
            var query = from c in context.Customers
                        select string.Concat(c.City, " ", c.Address, " ");

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[City] + N' ' + [c].[Address] + N' '
FROM [dbo].[Customers] AS [c]",
                context.SqlLog);
        }

        [TestMethod]
        public void String_Concat_string_params()
        {
            var query = from c in context.Customers
                        select string.Concat(c.City, " ", c.Address, " ", c.PostalCode);

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[City] + N' ' + [c].[Address] + N' ' + [c].[PostalCode]
FROM [dbo].[Customers] AS [c]",
                context.SqlLog);
        }

        [TestMethod]
        public void String_Trim()
        {
            var query = from c in context.Customers
                        select c.City.Trim();

            query.ToList();

            Assert.AreEqual(
                @"SELECT LTRIM(RTRIM([c].[City]))
FROM [dbo].[Customers] AS [c]",
                context.SqlLog);
        }

        [TestMethod]
        public void String_TrimStart()
        {
            var query = from c in context.Customers
                        select c.City.TrimStart();

            query.ToList();

            Assert.AreEqual(
                @"SELECT LTRIM([c].[City])
FROM [dbo].[Customers] AS [c]",
                context.SqlLog);
        }

        [TestMethod]
        public void String_TrimEnd()
        {
            var query = from c in context.Customers
                        select c.City.TrimEnd();

            query.ToList();

            Assert.AreEqual(
                @"SELECT RTRIM([c].[City])
FROM [dbo].[Customers] AS [c]",
                context.SqlLog);
        }

        [TestMethod]
        public void String_ToUpper()
        {
            var query = from c in context.Customers
                        select c.City.ToUpper();

            query.ToList();

            Assert.AreEqual(
                @"SELECT UPPER([c].[City])
FROM [dbo].[Customers] AS [c]",
                context.SqlLog);
        }

        [TestMethod]
        public void String_ToLower()
        {
            var query = from c in context.Customers
                        select c.City.ToLower();

            query.ToList();

            Assert.AreEqual(
                @"SELECT LOWER([c].[City])
FROM [dbo].[Customers] AS [c]",
                context.SqlLog);
        }

        [TestMethod]
        public void String_Substring1()
        {
            var query = from c in context.Customers
                        select c.City.Substring(1);

            query.ToList();

            Assert.AreEqual(
                @"SELECT SUBSTRING([c].[City], 2, LEN([c].[City]))
FROM [dbo].[Customers] AS [c]",
                context.SqlLog);
        }

        [TestMethod]
        public void String_Substring2()
        {
            var query = from c in context.Customers
                        select c.City.Substring(1, 2);

            query.ToList();

            Assert.AreEqual(
                @"SELECT SUBSTRING([c].[City], 2, 2)
FROM [dbo].[Customers] AS [c]",
                context.SqlLog);
        }

        [TestMethod]
        public void String_Replace1()
        {
            var query = from c in context.Customers
                        select c.City.Replace('A', 'Z');

            query.ToList();

            Assert.AreEqual(
                @"SELECT REPLACE([c].[City], N'A', N'Z')
FROM [dbo].[Customers] AS [c]",
                context.SqlLog);
        }

        [TestMethod]
        public void String_Replace2()
        {
            var query = from c in context.Customers
                        select c.City.Replace("Be", "Me");

            query.ToList();

            Assert.AreEqual(
                @"SELECT REPLACE([c].[City], N'Be', N'Me')
FROM [dbo].[Customers] AS [c]",
                context.SqlLog);
        }

        [TestMethod]
        public void String_Contains()
        {
            var query = from c in context.Customers
                        where c.City.Contains("B")
                        select new { c.City, c.ContactName };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[City] AS [City], [c].[ContactName] AS [ContactName]
FROM [dbo].[Customers] AS [c]
WHERE CHARINDEX(N'B', [c].[City]) > 0",
                context.SqlLog);
        }

        [TestMethod]
        public void String_StartsWith()
        {
            var query = from c in context.Customers
                        where c.City.StartsWith("Be")
                        select new { c.City, c.ContactName };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[City] AS [City], [c].[ContactName] AS [ContactName]
FROM [dbo].[Customers] AS [c]
WHERE LEFT([c].[City], LEN(N'Be')) = N'Be'",
                context.SqlLog);
        }

        [TestMethod]
        public void String_EndsWith()
        {
            var query = from c in context.Customers
                        where c.City.EndsWith("in")
                        select new { c.City, c.ContactName };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[City] AS [City], [c].[ContactName] AS [ContactName]
FROM [dbo].[Customers] AS [c]
WHERE RIGHT([c].[City], LEN(N'in')) = N'in'",
                context.SqlLog);
        }

        [TestMethod]
        public void String_IsNullOrEmpty()
        {
            var query = from c in context.Customers
                        where !string.IsNullOrEmpty(c.City)
                        select new { c.City, c.ContactName };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[City] AS [City], [c].[ContactName] AS [ContactName]
FROM [dbo].[Customers] AS [c]
WHERE ([c].[City] IS NOT NULL) AND (DATALENGTH([c].[City]) <> 0)",
                context.SqlLog);
        }

        [TestMethod]
        public void String_IsNullOrWhiteSpace()
        {
            var query = from c in context.Customers
                        where !string.IsNullOrWhiteSpace(c.City)
                        select new { c.City, c.ContactName };

            query.ToList();

            Assert.AreEqual(
                @"SELECT [c].[City] AS [City], [c].[ContactName] AS [ContactName]
FROM [dbo].[Customers] AS [c]
WHERE ([c].[City] IS NOT NULL) AND ([c].[City] <> N'')",
                context.SqlLog);
        }
    }
}
