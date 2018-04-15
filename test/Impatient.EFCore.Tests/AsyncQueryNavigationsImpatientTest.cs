using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests
{
    public class AsyncQueryNavigationsImpatientTest : AsyncQueryNavigationsTestBase<NorthwindQueryImpatientFixture>
    {
        public AsyncQueryNavigationsImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
