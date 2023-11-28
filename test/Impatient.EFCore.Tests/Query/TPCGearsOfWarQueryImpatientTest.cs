using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class TPCGearsOfWarQueryImpatientTest : TPCGearsOfWarQueryRelationalTestBase<TPCGearsOfWarQueryImpatientTest.Fixture>
    {
        public TPCGearsOfWarQueryImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        public new class Fixture : TPCGearsOfWarQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
