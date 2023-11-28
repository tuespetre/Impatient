using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class PrimitiveCollectionsQueryImpatientTest(PrimitiveCollectionsQueryImpatientTest.Fixture fixture) : PrimitiveCollectionsQueryRelationalTestBase<PrimitiveCollectionsQueryImpatientTest.Fixture>(fixture)
    {
        public new class Fixture : PrimitiveCollectionsQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
