using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class AsTrackingImpatientTest : 
        AsTrackingTestBase<NorthwindQueryImpatientFixture>,
        IClassFixture<NorthwindQueryImpatientFixture>
    {
        public AsTrackingImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
