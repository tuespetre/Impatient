using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindCompiledQueryImpatientTest : NorthwindCompiledQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public NorthwindCompiledQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
