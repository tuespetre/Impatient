using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindSetOperationsQueryImpatientTest : NorthwindSetOperationsQueryRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public NorthwindSetOperationsQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture) =>
            new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
    }
}
