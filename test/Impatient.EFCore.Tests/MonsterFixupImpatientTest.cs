using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class MonsterFixupImpatientTest : MonsterFixupTestBase<MonsterFixupImpatientTest.MonsterFixupImpatientFixture>
    {
        public MonsterFixupImpatientTest(MonsterFixupImpatientFixture fixture) : base(fixture)
        {
        }

        public class MonsterFixupImpatientFixture : MonsterFixupChangedChangingFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder).ConfigureWarnings(w => w.Log(RelationalEventId.QueryClientEvaluationWarning));

            protected override void OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(
                ModelBuilder builder)
            {
                base.OnModelCreating<TMessage, TProduct, TProductPhoto, TProductReview, TComputerDetail, TDimensions>(builder);

                builder.Entity<TMessage>().Property(e => e.MessageId).UseSqlServerIdentityColumn();

                builder.Entity<TProduct>()
                    .OwnsOne(c => (TDimensions)c.Dimensions, db =>
                    {
                        db.Property(d => d.Depth).HasColumnType("decimal(18,2)");
                        db.Property(d => d.Width).HasColumnType("decimal(18,2)");
                        db.Property(d => d.Height).HasColumnType("decimal(18,2)");
                    });

                builder.Entity<TProductPhoto>().Property(e => e.PhotoId).UseSqlServerIdentityColumn();
                builder.Entity<TProductReview>().Property(e => e.ReviewId).UseSqlServerIdentityColumn();

                builder.Entity<TComputerDetail>()
                    .OwnsOne(c => (TDimensions)c.Dimensions, db =>
                    {
                        db.Property(d => d.Depth).HasColumnType("decimal(18,2)");
                        db.Property(d => d.Width).HasColumnType("decimal(18,2)");
                        db.Property(d => d.Height).HasColumnType("decimal(18,2)");
                    });
            }
        }
    }
}
