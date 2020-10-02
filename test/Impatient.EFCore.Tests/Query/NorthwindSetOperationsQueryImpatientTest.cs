using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindSetOperationsQueryImpatientTest : NorthwindSetOperationsQueryRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public NorthwindSetOperationsQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
