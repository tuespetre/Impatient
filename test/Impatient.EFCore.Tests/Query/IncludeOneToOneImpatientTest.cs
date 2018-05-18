using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class IncludeOneToOneImpatientTest : IncludeOneToOneTestBase<OneToOneQueryImpatientFixture>
    {
        public IncludeOneToOneImpatientTest(OneToOneQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }

    public class OneToOneQueryImpatientFixture : IncludeOneToOneTestBase<OneToOneQueryImpatientFixture>.OneToOneQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
