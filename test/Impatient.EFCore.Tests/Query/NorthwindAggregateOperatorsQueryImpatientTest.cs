using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindAggregateOperatorsQueryImpatientTest : NorthwindAggregateOperatorsQueryRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public NorthwindAggregateOperatorsQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
