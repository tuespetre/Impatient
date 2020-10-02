using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class ComplexNavigationsWeakQueryImpatientTest : ComplexNavigationsWeakQueryTestBase<ComplexNavigationsWeakQueryImpatientTest.Fixture>
    {
        public ComplexNavigationsWeakQueryImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        protected override QueryAsserter CreateQueryAsserter(Fixture fixture) => 
            new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

        public class Fixture : ComplexNavigationsWeakQueryRelationalFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
