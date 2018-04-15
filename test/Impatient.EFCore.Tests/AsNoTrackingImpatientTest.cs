using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class AsNoTrackingImpatientTest : 
        AsNoTrackingTestBase<NorthwindQueryImpatientFixture>,
        IClassFixture<NorthwindQueryImpatientFixture>
    {
        public AsNoTrackingImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
