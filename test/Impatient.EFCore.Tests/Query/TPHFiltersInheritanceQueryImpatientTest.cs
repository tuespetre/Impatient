using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query;

public class TPHFiltersInheritanceQueryImpatientTest(TPHFiltersInheritanceQueryImpatientTest.Fixture fixture) : FiltersInheritanceQueryTestBase<TPHFiltersInheritanceQueryImpatientTest.Fixture>(fixture)
{
    public new class Fixture : TPHInheritanceQueryFixture
    {
        public override bool EnableFilters => true;

        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
