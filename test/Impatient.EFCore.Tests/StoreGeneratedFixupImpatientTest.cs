using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests;

public class StoreGeneratedFixupImpatientTest : StoreGeneratedFixupRelationalTestBase<StoreGeneratedFixupImpatientTest.Fixture>
{
    public StoreGeneratedFixupImpatientTest(Fixture fixture) : base(fixture)
    {
    }

    protected override void MarkIdsTemporary(DbContext context, object dependent, object principal)
    {
        var entry = context.Entry(dependent);
        entry.Property("Id1").IsTemporary = true;
        entry.Property("Id2").IsTemporary = true;

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsForeignKey())
            {
                property.IsTemporary = true;
            }
        }

        entry = context.Entry(principal);
        entry.Property("Id1").IsTemporary = true;
        entry.Property("Id2").IsTemporary = true;
    }

    protected override void MarkIdsTemporary(DbContext context, object game, object level, object item)
    {
        var entry = context.Entry(game);
        entry.Property("Id").IsTemporary = true;

        entry = context.Entry(item);
        entry.Property("Id").IsTemporary = true;
    }

    protected override bool EnforcesFKs
        => true;

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public new class Fixture : StoreGeneratedFixupRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory
            => ImpatientTestStoreFactory.Instance;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Parent>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<Child>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ParentPN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ChildPN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ParentDN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ChildDN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ParentNN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ChildNN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<CategoryDN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ProductDN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<CategoryPN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ProductPN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<CategoryNN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<ProductNN>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<Category>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<Product>(
                b =>
                {
                    b.Property(e => e.Id1).ValueGeneratedOnAdd();
                    b.Property(e => e.Id2).ValueGeneratedOnAdd().HasDefaultValueSql("newid()");
                });

            modelBuilder.Entity<Item>(b => b.Property(e => e.Id).ValueGeneratedOnAdd());

            modelBuilder.Entity<Game>(b => b.Property(e => e.Id).ValueGeneratedOnAdd().HasDefaultValueSql("newid()"));
        }
    }
}
