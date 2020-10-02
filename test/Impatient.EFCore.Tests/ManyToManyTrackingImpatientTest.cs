using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class ManyToManyTrackingImpatientTest : ManyToManyTrackingTestBase<ManyToManyTrackingImpatientTest.Fixture>
    {
        public ManyToManyTrackingImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.ListLoggerFactory.Clear();
        }

        public class Fixture : ManyToManyTrackingFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
