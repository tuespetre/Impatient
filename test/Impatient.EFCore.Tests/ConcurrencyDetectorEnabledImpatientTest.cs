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

    public new class Fixture : ConcurrencyDetectorFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    [Theory(Skip = EFCoreSkipReasons.Punt)]
    public override Task Any(bool async)
    {
        return base.Any(async);
    }

    [Theory(Skip = EFCoreSkipReasons.Punt)]
    public override Task Count(bool async)
    {
        return base.Count(async);
    }

    [Theory(Skip = EFCoreSkipReasons.Punt)]
    public override Task Find(bool async)
    {
        return base.Find(async);
    }

    [Theory(Skip = EFCoreSkipReasons.Punt)]
    public override Task First(bool async)
    {
        return base.First(async);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task FromSql(bool async)
    {
        return base.FromSql(async);
    }

    [Theory(Skip = EFCoreSkipReasons.Punt)]
    public override Task Last(bool async)
    {
        return base.Last(async);
    }

    [Theory(Skip = EFCoreSkipReasons.Punt)]
    public override Task Single(bool async)
    {
        return base.Single(async);
    }

    [Theory(Skip = EFCoreSkipReasons.Punt)]
    public override Task ToList(bool async)
    {
        return base.ToList(async);
    }
}
