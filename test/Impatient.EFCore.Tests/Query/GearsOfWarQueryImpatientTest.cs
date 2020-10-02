using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class GearsOfWarQueryImpatientTest : GearsOfWarQueryRelationalTestBase<GearsOfWarQueryImpatientFixture>
    {
        public GearsOfWarQueryImpatientTest(GearsOfWarQueryImpatientFixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
