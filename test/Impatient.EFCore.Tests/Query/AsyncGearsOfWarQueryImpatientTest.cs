using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class AsyncGearsOfWarQueryImpatientTest : AsyncGearsOfWarQueryRelationalTestBase<AsyncGearsOfWarQueryImpatientTest.Fixture>
    {
        public AsyncGearsOfWarQueryImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        public class Fixture : GearsOfWarQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
