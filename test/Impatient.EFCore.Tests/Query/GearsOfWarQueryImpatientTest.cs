using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class GearsOfWarQueryImpatientTest : GearsOfWarQueryRelationalTestBase<GearsOfWarQueryImpatientTest.Fixture>
    {
        public GearsOfWarQueryImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        protected override QueryAsserter CreateQueryAsserter(Fixture fixture) =>
            new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

        public class Fixture : GearsOfWarQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
