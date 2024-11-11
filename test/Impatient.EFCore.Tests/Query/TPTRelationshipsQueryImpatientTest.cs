using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class TPTRelationshipsQueryImpatientTest : TPTRelationshipsQueryTestBase<TPTRelationshipsQueryImpatientTest.Fixture>
{
    public TPTRelationshipsQueryImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    public new class Fixture : TPTRelationshipsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    // What is the goal here? To push down the derived include before falling back to client eval of the cast?
    [Theory(Skip = Punt)]
    public override async Task Include_on_derived_type_with_queryable_Cast(bool async)
    {
        await base.Include_on_derived_type_with_queryable_Cast(async);
    }

    [Theory(Skip = Punt)]
    public override Task Include_on_derived_type_with_queryable_Cast_split(bool async)
    {
        return base.Include_on_derived_type_with_queryable_Cast_split(async);
    }
}
