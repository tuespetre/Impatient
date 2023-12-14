using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestModels.TransportationModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Impatient.EFCore.Tests
{
    public class TableSplittingImpatientTest : TableSplittingTestBase
    {
        public TableSplittingImpatientTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance; 
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Engine>().ToTable("Vehicles")
                .Property(e => e.Computed).HasComputedColumnSql("1", stored: true);
        }
    }
}
