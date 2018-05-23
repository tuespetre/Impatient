using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    // These tests seem to be broken at the fixture level
    public class OwnedQueryImpatientTest : RelationalOwnedQueryTestBase<OwnedQueryImpatientFixture>
    {
        public OwnedQueryImpatientTest(OwnedQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }

    public class OwnedQueryImpatientFixture : RelationalOwnedQueryTestBase<OwnedQueryImpatientFixture>.RelationalOwnedQueryFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
