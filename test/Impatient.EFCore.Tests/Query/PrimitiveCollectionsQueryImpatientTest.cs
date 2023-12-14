using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;

namespace Impatient.EFCore.Tests.Query
{
    public class PrimitiveCollectionsQueryImpatientTest(PrimitiveCollectionsQueryImpatientTest.Fixture fixture) 
        : PrimitiveCollectionsQueryRelationalTestBase<PrimitiveCollectionsQueryImpatientTest.Fixture>(fixture)
    {
        public override Task Column_collection_of_bools_Contains(bool async)
        {
            return base.Column_collection_of_bools_Contains(async);
        }

        public new class Fixture : PrimitiveCollectionsQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
