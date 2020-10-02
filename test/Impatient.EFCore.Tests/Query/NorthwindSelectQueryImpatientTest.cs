using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindSelectQueryImpatientTest : NorthwindSelectQueryRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public NorthwindSelectQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
