using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class CompositeKeysQueryImpatientTest(CompositeKeysQueryImpatientTest.Fixture fixture) : CompositeKeysQueryRelationalTestBase<CompositeKeysQueryImpatientTest.Fixture>(fixture)
    {
        public new class Fixture : CompositeKeysQueryRelationalFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
