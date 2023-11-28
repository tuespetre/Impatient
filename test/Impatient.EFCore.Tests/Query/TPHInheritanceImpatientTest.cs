using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Impatient.EFCore.Tests.Query
{
    public class TPHInheritanceImpatientTest : TPHInheritanceQueryTestBase<TPHInheritanceImpatientTest.Fixture>
    {
        public TPHInheritanceImpatientTest(Fixture fixture, ITestOutputHelper testOutputHelper) : base(fixture, testOutputHelper)
        {
        }

        public new class Fixture : TPHInheritanceQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
