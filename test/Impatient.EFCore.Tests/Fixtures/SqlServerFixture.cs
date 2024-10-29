using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace Impatient.EFCore.Tests.Fixtures;

public class SqlServerFixture : ServiceProviderFixtureBase
{
    public static IServiceProvider DefaultServiceProvider { get; }
        = new ServiceCollection().AddEntityFrameworkSqlServer().BuildServiceProvider(validateScopes: true);

    public TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ServiceProvider.GetRequiredService<ILoggerFactory>();

    protected override ITestStoreFactory TestStoreFactory
        => ImpatientTestStoreFactory.Instance;

    public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        => base.AddOptions(builder).ConfigureWarnings(
            w =>
            {
                //w.Log(SqlServerEventId.ByteIdentityColumnWarning);
                //w.Log(SqlServerEventId.JsonTypeExperimental);
                //w.Log(SqlServerEventId.DecimalTypeKeyWarning);
            });
}
