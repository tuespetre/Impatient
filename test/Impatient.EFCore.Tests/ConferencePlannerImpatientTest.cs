using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class ConferencePlannerImpatientTest : ConferencePlannerTestBase<ConferencePlannerImpatientTest.ConferencePlannerFixture>
    {
        public ConferencePlannerImpatientTest(ConferencePlannerFixture fixture) : base(fixture)
        {
            fixture.ListLoggerFactory.Clear();
        }

        public class ConferencePlannerFixture : ConferencePlannerFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
