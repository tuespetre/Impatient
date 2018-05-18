using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class AsyncQueryNavigationsImpatientTest : AsyncQueryNavigationsTestBase<NorthwindQueryImpatientFixture>
    {
        public AsyncQueryNavigationsImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
