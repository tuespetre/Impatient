using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class MappingQueryImpatientTest : MappingQueryTestBase<MappingQueryImpatientTest.MappingQueryImpatientFixture>
    {
        public MappingQueryImpatientTest(MappingQueryImpatientFixture fixture) : base(fixture)
        {
        }

        public class MappingQueryImpatientFixture : MappingQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

            protected override string DatabaseSchema => "dbo";

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<MappedCustomer>(e =>
                {
                    e.Property(c => c.CompanyName2).Metadata.SetColumnName("CompanyName");
                    e.Metadata.SetTableName("Customers");
                    e.Metadata.SetSchema("dbo");
                });

                modelBuilder.Entity<MappedEmployee>()
                    .Property(c => c.EmployeeID)
                    .HasColumnType("int");
            }
        }
    }
}
