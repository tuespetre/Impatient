using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class TPTManyToManyQueryImpatientTest : TPTManyToManyQueryRelationalTestBase<TPTManyToManyQueryImpatientTest.Fixture>
    {
        public TPTManyToManyQueryImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public class Fixture : TPTManyToManyQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
