using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class CompiledQueryImpatientTest : CompiledQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public CompiledQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
