using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class ChangeTrackingImpatientTest : ChangeTrackingTestBase<NorthwindQueryImpatientFixture>
    {
        public ChangeTrackingImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
