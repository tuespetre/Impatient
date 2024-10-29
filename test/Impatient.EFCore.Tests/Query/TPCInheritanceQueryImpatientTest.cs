using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Impatient.EFCore.Tests.Query;

public class TPCInheritanceQueryImpatientTest : TPCInheritanceQueryTestBase<TPCInheritanceQueryImpatientTest.Fixture>
{
    public TPCInheritanceQueryImpatientTest(Fixture fixture, ITestOutputHelper testOutputHelper) : base(fixture, testOutputHelper)
    {
    }

    [Theory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Can_query_all_animals(bool async)
    {
        return base.Can_query_all_animals(async);
    }

    public new class Fixture : TPCInheritanceQueryFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
