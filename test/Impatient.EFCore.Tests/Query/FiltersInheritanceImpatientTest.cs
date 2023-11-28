using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class FiltersInheritanceImpatientTest(FiltersInheritanceImpatientTest.Fixture fixture) : FiltersInheritanceQueryTestBase<FiltersInheritanceImpatientTest.Fixture>(fixture)
    {
        public class Fixture : InheritanceQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
