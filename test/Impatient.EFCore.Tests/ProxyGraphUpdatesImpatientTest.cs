using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Impatient.EFCore.Tests
{
    public class ProxyGraphUpdatesImpatientTest : ProxyGraphUpdatesTestBase<ProxyGraphUpdatesImpatientTest.ProxyGraphUpdatesImpatientFixture>
    {
        public ProxyGraphUpdatesImpatientTest(ProxyGraphUpdatesImpatientFixture fixture) : base(fixture)
        {
        }

        protected override bool DoesLazyLoading => throw new NotImplementedException();

        protected override bool DoesChangeTracking => throw new NotImplementedException();

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class ProxyGraphUpdatesImpatientFixture : ProxyGraphUpdatesFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

            protected override string StoreName { get; } = "ProxyGraphLazyLoadingUpdatesTest";

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder.UseLazyLoadingProxies());

            protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
                => base.AddServices(serviceCollection.AddEntityFrameworkProxies());

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.UseIdentityColumns();

                base.OnModelCreating(modelBuilder, context);
            }
        }
    }
}
