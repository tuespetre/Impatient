using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class TransactionInterceptionImpatientTest : TransactionInterceptionTestBase
    {
        public TransactionInterceptionImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        public class Fixture : InterceptionFixtureBase
        {
            protected override bool ShouldSubscribeToDiagnosticListener => throw new System.NotImplementedException();

            protected override string StoreName => throw new System.NotImplementedException();

            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
