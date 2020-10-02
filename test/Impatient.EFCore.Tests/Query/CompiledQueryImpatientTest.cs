using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    internal class CompiledQueryImpatientTest : NorthwindCompiledQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public CompiledQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
