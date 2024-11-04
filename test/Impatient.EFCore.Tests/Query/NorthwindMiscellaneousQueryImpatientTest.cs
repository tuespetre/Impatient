using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindMiscellaneousQueryImpatientTest : NorthwindMiscellaneousQueryRelationalTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindMiscellaneousQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
    }

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture) =>
        new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public override Task All_top_level(bool async)
    {
        return base.All_top_level(async);
    }
}
