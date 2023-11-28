using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class TPCFiltersInheritanceQueryImpatientTest : TPCFiltersInheritanceQueryTestBase<TPCFiltersInheritanceQueryImpatientTest.Fixture>
    {
        public TPCFiltersInheritanceQueryImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        public new class Fixture : TPCInheritanceQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
