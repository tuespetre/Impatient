using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindSplitIncludeNoTrackingQueryImpatientTest : NorthwindSplitIncludeNoTrackingQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public NorthwindSplitIncludeNoTrackingQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
