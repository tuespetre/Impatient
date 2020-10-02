using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindStringIncludeQueryImpatientTest : NorthwindStringIncludeQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public NorthwindStringIncludeQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
