using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests
{
    public class DbFunctionsImpatientTest : DbFunctionsTestBase<NorthwindQueryImpatientFixture>
    {
        public DbFunctionsImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
