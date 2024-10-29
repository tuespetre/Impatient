using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests;

public class ManyToManyLoadImpatientTest : ManyToManyLoadTestBase<ManyToManyLoadImpatientTest.Fixture>
{
    public ManyToManyLoadImpatientTest(Fixture fixture) : base(fixture)
    {
    }

    public class Fixture : ManyToManyLoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
