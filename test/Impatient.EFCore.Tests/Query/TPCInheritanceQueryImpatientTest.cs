using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Impatient.EFCore.Tests.Query;

public class TPCInheritanceQueryImpatientTest : TPCInheritanceQueryTestBase<TPCInheritanceQueryImpatientTest.Fixture>
{
    public TPCInheritanceQueryImpatientTest(Fixture fixture, ITestOutputHelper testOutputHelper) : base(fixture, testOutputHelper)
    {
    }

    public new class Fixture : TPCInheritanceQueryFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    public override Task Using_OfType_on_multiple_type_with_no_result(bool async)
    {
        // TODO: allow this type of query, but just use an 'empty query expression' to avoid hitting the db.
        return base.Using_OfType_on_multiple_type_with_no_result(async);
    }
}
