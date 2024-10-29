using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query;

public class ComplexNavigationsCollectionsSplitQueryImpatientTest : ComplexNavigationsCollectionsSplitQueryRelationalTestBase<ComplexNavigationsCollectionsSplitQueryImpatientTest.Fixture>
{
    public ComplexNavigationsCollectionsSplitQueryImpatientTest(Fixture fixture) : base(fixture)
    {
    }

    /*protected override QueryAsserter CreateQueryAsserter(Fixture fixture) =>
        new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);*/

    public class Fixture : ComplexNavigationsQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}