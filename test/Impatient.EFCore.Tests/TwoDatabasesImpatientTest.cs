using Impatient.EFCore.Tests.Fixtures;
using Microsoft.EntityFrameworkCore;
using System;
using Xunit;

namespace Impatient.EFCore.Tests;

public class TwoDatabasesImpatientTest(SqlServerFixture fixture) : TwoDatabasesTestBase(fixture), IClassFixture<SqlServerFixture>
{
    protected override string DummyConnectionString
        => "Database=DoesNotExist";

    protected override TwoDatabasesWithDataContext CreateBackingContext(string databaseName)
        => throw new NotImplementedException();
        //=> new(Fixture.CreateOptions(ImpatientTestStoreFactory.Instance.Create(databaseName)));

    protected override DbContextOptionsBuilder CreateTestOptions(
        DbContextOptionsBuilder optionsBuilder,
        bool withConnectionString = false,
        bool withNullConnectionString = false)
        => withConnectionString
            ? withNullConnectionString
                ? optionsBuilder.UseSqlServer((string)null)
                : optionsBuilder.UseSqlServer(DummyConnectionString)
            : optionsBuilder.UseSqlServer();
}
