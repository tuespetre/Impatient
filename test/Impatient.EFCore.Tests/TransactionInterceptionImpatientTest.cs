using Microsoft.EntityFrameworkCore;

namespace Impatient.EFCore.Tests
{
    public class TransactionInterceptionImpatientTest : TransactionInterceptionTestBase
    {
        public TransactionInterceptionImpatientTest(InterceptionFixtureBase fixture) : base(fixture)
        {
            fixture.ListLoggerFactory.Clear();
        }
    }
}
