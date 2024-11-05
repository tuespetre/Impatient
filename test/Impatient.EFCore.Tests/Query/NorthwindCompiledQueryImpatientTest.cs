using Impatient.EFCore.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Query;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindCompiledQueryImpatientTest : NorthwindCompiledQueryTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindCompiledQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
    }

    [Fact(Skip = EFCoreSkipReasons.FromSql)]
    public override void Keyless_query()
    {
        base.Keyless_query();
    }

    [Fact(Skip = EFCoreSkipReasons.FromSql)]
    public override Task Keyless_query_async()
    {
        return base.Keyless_query_async();
    }

    [Fact(Skip = EFCoreSkipReasons.FromSql)]
    public override void Keyless_query_first()
    {
        base.Keyless_query_first();
    }

    [Fact(Skip = EFCoreSkipReasons.FromSql)]
    public override Task Keyless_query_first_async()
    {
        return base.Keyless_query_first_async();
    }
}
