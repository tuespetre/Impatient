using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests;

public class SaveChangesInterceptionImpatientTest : SaveChangesInterceptionTestBase
{
    public SaveChangesInterceptionImpatientTest(InterceptionFixtureBase fixture) : base(fixture)
    {
    }

    public class Fixture : InterceptionFixtureBase
    {
        protected override bool ShouldSubscribeToDiagnosticListener => false;

        protected override string StoreName => nameof(SaveChangesInterceptionImpatientTest);

        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
