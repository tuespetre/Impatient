using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update;

namespace Impatient.EFCore.Tests.Update
{
    public class StoreValueGenerationImpatientTest : StoreValueGenerationTestBase<StoreValueGenerationImpatientTest.Fixture>
    {
        public StoreValueGenerationImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        public new class Fixture : StoreValueGenerationFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
