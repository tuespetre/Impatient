using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class IncludeImpatientTest : NorthwindIncludeQueryRelationalTestBase<NorthwindQueryImpatientFixture>
{
    public IncludeImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_with_multiple_ordering(bool async)
    {
        return base.Filtered_include_with_multiple_ordering(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Include_collection_with_client_filter(bool async)
    {
        return base.Include_collection_with_client_filter(async);
    }

    [Theory(Skip = TranslationBeyondEF)]
    public override Task Include_collection_with_last_no_orderby(bool async)
    {
        return base.Include_collection_with_last_no_orderby(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Multi_level_includes_are_applied_with_skip(bool async)
    {
        return base.Multi_level_includes_are_applied_with_skip(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Multi_level_includes_are_applied_with_skip_take(bool async)
    {
        return base.Multi_level_includes_are_applied_with_skip_take(async);
    }

    [Theory(Skip = LiftedInclude)]
    public override Task Multi_level_includes_are_applied_with_take(bool async)
    {
        return base.Multi_level_includes_are_applied_with_take(async);
    }
}
