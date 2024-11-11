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
using static Impatient.EFCore.Tests.CommandInterceptionImpatientTest;

namespace Impatient.EFCore.Tests;

public class CommandInterceptionImpatientTest : CommandInterceptionTestBase, IClassFixture<Fixture>
{
    public CommandInterceptionImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.ListLoggerFactory.Clear();
    }

    public new class Fixture : InterceptionFixtureBase
    {
        protected override bool ShouldSubscribeToDiagnosticListener => false;

        protected override string StoreName => nameof(CommandInterceptionImpatientTest);

        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

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
