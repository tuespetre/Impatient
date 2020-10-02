using Microsoft.EntityFrameworkCore;

namespace Impatient.EFCore.Tests
{
    public class CommandInterceptionImpatientTest : CommandInterceptionTestBase
    {
        public CommandInterceptionImpatientTest(InterceptionFixtureBase fixture) : base(fixture)
        {
            fixture.ListLoggerFactory.Clear();
        }
    }
}
