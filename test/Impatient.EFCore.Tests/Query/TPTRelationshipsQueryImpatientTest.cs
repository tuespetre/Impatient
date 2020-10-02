using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class TPTRelationshipsQueryImpatientTest : TPTRelationshipsQueryTestBase<TPTRelationshipsQueryImpatientTest.Fixture>
    {
        public TPTRelationshipsQueryImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public class Fixture : TPTRelationshipsQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
