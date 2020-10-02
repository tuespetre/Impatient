using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class InheritanceRelationshipsQueryImpatientTest : InheritanceRelationshipsQueryRelationalTestBase<InheritanceRelationshipsQueryImpatientTest.Fixture>
    {
        public InheritanceRelationshipsQueryImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public class Fixture : InheritanceRelationshipsQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
