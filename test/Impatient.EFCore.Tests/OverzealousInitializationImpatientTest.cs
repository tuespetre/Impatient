using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class OverzealousInitializationImpatientTest : OverzealousInitializationTestBase<OverzealousInitializationImpatientTest.Fixture>
    {
        public OverzealousInitializationImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.ListLoggerFactory.Clear();
        }

        public class Fixture : OverzealousInitializationFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
