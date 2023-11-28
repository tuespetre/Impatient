using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class CompositeKeysSplitQueryImpatientTest : CompositeKeysSplitQueryRelationalTestBase<CompositeKeysSplitQueryImpatientTest.Fixture>
    {
        public CompositeKeysSplitQueryImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        public new class Fixture : CompositeKeysQueryRelationalFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
