using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class AsTrackingImpatientTest : NorthwindAsTrackingQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public AsTrackingImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
