using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class GearsOfWarQueryImpatientTest : GearsOfWarQueryRelationalTestBase<GearsOfWarQueryImpatientTest.Fixture>
    {
        public GearsOfWarQueryImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        [ConditionalTheory]
        [MemberData(nameof(IsAsyncData))]
        public override Task Where_DateOnly_DayOfWeek(bool async)
        {
            return base.Where_DateOnly_DayOfWeek(async);
        }

        protected override QueryAsserter CreateQueryAsserter(Fixture fixture) =>
            new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

        public class Fixture : GearsOfWarQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
