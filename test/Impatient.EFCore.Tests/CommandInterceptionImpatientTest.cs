using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class CommandInterceptionImpatientTest : CommandInterceptionTestBase
    {
        public CommandInterceptionImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.ListLoggerFactory.Clear();
        }

        public new class Fixture : InterceptionFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener => false;

            protected override string StoreName => nameof(CommandInterceptionImpatientTest);

            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
