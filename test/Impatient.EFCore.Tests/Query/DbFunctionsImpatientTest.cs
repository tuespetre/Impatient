using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class DbFunctionsImpatientTest : DbFunctionsTestBase<NorthwindQueryImpatientFixture>
    {
        public DbFunctionsImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
