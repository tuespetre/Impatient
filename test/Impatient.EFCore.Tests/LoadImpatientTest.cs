using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class LoadImpatientTest : LoadTestBase<LoadImpatientTest.LoadImpatientFixture>
    {
        public LoadImpatientTest(LoadImpatientFixture fixture) : base(fixture)
        {
        }

        public class LoadImpatientFixture : LoadFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
