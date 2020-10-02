using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class AsNoTrackingImpatientTest : NorthwindAsNoTrackingQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public AsNoTrackingImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
