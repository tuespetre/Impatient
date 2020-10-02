using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindMiscellaneousQueryImpatientTest : NorthwindMiscellaneousQueryRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public NorthwindMiscellaneousQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
