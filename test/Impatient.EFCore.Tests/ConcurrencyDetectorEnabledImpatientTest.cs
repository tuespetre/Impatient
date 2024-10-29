using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests;

public class ConcurrencyDetectorEnabledImpatientTest : ConcurrencyDetectorEnabledRelationalTestBase<ConcurrencyDetectorEnabledImpatientTest.Fixture>
{
    public ConcurrencyDetectorEnabledImpatientTest(Fixture fixture) : base(fixture)
    {
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    [MemberData(nameof(IsAsyncData))]
    public override Task FromSql(bool async)
    {
        return base.FromSql(async);
    }

    public new class Fixture : ConcurrencyDetectorFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
