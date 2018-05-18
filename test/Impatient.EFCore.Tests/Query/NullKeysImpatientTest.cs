using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class NullKeysImpatientTest : NullKeysTestBase<NullKeysImpatientFixture>
    {
        public NullKeysImpatientTest(NullKeysImpatientFixture fixture) : base(fixture)
        {
        }
    }

    public class NullKeysImpatientFixture : NullKeysTestBase<NullKeysImpatientFixture>.NullKeysFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
