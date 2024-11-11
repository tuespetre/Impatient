using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindSelectQueryImpatientTest : NorthwindSelectQueryRelationalTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindSelectQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    // We compute a default ordering.
    [Theory(Skip = TranslationBeyondEF)]
    public override Task Reverse_without_explicit_ordering(bool async)
    {
        return base.Reverse_without_explicit_ordering(async);
    }

    // It translates just fine.
    [Theory(Skip = TranslationBeyondEF)]
    public override Task Select_bool_closure_with_order_by_property_with_cast_to_nullable(bool async)
    {
        return base.Select_bool_closure_with_order_by_property_with_cast_to_nullable(async);
    }

    // Really not sure what the problem is in this one, we translate and execute it just fine,
    // the materializer happens in the client but it's not like it's a client predicate, so...
    [Theory(Skip = TranslationBeyondEF)]
    public override Task VisitLambda_should_not_be_visited_trivially(bool async)
    {
        return base.VisitLambda_should_not_be_visited_trivially(async);
    }
}
