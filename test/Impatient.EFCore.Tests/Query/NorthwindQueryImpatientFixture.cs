using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public abstract class NorthwindQueryImpatientFixtureBase<TCustomizer> : NorthwindQueryRelationalFixture<TCustomizer>
        where TCustomizer : IModelCustomizer, new()
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Customer>()
                .Property(c => c.CustomerID)
                .HasColumnType("nchar(5)");

            modelBuilder.Entity<Employee>(b =>
            {
                b.Property(c => c.EmployeeID).HasColumnType("int");
                b.Property(c => c.ReportsTo).HasColumnType("int");
            });

            modelBuilder.Entity<Order>()
                .Property(o => o.EmployeeID)
                .HasColumnType("int");

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasColumnType("money");

            modelBuilder.Entity<Product>(b =>
            {
                b.Property(p => p.UnitPrice).HasColumnType("money");
                b.Property(p => p.UnitsInStock).HasColumnType("smallint");
            });

            modelBuilder.Entity<MostExpensiveProduct>().Property(p => p.UnitPrice).HasColumnType("money");
            modelBuilder.Entity<MostExpensiveProduct>().HasKey(mep => mep.TenMostExpensiveProducts);

            modelBuilder.Entity<Customer>().Property(c => c.CustomerID).ValueGeneratedNever();
            modelBuilder.Entity<Employee>().Property(c => c.EmployeeID).ValueGeneratedNever();
            modelBuilder.Entity<Order>().Property(c => c.OrderID).ValueGeneratedNever();
            modelBuilder.Entity<Product>().Property(c => c.ProductID).ValueGeneratedNever();
        }
    }

    public class NorthwindQueryImpatientFixture : NorthwindQueryImpatientFixtureBase<NoopModelCustomizer>
    {
    }
}
