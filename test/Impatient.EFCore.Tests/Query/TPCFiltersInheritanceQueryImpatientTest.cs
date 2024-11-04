using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using static Impatient.EFCore.Tests.Query.TPCFiltersInheritanceQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class TPCFiltersInheritanceQueryImpatientTest(Fixture fixture) : TPCFiltersInheritanceQueryTestBase<Fixture>(fixture)
{
    public new class Fixture : TPCInheritanceQueryFixture
    {
        public override bool EnableFilters => true;

        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
