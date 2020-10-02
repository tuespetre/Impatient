using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindWhereQueryImpatientTest : NorthwindWhereQueryRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public NorthwindWhereQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
