using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests;

public class ConcurrencyDetectorDisabledImpatientTest : ConcurrencyDetectorDisabledRelationalTestBase<ConcurrencyDetectorDisabledImpatientTest.Fixture>
{
    public ConcurrencyDetectorDisabledImpatientTest(Fixture fixture) : base(fixture)
    {
    }

    [ConditionalTheory(Skip = EFCoreSkipReasons.FromSql)]
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
