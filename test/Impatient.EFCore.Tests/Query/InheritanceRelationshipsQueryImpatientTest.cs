using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class InheritanceRelationshipsQueryImpatientTest : InheritanceRelationshipsQueryTestBase<InheritanceRelationshipsQueryImpatientFixture>
    {
        public InheritanceRelationshipsQueryImpatientTest(InheritanceRelationshipsQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }

    public class InheritanceRelationshipsQueryImpatientFixture : InheritanceRelationshipsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
