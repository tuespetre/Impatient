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

    [Theory(Skip = FromSql)]
    public override Task Contains_over_keyless_entity_throws(bool async)
    {
        return base.Contains_over_keyless_entity_throws(async);
    }

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture) =>
        new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
}
