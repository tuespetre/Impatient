using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using static Impatient.EFCore.Tests.PropertyValuesImpatientTest;

namespace Impatient.EFCore.Tests;

public class PropertyValuesImpatientTest(Fixture fixture) : PropertyValuesTestBase<Fixture>(fixture)
{
    public new class Fixture : PropertyValuesFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        
        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Building>()
                .Property(b => b.Value).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<CurrentEmployee>()
                .Property(ce => ce.LeaveBalance).HasColumnType("decimal(18,2)");
        }
    }

    public override Task Store_values_can_be_copied_into_an_object()
    {
        /* 
            was failing at first because of
                .Where(e => Equals(Property(e, "BuildingId"), value(Microsoft.EntityFrameworkCore.Storage.ValueBuffer).get_Item(0)))

                TODO: add a simplified test case for the above

            now failing because the NewArray expression is not being considered translatable for some reason -- investigate why
        */

        return base.Store_values_can_be_copied_into_an_object();
    }
}
