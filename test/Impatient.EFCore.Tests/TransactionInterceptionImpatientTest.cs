using Impatient.EFCore.Tests.Utilities;
using Impatient.EntityFrameworkCore.SqlServer;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using static Impatient.EFCore.Tests.TransactionInterceptionImpatientTest;

namespace Impatient.EFCore.Tests;

public class TransactionInterceptionImpatientTest : TransactionInterceptionTestBase, IClassFixture<Fixture>
{
    public TransactionInterceptionImpatientTest(Fixture fixture) : base(fixture)
    {
    }

    public new class Fixture : InterceptionFixtureBase
    {
        protected override string StoreName => "TransactionInterception";

        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

        protected override bool ShouldSubscribeToDiagnosticListener => false;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            new SqlServerDbContextOptionsBuilder(base.AddOptions(builder))
                .ExecutionStrategy(d => new SqlServerExecutionStrategy(d));

            return builder;
        }

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<IInterceptor> injectedInterceptors)
        {
            var services = serviceCollection
                .AddEntityFrameworkSqlServer()
                .AddImpatientEFCoreQueryCompiler(ImpatientCompatibility.Default)
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory());

            return base.InjectInterceptors(services, injectedInterceptors);
        }
    }
}
