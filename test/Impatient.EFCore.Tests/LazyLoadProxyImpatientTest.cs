using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class LazyLoadProxyImpatientTest : LazyLoadProxyTestBase<LazyLoadProxyImpatientTest.Fixture>
    {
        public LazyLoadProxyImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        public class Fixture : LoadFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
