using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindSetOperationsQueryImpatientTest : NorthwindSetOperationsQueryRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public NorthwindSetOperationsQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Theory]
        [MemberData(nameof(IsAsyncData))]
        public override Task OrderBy_Take_Union(bool async)
        {
            return base.OrderBy_Take_Union(async);
        }

        protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture) =>
            new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
    }
}
