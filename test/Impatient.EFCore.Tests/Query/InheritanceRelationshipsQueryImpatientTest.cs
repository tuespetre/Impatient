using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class InheritanceRelationshipsQueryImpatientTest : InheritanceRelationshipsQueryRelationalTestBase<InheritanceRelationshipsQueryImpatientTest.Fixture>
    {
        public InheritanceRelationshipsQueryImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override Task Include_collection_with_inheritance(bool async)
        {
            return base.Include_collection_with_inheritance(async);
        }

        public new class Fixture : InheritanceRelationshipsQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
