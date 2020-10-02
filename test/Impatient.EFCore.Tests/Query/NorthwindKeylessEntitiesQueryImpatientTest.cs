using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindKeylessEntitiesQueryImpatientTest : NorthwindKeylessEntitiesQueryRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public NorthwindKeylessEntitiesQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
