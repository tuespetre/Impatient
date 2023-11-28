using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class ComplexTypesTrackingImpatientTest : ComplexTypesTrackingTestBase<ComplexTypesTrackingImpatientTest.Fixture>
    {
        public ComplexTypesTrackingImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        public new class Fixture : FixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
