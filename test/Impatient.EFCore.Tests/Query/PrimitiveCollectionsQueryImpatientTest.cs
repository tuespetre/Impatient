using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class PrimitiveCollectionsQueryImpatientTest(PrimitiveCollectionsQueryImpatientTest.Fixture fixture) 
    : PrimitiveCollectionsQueryRelationalTestBase<PrimitiveCollectionsQueryImpatientTest.Fixture>(fixture)
{
    public new class Fixture : PrimitiveCollectionsQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    [DisagreeWithEFCore]
    [Theory(Skip = Punt)]
    public override Task Column_collection_ElementAt(bool async)
    {
        return base.Column_collection_ElementAt(async);
    }

    [DisagreeWithEFCore]
    [Theory(Skip = Punt)]
    public override Task Column_collection_OrderByDescending_ElementAt(bool async)
    {
        return base.Column_collection_OrderByDescending_ElementAt(async);
    }

    public override Task Column_collection_of_bools_Contains(bool async)
    {
        return base.Column_collection_of_bools_Contains(async);
    }

    public override async Task Parameter_collection_of_strings_Contains_nullable_string(bool async)
    {
        //return base.Parameter_collection_of_strings_Contains_nullable_string(async);

        var strings = new[] { "10", "999" };

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => strings.Contains(c.NullableString)));

        // this is the problematic part of the test
        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => !strings.Contains(c.NullableString)));
    }
}
