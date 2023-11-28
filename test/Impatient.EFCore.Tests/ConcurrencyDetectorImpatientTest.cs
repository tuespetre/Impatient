using System.Threading.Tasks;
using Impatient.EFCore.Tests.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class ConcurrencyDetectorImpatientTest : ConcurrencyDetectorEnabledRelationalTestBase<ConcurrencyDetectorImpatientTest.Fixture>
    {
        public class Fixture : ConcurrencyDetectorFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => throw new System.NotImplementedException();
        }

        public ConcurrencyDetectorImpatientTest(Fixture fixture) : base(fixture)
        {
        }
    }
}
