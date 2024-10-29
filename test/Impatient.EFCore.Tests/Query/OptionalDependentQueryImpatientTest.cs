using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query;

public class OptionalDependentQueryImpatientTest : OptionalDependentQueryTestBase<OptionalDependentQueryImpatientTest.Fixture>
{
    public OptionalDependentQueryImpatientTest(Fixture fixture) : base(fixture)
    {
    }

    public new class Fixture : OptionalDependentQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
