using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Impatient.EFCore.Tests
{
    public class NorthwindQueryImpatientFixture : NorthwindQueryRelationalFixture
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory { get; } = new TestSqlLoggerFactory();

        public override DbContextOptions BuildOptions(IServiceCollection additionalServices = null)
        {
            var services = additionalServices ?? new ServiceCollection();

            new ImpatientDbContextOptionsExtension().ApplyServices(services);

            var provider
                = services
                    .AddEntityFrameworkSqlServer()
                    .AddImpatientEFCoreQueryCompiler()
                    .AddSingleton(TestModelSource.GetFactory(OnModelCreating))
                    .AddSingleton<ILoggerFactory>(TestSqlLoggerFactory)
                    .BuildServiceProvider();

            return new DbContextOptionsBuilder()
                .UseInternalServiceProvider(provider)
                .UseSqlServer(@"Server=.\sqlexpress; Database=NORTHWND; Trusted_Connection=true; MultipleActiveResultSets=True")
                .Options;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>()
                .Property(c => c.CustomerID)
                .HasColumnType("nchar(5)");

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasColumnType("money");

            modelBuilder.Entity<Product>()
                .Property(p => p.UnitPrice)
                .HasColumnType("money");
        }
    }
}
