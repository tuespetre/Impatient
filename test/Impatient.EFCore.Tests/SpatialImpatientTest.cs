using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class SpatialImpatientTest : SpatialTestBase<SpatialImpatientTest.Fixture>
    {
        public SpatialImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.ListLoggerFactory.Clear();
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
            throw new System.NotImplementedException();
        }

        public class Fixture : SpatialFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
