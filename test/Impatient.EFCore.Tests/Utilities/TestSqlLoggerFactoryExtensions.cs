using Microsoft.EntityFrameworkCore.TestUtilities;
using System;
using Xunit;

namespace Impatient.EFCore.Tests.Utilities;

public static class TestSqlLoggerExtensions
{
    public static void AssertSql(this TestSqlLoggerFactory sqlLoggerFactory, string sql)
    {
        var expected = sql.Trim();
        var actual = sqlLoggerFactory.Sql;

        if (actual != expected)
        {
            throw new Exception($@"Expected:
{expected}

Actual:
{actual}");
        }
    }

    public static void AssertSqlStartsWith(this TestSqlLoggerFactory sqlLoggerFactory, string sql)
    {
        var expected = sql.Trim();
        var actual = sqlLoggerFactory.Sql;

        Assert.StartsWith(expected, actual);
    }
}
