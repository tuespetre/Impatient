using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class GearsOfWarQueryImpatientTest : GearsOfWarQueryRelationalTestBase<GearsOfWarQueryImpatientTest.Fixture>
{
    public GearsOfWarQueryImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task GroupBy_Select_sum(bool async)
    {
        return base.GroupBy_Select_sum(async);
    }

    protected override QueryAsserter CreateQueryAsserter(Fixture fixture) =>
        new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : GearsOfWarQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
