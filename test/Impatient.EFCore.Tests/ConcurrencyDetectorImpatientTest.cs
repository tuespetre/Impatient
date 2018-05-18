using Impatient.EFCore.Tests.Query;
using Microsoft.EntityFrameworkCore;

namespace Impatient.EFCore.Tests
{
    public class ConcurrencyDetectorImpatientTest : ConcurrencyDetectorTestBase<NorthwindQueryImpatientFixture>
    {
        public ConcurrencyDetectorImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
