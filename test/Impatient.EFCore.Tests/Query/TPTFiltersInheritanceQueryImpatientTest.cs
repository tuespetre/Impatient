using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using static Impatient.EFCore.Tests.Query.TPTFiltersInheritanceQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class TPTFiltersInheritanceQueryImpatientTest(Fixture fixture) : TPTFiltersInheritanceQueryTestBase<Fixture>(fixture)
{
    public new class Fixture : TPTInheritanceQueryFixture
    {
        public override bool EnableFilters => true;

        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
