using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindFunctionsQueryImpatientTest : NorthwindFunctionsQueryRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public NorthwindFunctionsQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
