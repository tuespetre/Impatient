using Impatient.Query;
using Impatient.Tests.Utilities;
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
    }
}
