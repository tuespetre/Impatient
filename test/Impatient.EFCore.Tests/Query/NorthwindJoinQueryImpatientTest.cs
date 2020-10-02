using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindJoinQueryImpatientTest : NorthwindJoinQueryRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public NorthwindJoinQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
