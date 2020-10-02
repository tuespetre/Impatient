using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class TPTManyToManyNoTrackingQueryImpatientTest : TPTManyToManyNoTrackingQueryRelationalTestBase<TPTManyToManyNoTrackingQueryImpatientTest.Fixture>
    {
        public TPTManyToManyNoTrackingQueryImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public class Fixture : TPTManyToManyQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
