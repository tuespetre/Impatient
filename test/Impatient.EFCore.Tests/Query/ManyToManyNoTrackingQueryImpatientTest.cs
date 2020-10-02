using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class ManyToManyNoTrackingQueryImpatientTest : ManyToManyNoTrackingQueryRelationalTestBase<ManyToManyNoTrackingQueryImpatientTest.Fixture>
    {
        public ManyToManyNoTrackingQueryImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.ListLoggerFactory.Clear();
        }

        public class Fixture : ManyToManyQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
