using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class ComplexNavigationsQueryImpatientTest : ComplexNavigationsQueryRelationalTestBase<ComplexNavigationsQueryImpatientTest.Fixture>
    {
        public ComplexNavigationsQueryImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        protected override QueryAsserter CreateQueryAsserter(Fixture fixture) =>
            new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

        public class Fixture : ComplexNavigationsQueryRelationalFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
