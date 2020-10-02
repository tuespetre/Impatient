using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class KeysWithConvertersImpatientTest : KeysWithConvertersTestBase<KeysWithConvertersImpatientTest.Fixture>
    {
        public KeysWithConvertersImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.ListLoggerFactory.Clear();
        }

        public class Fixture : KeysWithConvertersFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
