using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests;

public class ValueConvertersEndToEndImpatientTest : ValueConvertersEndToEndTestBase<ValueConvertersEndToEndImpatientTest.Fixture>
{
    public ValueConvertersEndToEndImpatientTest(Fixture fixture) : base(fixture)
    {
    }

    public new class Fixture : ValueConvertersEndToEndFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
