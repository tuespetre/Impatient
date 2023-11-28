using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class Ef6GroupByImpatientTest(Ef6GroupByImpatientTest.Fixture fixture) : Ef6GroupByTestBase<Ef6GroupByImpatientTest.Fixture>(fixture)
    {
        public new class Fixture : Ef6GroupByFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
