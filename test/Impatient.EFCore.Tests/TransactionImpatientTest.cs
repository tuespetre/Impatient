using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Data.SqlClient;

namespace Impatient.EFCore.Tests
{
    public class TransactionImpatientTest : TransactionTestBase<TransactionImpatientTest.TransactionImpatientFixture>
    {
        public TransactionImpatientTest(TransactionImpatientFixture fixture) : base(fixture)
        {
        }

        protected override bool SnapshotSupported => true;

        protected override bool AmbientTransactionsSupported => true;

        protected override DbContext CreateContextWithConnectionString()
        {
            var options = Fixture.AddOptions(
                    new DbContextOptionsBuilder()
                        .UseSqlServer(TestStore.ConnectionString))
                .UseInternalServiceProvider(Fixture.ServiceProvider);

            return new DbContext(options.Options);
        }

        public class TransactionImpatientFixture : TransactionFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

            protected override void Seed(DbContext context)
            {
                base.Seed(context);

                var database = context.Database.GetDbConnection().Database;

#pragma warning disable EF1000 // Possible SQL injection vulnerability.
                context.Database.ExecuteSqlCommand($"ALTER DATABASE [{database}] SET ALLOW_SNAPSHOT_ISOLATION ON".ToString());
                context.Database.ExecuteSqlCommand($"ALTER DATABASE [{database}] SET READ_COMMITTED_SNAPSHOT ON".ToString());
#pragma warning restore EF1000 // Possible SQL injection vulnerability.
            }

            public override void Reseed()
            {
                using (var context = CreateContext())
                {
                    context.Set<TransactionCustomer>().RemoveRange(context.Set<TransactionCustomer>());
                    context.SaveChanges();

                    base.Seed(context);
                }
            }

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            {
                new SqlServerDbContextOptionsBuilder(
                        base.AddOptions(builder)
                            .ConfigureWarnings(
                                w => w.Log(RelationalEventId.QueryClientEvaluationWarning)
                                      .Log(CoreEventId.FirstWithoutOrderByAndFilterWarning)))
                    .MaxBatchSize(1);
                return builder;
            }
        }
    }
}
