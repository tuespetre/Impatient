using Microsoft.EntityFrameworkCore;

namespace Impatient.EFCore.Tests
{
    public class TwoDatabasesImpatientTest : TwoDatabasesTestBase
    {
        public TwoDatabasesImpatientTest(FixtureBase fixture) : base(fixture)
        {
        }

        protected override string DummyConnectionString => throw new System.NotImplementedException();

        protected override TwoDatabasesWithDataContext CreateBackingContext(string databaseName)
        {
            throw new System.NotImplementedException();
        }

        protected override DbContextOptionsBuilder CreateTestOptions(DbContextOptionsBuilder optionsBuilder, bool withConnectionString = false, bool withNullConnectionString = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
