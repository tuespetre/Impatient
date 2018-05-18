using Microsoft.EntityFrameworkCore;
using System;
using Xunit;

namespace Impatient.EFCore.Tests.Utilities
{
    public static class TestSqlLoggerExtensions
    {
        public static void AssertSql<T>(this SharedStoreFixtureBase<T> fixture, string sql)
            where T : DbContext
        {
            var expected = sql.Trim();
            var actual = ((dynamic)fixture).TestSqlLoggerFactory.Sql;

            if (actual != expected)
            {
                throw new Exception($@"Expected:
{expected}

Actual:
{actual}");
            }
        }

        public static void AssertSqlStartsWith<T>(this SharedStoreFixtureBase<T> fixture, string sql)
            where T : DbContext
        {
            var expected = sql.Trim();
            var actual = ((dynamic)fixture).TestSqlLoggerFactory.Sql;

            Assert.StartsWith(expected, actual);
        }
    }
}
