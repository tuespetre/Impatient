using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests;

public class ManyToManyFieldsLoadImpatientTest : ManyToManyFieldsLoadTestBase<ManyToManyFieldsLoadImpatientTest.Fixture>
{
    public ManyToManyFieldsLoadImpatientTest(Fixture fixture) : base(fixture)
    {
    }

    public new class Fixture : ManyToManyFieldsLoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
