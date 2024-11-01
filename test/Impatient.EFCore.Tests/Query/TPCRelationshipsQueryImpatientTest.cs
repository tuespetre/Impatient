using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.InheritanceRelationshipsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;

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

    public override Task Include_collection_with_inheritance(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<BaseInheritanceRelationshipEntity>().Include(e => e.BaseCollectionOnBase),
            elementAsserter: (e, a) => QueryAsserter.AssertInclude(
                e, a,
                [/*new ExpectedInclude<BaseInheritanceRelationshipEntity>(x => x.BaseCollectionOnBase)*/]));
}
