using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class AsNoTrackingImpatientTest : AsNoTrackingTestBase<NorthwindQueryImpatientFixture>
    {
        public AsNoTrackingImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
