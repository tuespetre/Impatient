using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

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

    [Theory(Skip = SpecialIncludes)]
    public override Task Load_collection_using_Query_with_filtered_Include(bool async)
    {
        return base.Load_collection_using_Query_with_filtered_Include(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Load_collection_using_Query_with_filtered_Include_and_projection(bool async)
    {
        return base.Load_collection_using_Query_with_filtered_Include_and_projection(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Query_with_filtered_Include_marks_only_left_as_loaded(bool async)
    {
        return base.Query_with_filtered_Include_marks_only_left_as_loaded(async);
    }
}
