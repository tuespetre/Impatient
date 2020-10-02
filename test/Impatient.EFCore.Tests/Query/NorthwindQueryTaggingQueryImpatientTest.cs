using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindQueryTaggingQueryImpatientTest : NorthwindQueryTaggingQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public NorthwindQueryTaggingQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
