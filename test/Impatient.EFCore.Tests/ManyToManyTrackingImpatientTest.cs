using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests;

public class ManyToManyTrackingImpatientTest : ManyToManyTrackingRelationalTestBase<ManyToManyTrackingImpatientTest.Fixture>
{
    public ManyToManyTrackingImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.ListLoggerFactory.Clear();
    }

    public new class Fixture : ManyToManyTrackingRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
