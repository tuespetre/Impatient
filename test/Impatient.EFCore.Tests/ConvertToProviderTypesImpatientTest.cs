using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class ConvertToProviderTypesImpatientTest : ConvertToProviderTypesTestBase<ConvertToProviderTypesImpatientTest.ConvertToProviderTypesImpatientFixture>
    {
        public ConvertToProviderTypesImpatientTest(ConvertToProviderTypesImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact]
        public override void Can_insert_and_read_back_with_binary_key()
        {
            if (!Fixture.SupportsBinaryKeys)
            {
                return;
            }

            using (var context = CreateContext())
            {
                context.Set<BinaryKeyDataType>().Add(
                    new BinaryKeyDataType
                    {
                        Id = new byte[] { 1, 2, 3 }
                    });

                context.Set<BinaryForeignKeyDataType>().Add(
                    new BinaryForeignKeyDataType
                    {
                        Id = 77,
                        BinaryKeyDataTypeId = new byte[] { 1, 2, 3 }
                    });

                Assert.Equal(2, context.SaveChanges());
            }

            using (var context = CreateContext())
            {
                var entity = context
                    .Set<BinaryKeyDataType>()
                    .Include(e => e.Dependents)
                    .Where(e => e.Id == new byte[] { 1, 2, 3 })
                    .ToList().Single();

                Assert.Equal(new byte[] { 1, 2, 3 }, entity.Id);
                Assert.Equal(new byte[] { 1, 2, 3 }, entity.Dependents.First().BinaryKeyDataTypeId);
            }
        }

        public class ConvertToProviderTypesImpatientFixture : ConvertToProviderTypesFixtureBase
        {
            public override bool StrictEquality => true;

            public override bool SupportsAnsi => true;

            public override bool SupportsUnicodeToAnsiConversion => true;

            public override bool SupportsLargeStringComparisons => true;

            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

            public override bool SupportsBinaryKeys => true;

            public override DateTime DefaultDateTime => new DateTime();

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base
                    .AddOptions(builder)
                    .ConfigureWarnings(
                        c => c.Log(RelationalEventId.QueryClientEvaluationWarning)
                            .Log(SqlServerEventId.DecimalTypeDefaultWarning));

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<BuiltInDataTypes>().Property(e => e.Enum8).IsFixedLength();
            }
        }
    }
}
