using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class IncludeImpatientTest : NorthwindIncludeQueryRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public IncludeImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
