using Microsoft.EntityFrameworkCore;
using System;

namespace Impatient.EFCore.Tests
{
    public class ConnectionInterceptionImpatientTest : ConnectionInterceptionTestBase
    {
        public ConnectionInterceptionImpatientTest(InterceptionFixtureBase fixture) : base(fixture)
        {
            fixture.ListLoggerFactory.Clear();
        }

        protected override BadUniverseContext CreateBadUniverse(DbContextOptionsBuilder optionsBuilder)
        {
            throw new NotImplementedException();
        }
    }
}
