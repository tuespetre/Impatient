using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class ComplexTypeQueryImpatientTest(ComplexTypeQueryImpatientTest.Fixture fixture) : ComplexTypeQueryRelationalTestBase<ComplexTypeQueryImpatientTest.Fixture>(fixture)
    {
        public new class Fixture : ComplexTypeQueryRelationalFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
