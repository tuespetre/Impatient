using Impatient.EFCore.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;
using static Impatient.EFCore.Tests.Query.FiltersImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class FiltersImpatientTest : NorthwindQueryFiltersQueryTestBase<Fixture>
{
    public FiltersImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    public new class Fixture : NorthwindQueryImpatientFixtureBase<NorthwindQueryFiltersCustomizer>
    {
    }

    [Theory(Skip = ClientEval)]
    public override Task Client_eval(bool async)
    {
        return base.Client_eval(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Included_one_to_many_query_with_client_eval(bool async)
    {
        return base.Included_one_to_many_query_with_client_eval(async);
    }
}
