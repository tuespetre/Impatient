using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query;

public class TPCRelationshipsQueryImpatientTest : TPCRelationshipsQueryTestBase<TPCRelationshipsQueryImpatientTest.Fixture>
{
    public TPCRelationshipsQueryImpatientTest(Fixture fixture) : base(fixture)
    {
    }

    public new class Fixture : TPCRelationshipsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
