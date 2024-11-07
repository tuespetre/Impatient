using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;

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

    public override Task Load_collection(EntityState state, QueryTrackingBehavior queryTrackingBehavior, bool async)
    {
        return base.Load_collection(state, queryTrackingBehavior, async);
    }
}
