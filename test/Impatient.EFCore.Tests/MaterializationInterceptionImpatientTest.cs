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
using static Impatient.EFCore.Tests.MaterializationInterceptionImpatientTest;

namespace Impatient.EFCore.Tests;

public class MaterializationInterceptionImpatientTest : MaterializationInterceptionTestBase<ImpatientSqlServerLibraryContext>, IClassFixture<Fixture>
{
    public MaterializationInterceptionImpatientTest(Fixture fixture) : base(fixture)
    {
    }

    public class ImpatientSqlServerLibraryContext : LibraryContext
    {
        public ImpatientSqlServerLibraryContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TestEntity30244>().OwnsMany(e => e.Settings, b => b.ToJson());
        }
    }

    public override LibraryContext CreateContext(IEnumerable<ISingletonInterceptor> interceptors, bool inject)
    {
        return new ImpatientSqlServerLibraryContext(base.Fixture.CreateOptions(interceptors, inject));
    }

    public new class Fixture : SingletonInterceptorsFixtureBase
    {
        protected override string StoreName => nameof(MaterializationInterceptionImpatientTest);

        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            new SqlServerDbContextOptionsBuilder(base.AddOptions(builder))
                .ExecutionStrategy(d => new SqlServerExecutionStrategy(d));

            return builder;
        }

        protected override IServiceCollection InjectInterceptors(
            IServiceCollection serviceCollection,
            IEnumerable<ISingletonInterceptor> injectedInterceptors)
        {
            var services = serviceCollection
                .AddEntityFrameworkSqlServer()
                .AddImpatientEFCoreQueryCompiler(ImpatientCompatibility.Default)
                .AddSingleton<ILoggerFactory>(new TestSqlLoggerFactory());

            return base.InjectInterceptors(services, injectedInterceptors);
        }
    }
}
