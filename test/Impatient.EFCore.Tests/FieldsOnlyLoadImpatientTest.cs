using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class FieldsOnlyLoadImpatientTest : FieldsOnlyLoadTestBase<FieldsOnlyLoadImpatientTest.Fixture>
    {
        public FieldsOnlyLoadImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        public new class Fixture : FieldsOnlyLoadFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
