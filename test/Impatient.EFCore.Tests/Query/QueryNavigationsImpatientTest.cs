using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class QueryNavigationsImpatientTest : NorthwindNavigationsQueryRelationalTestBase<NorthwindQueryImpatientFixture>
{
    public QueryNavigationsImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Collection_select_nav_prop_all_client(bool async)
    {
        return base.Collection_select_nav_prop_all_client(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Select_Where_Navigation_Client(bool async)
    {
        return base.Select_Where_Navigation_Client(async);
    }
}
