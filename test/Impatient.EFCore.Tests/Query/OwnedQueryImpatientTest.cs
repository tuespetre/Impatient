using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    // These tests seem to be broken at the fixture level
    public class OwnedQueryImpatientTest : OwnedQueryRelationalTestBase<OwnedQueryImpatientFixture>
    {
        public OwnedQueryImpatientTest(OwnedQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }

    public class OwnedQueryImpatientFixture : OwnedQueryRelationalTestBase<OwnedQueryImpatientFixture>.RelationalOwnedQueryFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
