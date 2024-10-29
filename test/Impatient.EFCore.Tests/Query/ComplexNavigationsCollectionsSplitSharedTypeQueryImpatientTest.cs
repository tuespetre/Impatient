using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query;

public class ComplexNavigationsCollectionsSplitSharedTypeQueryImpatientTest : ComplexNavigationsCollectionsSplitSharedTypeQueryRelationalTestBase<ComplexNavigationsCollectionsSplitSharedTypeQueryImpatientTest.Fixture>
{
    public ComplexNavigationsCollectionsSplitSharedTypeQueryImpatientTest(Fixture fixture) : base(fixture)
    {
    }

    /*protected override QueryAsserter CreateQueryAsserter(Fixture fixture) =>
        new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);*/

    public class Fixture : ComplexNavigationsSharedTypeQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}