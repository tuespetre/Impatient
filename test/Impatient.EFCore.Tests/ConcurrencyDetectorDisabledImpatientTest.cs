using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class ConcurrencyDetectorDisabledImpatientTest : ConcurrencyDetectorDisabledRelationalTestBase<ConcurrencyDetectorDisabledImpatientTest.Fixture>
    {
        public ConcurrencyDetectorDisabledImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        public new class Fixture : ConcurrencyDetectorFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
