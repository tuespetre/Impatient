using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class SimpleQueryImpatientTest : NorthwindAsyncSimpleQueryRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public SimpleQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
