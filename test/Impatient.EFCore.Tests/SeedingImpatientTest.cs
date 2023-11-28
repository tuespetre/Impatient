using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class SeedingImpatientTest : SeedingTestBase
    {
        protected override TestStore TestStore => throw new System.NotImplementedException();

        protected override SeedingContext CreateContextWithEmptyDatabase(string testId)
        {
            throw new System.NotImplementedException();
        }
    }
}
