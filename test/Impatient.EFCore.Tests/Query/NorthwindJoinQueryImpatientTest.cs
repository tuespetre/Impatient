using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindJoinQueryImpatientTest : NorthwindJoinQueryRelationalTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindJoinQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
    }

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture) =>
        new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    [Theory(Skip = ClientEval)]
    public override Task Client_Join_select_many(bool async)
    {
        return base.Client_Join_select_many(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task SelectMany_with_client_eval_with_collection_shaper(bool async)
    {
        return base.SelectMany_with_client_eval_with_collection_shaper(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task SelectMany_with_client_eval_with_collection_shaper_ignored(bool async)
    {
        return base.SelectMany_with_client_eval_with_collection_shaper_ignored(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task SelectMany_with_client_eval_with_constructor(bool async)
    {
        return base.SelectMany_with_client_eval_with_constructor(async);
    }
}
