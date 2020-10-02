using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class FiltersInheritanceImpatientTest : FiltersInheritanceQueryTestBase<FiltersInheritanceImpatientTest.Fixture>
    {
        public FiltersInheritanceImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        public class Fixture : InheritanceQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

            protected override bool EnableFilters => true;
        }
    }
}
