using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class OwnedQueryImpatientTest : OwnedQueryRelationalTestBase<OwnedQueryImpatientTest.Fixture>
{
    public OwnedQueryImpatientTest(Fixture fixture) : base(fixture)
    {
    }

    [ConditionalTheory(Skip = FromSql)]
    [MemberData(nameof(IsAsyncData))]
    public override Task Using_from_sql_on_owner_generates_join_with_table_for_owned_shared_dependents(bool async)
    {
        return base.Using_from_sql_on_owner_generates_join_with_table_for_owned_shared_dependents(async);
    }

    protected override QueryAsserter CreateQueryAsserter(Fixture fixture) =>
        new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : RelationalOwnedQueryFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
