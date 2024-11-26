using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindWhereQueryImpatientTest : NorthwindWhereQueryRelationalTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindWhereQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    [DisagreeWithEFCore]
    [Theory(Skip = Punt)]
    public override Task ElementAt_over_custom_projection_compared_to_not_null(bool async)
    {
        return base.ElementAt_over_custom_projection_compared_to_not_null(async);
    }

    // just added this override because the base test only uses InlineData(false) for some reason,
    // and Test Explorer saved playlist would then not show it for some reason?
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_bitwise_xor(bool async)
    {
        return base.Where_bitwise_xor(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Where_bool_client_side_negated(bool async)
    {
        return base.Where_bool_client_side_negated(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Where_client(bool async)
    {
        return base.Where_client(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Where_client_and_server_non_top_level(bool async)
    {
        return base.Where_client_and_server_non_top_level(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Where_client_and_server_top_level(bool async)
    {
        return base.Where_client_and_server_top_level(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Where_client_deep_inside_predicate_and_server_top_level(bool async)
    {
        return base.Where_client_deep_inside_predicate_and_server_top_level(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Where_client_or_server_top_level(bool async)
    {
        return base.Where_client_or_server_top_level(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Where_equals_method_string_with_ignore_case(bool async)
    {
        return base.Where_equals_method_string_with_ignore_case(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Where_subquery_correlated_client_eval(bool async)
    {
        return base.Where_subquery_correlated_client_eval(async);
    }
}
