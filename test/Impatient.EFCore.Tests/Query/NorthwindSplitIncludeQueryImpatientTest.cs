using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindSplitIncludeQueryImpatientTest : NorthwindSplitIncludeQueryTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindSplitIncludeQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Include_collection_with_client_filter(bool async)
    {
        return base.Include_collection_with_client_filter(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Include_collection_with_last_no_orderby(bool async)
    {
        return base.Include_collection_with_last_no_orderby(async);
    }
}
