using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindAggregateOperatorsQueryImpatientTest : NorthwindAggregateOperatorsQueryRelationalTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindAggregateOperatorsQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
    }

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture) =>
        new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    [Theory(Skip = ProjectionSplitting)]
    public override Task Average_over_max_subquery_is_client_eval(bool async)
    {
        return base.Average_over_max_subquery_is_client_eval(async);
    }

    [Theory(Skip = ProjectionSplitting)]
    public override Task Average_over_nested_subquery_is_client_eval(bool async)
    {
        return base.Average_over_nested_subquery_is_client_eval(async);
    }

    [Theory(Skip = FromSql)]
    public override Task Contains_over_keyless_entity_throws(bool async)
    {
        return base.Contains_over_keyless_entity_throws(async);
    }

    [Theory(Skip = "Impatient supports Last without an ordering")]
    public override Task Last_when_no_order_by(bool async)
    {
        return base.Last_when_no_order_by(async);
    }

    [Theory(Skip = "Impatient supports LastOrDefault without an ordering")]
    public override Task LastOrDefault_when_no_order_by(bool async)
    {
        return base.LastOrDefault_when_no_order_by(async);
    }

    [Theory(Skip = ProjectionSplitting)]
    public override Task Max_over_nested_subquery_is_client_eval(bool async)
    {
        return base.Max_over_nested_subquery_is_client_eval(async);
    }

    [Theory(Skip = ProjectionSplitting)]
    public override Task Max_over_sum_subquery_is_client_eval(bool async)
    {
        return base.Max_over_sum_subquery_is_client_eval(async);
    }

    [Theory(Skip = ProjectionSplitting)]
    public override Task Min_over_max_subquery_is_client_eval(bool async)
    {
        return base.Min_over_max_subquery_is_client_eval(async);
    }

    [Theory(Skip = ProjectionSplitting)]
    public override Task Min_over_nested_subquery_is_client_eval(bool async)
    {
        return base.Min_over_nested_subquery_is_client_eval(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task OrderBy_Count_with_predicate_client_eval(bool async)
    {
        return base.OrderBy_Count_with_predicate_client_eval(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task OrderBy_Count_with_predicate_client_eval_mixed(bool async)
    {
        return base.OrderBy_Count_with_predicate_client_eval_mixed(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task OrderBy_Where_Count_client_eval(bool async)
    {
        return base.OrderBy_Where_Count_client_eval(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task OrderBy_Where_Count_client_eval_mixed(bool async)
    {
        return base.OrderBy_Where_Count_client_eval_mixed(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task OrderBy_Where_Count_with_predicate_client_eval(bool async)
    {
        return base.OrderBy_Where_Count_with_predicate_client_eval(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task OrderBy_Where_Count_with_predicate_client_eval_mixed(bool async)
    {
        return base.OrderBy_Where_Count_with_predicate_client_eval_mixed(async);
    }

    [Theory(Skip = ProjectionSplitting)]
    public override Task Sum_over_min_subquery_is_client_eval(bool async)
    {
        return base.Sum_over_min_subquery_is_client_eval(async);
    }

    [Theory(Skip = ProjectionSplitting)]
    public override Task Sum_over_nested_subquery_is_client_eval(bool async)
    {
        return base.Sum_over_nested_subquery_is_client_eval(async);
    }

    [Theory(Skip = ClientEval)]
    public override Task Where_OrderBy_Count_client_eval(bool async)
    {
        return base.Where_OrderBy_Count_client_eval(async);
    }
}
