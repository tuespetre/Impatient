using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindSplitIncludeQueryImpatientTest : NorthwindSplitIncludeQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public NorthwindSplitIncludeQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
