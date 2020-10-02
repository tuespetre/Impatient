using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class QueryNavigationsImpatientTest : NorthwindNavigationsQueryRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public QueryNavigationsImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
