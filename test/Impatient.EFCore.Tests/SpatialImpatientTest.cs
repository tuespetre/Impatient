using Impatient.EFCore.Tests.Utilities;
using Impatient.EntityFrameworkCore.SqlServer;
using Impatient.Query.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.SpatialModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;

namespace Impatient.EFCore.Tests;

public class SpatialImpatientTest : SpatialTestBase<SpatialImpatientTest.Fixture>
{
    public SpatialImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.ListLoggerFactory.Clear();
    }

    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
        facade.UseTransaction(transaction.GetDbTransaction());
    }

    public new class Fixture : SpatialFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

        protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
            => base.AddServices(serviceCollection)
                .AddEntityFrameworkSqlServerNetTopologySuite()
                .AddImpatientEFCoreQueryCompiler(ImpatientCompatibility.Default);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
        {
            var optionsBuilder = base.AddOptions(builder);
            new SqlServerDbContextOptionsBuilder(optionsBuilder).UseNetTopologySuite();

            return optionsBuilder;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<LineStringEntity>().Property(e => e.LineString).HasColumnType("geometry");
            modelBuilder.Entity<MultiLineStringEntity>().Property(e => e.MultiLineString).HasColumnType("geometry");
            modelBuilder.Entity<PointEntity>(
                x =>
                {
                    x.Property(e => e.Geometry).HasColumnType("geometry");
                    x.Property(e => e.Point).HasColumnType("geometry");
                    x.Property(e => e.PointZ).HasColumnType("geometry");
                    x.Property(e => e.PointM).HasColumnType("geometry");
                    x.Property(e => e.PointZM).HasColumnType("geometry");
                });
            modelBuilder.Entity<PolygonEntity>().Property(e => e.Polygon).HasColumnType("geometry");
            modelBuilder.Entity<GeoPointEntity>().Property(e => e.Location).HasColumnType("geometry");
        }
    }
}
