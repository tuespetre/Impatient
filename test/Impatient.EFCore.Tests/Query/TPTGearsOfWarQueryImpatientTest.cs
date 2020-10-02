using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class TPTGearsOfWarQueryImpatientTest : TPTGearsOfWarQueryRelationalTestBase<TPTGearsOfWarQueryImpatientTest.Fixture>
    {
        public TPTGearsOfWarQueryImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public class Fixture : TPTGearsOfWarQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
