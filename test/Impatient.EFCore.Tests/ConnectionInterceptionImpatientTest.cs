using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using System.Data.Common;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Xunit;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Impatient.EFCore.Tests;

public abstract class ConnectionInterceptionImpatientTestBase : ConnectionInterceptionTestBase
{
    public ConnectionInterceptionImpatientTestBase(InterceptionFixtureBase fixture) : base(fixture)
    {
    }

    public abstract class InterceptionImpatientFixtureBase : InterceptionFixtureBase
    {
        protected override string StoreName => "ConnectionInterception";

        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
            => base.InjectInterceptors(serviceCollection.AddEntityFrameworkSqlServer(), injectedInterceptors);
    }

    protected override DbContextOptionsBuilder ConfigureProvider(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer();

    protected override BadUniverseContext CreateBadUniverse(DbContextOptionsBuilder optionsBuilder)
        => new(optionsBuilder.UseSqlServer(new FakeDbConnection()).Options);

    public class FakeDbConnection : DbConnection
    {
        [AllowNull]
        public override string ConnectionString { get; set; }

        public override string Database
            => "Database";

        public override string DataSource
            => "DataSource";

        public override string ServerVersion
            => throw new NotImplementedException();

        public override ConnectionState State
            => ConnectionState.Closed;

        public override void ChangeDatabase(string databaseName)
            => throw new NotImplementedException();

        public override void Close()
            => throw new NotImplementedException();

        public override void Open()
            => throw new NotImplementedException();

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
            => throw new NotImplementedException();

        protected override DbCommand CreateDbCommand()
            => throw new NotImplementedException();
    }
}

public class ConnectionInterceptionImpatientTest
        : ConnectionInterceptionImpatientTestBase, IClassFixture<ConnectionInterceptionImpatientTest.InterceptionImpatientFixture>
{
    public ConnectionInterceptionImpatientTest(InterceptionImpatientFixture fixture)
        : base(fixture)
    {
    }

    public class InterceptionImpatientFixture : InterceptionImpatientFixtureBase
    {
        protected override bool ShouldSubscribeToDiagnosticListener
            => false;
    }
}

public class ConnectionInterceptionWithConnectionStringSqlServerTest
    : ConnectionInterceptionImpatientTestBase,
        IClassFixture<ConnectionInterceptionWithConnectionStringSqlServerTest.InterceptionImpatientFixture>
{
    public ConnectionInterceptionWithConnectionStringSqlServerTest(InterceptionImpatientFixture fixture)
        : base(fixture)
    {
    }

    public class InterceptionImpatientFixture : InterceptionImpatientFixtureBase
    {
        protected override bool ShouldSubscribeToDiagnosticListener
            => false;
    }

    protected override DbContextOptionsBuilder ConfigureProvider(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Database=Dummy");
}

public class ConnectionInterceptionWithDiagnosticsImpatientTest
    : ConnectionInterceptionImpatientTestBase,
        IClassFixture<ConnectionInterceptionWithDiagnosticsImpatientTest.InterceptionImpatientFixture>
{
    public ConnectionInterceptionWithDiagnosticsImpatientTest(InterceptionImpatientFixture fixture)
        : base(fixture)
    {
    }

    public class InterceptionImpatientFixture : InterceptionImpatientFixtureBase
    {
        protected override bool ShouldSubscribeToDiagnosticListener
            => true;
    }
}
