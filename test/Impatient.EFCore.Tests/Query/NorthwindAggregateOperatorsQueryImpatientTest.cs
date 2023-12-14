using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindAggregateOperatorsQueryImpatientTest : NorthwindAggregateOperatorsQueryRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public NorthwindAggregateOperatorsQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Theory(Skip = "Impatient supports Last without an ordering")]
        [MemberData(nameof(IsAsyncData))]
        public override Task Last_when_no_order_by(bool async)
        {
            return base.Last_when_no_order_by(async);
        }

        [Theory(Skip = "Impatient supports LastOrDefault without an ordering")]
        [MemberData(nameof(IsAsyncData))]
        public override Task LastOrDefault_when_no_order_by(bool async)
        {
            return base.LastOrDefault_when_no_order_by(async);
        }

        protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture) =>
            new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
    }
}
