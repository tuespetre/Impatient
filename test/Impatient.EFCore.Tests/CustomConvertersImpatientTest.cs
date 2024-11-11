using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests;

public class CustomConvertersImpatientTest : CustomConvertersTestBase<CustomConvertersImpatientTest.CustomConvertersImpatientFixture>
{
    public CustomConvertersImpatientTest(CustomConvertersImpatientFixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => Fixture.TestSqlLoggerFactory.AssertSql(expected);

    public class CustomConvertersImpatientFixture : CustomConvertersFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

        public override bool StrictEquality => true;

        public override bool SupportsAnsi => true;

        public override bool SupportsUnicodeToAnsiConversion => true;

        public override bool SupportsLargeStringComparisons => true;

        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

        public override bool SupportsBinaryKeys => true;

        public override DateTime DefaultDateTime => new();

        public override bool SupportsDecimalComparisons => true;

        public override bool PreservesDateTimeKind => false;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base
                .AddOptions(builder)
                .ConfigureWarnings(
                    c => c.Log(SqlServerEventId.DecimalTypeDefaultWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<BuiltInDataTypes>().Property(e => e.TestBoolean).IsFixedLength();
        }
    }

    // Really not sure what the problem is here, seems to execute okay, but EF Core expects a failure
    [Fact(Skip = Punt)]
    public override void Composition_over_collection_of_complex_mapped_as_scalar()
    {
        base.Composition_over_collection_of_complex_mapped_as_scalar();
    }

    // This is a materialization issue on our end. We must be supplying a default value when we shouldn't. 
    public override void Optional_owned_with_converter_reading_non_nullable_column()
    {
        base.Optional_owned_with_converter_reading_non_nullable_column();
    }
}
