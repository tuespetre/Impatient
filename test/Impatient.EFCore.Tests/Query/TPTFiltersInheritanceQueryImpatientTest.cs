using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class TPTFiltersInheritanceQueryImpatientTest : TPTFiltersInheritanceQueryTestBase<TPTFiltersInheritanceQueryImpatientTest.Fixture>
    {
        public TPTFiltersInheritanceQueryImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public class Fixture : TPTInheritanceQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
