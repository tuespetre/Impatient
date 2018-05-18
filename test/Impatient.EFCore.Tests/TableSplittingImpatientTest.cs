using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;
using Microsoft.Extensions.DependencyInjection;
using System;

#pragma warning disable xUnit1024 // Test methods cannot have overloads

namespace Impatient.EFCore.Tests
{
    public class TableSplittingImpatientTest : TableSplittingTestBase<ImpatientTestStore>
    {
        public override TransportationContext CreateContext(ImpatientTestStore testStore, Action<ModelBuilder> onModelCreating)
        {
            var services = new ServiceCollection();

            var provider
                = services
                    .AddEntityFrameworkSqlServer()
                    .AddImpatientEFCoreQueryCompiler()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .BuildServiceProvider();

            var options
                = new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(provider)
                    .UseSqlServer(testStore.Connection)
                    .Options;

            using (var temp = new TransportationContext(new DbContextOptionsBuilder(options).Options))
            {
                if (temp.Database.EnsureCreated())
                {
                    temp.Seed();
                }
            }

            var context
                = new TransportationContext(
                    new DbContextOptionsBuilder(options)
                        .UseSqlServer(testStore.Connection).Options);

            context.Database.UseTransaction(testStore.Transaction);

            return context;
        }

        public override ImpatientTestStore CreateTestStore(Action<ModelBuilder> onModelCreating)
        {
            return new ImpatientTestStore(@"Server=.\\sqlexpress; Database=efcore-impatient-table-splitting; Trusted_Connection=True");
        }
    }
}
