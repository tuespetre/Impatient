using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query;

public class CompositeKeysQueryImpatientTest(CompositeKeysQueryImpatientTest.Fixture fixture) : CompositeKeysQueryRelationalTestBase<CompositeKeysQueryImpatientTest.Fixture>(fixture)
{
    protected override QueryAsserter CreateQueryAsserter(Fixture fixture) =>
        new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : CompositeKeysQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
