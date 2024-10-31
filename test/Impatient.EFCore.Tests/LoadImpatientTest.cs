using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using static Impatient.EFCore.Tests.LoadImpatientTest;

namespace Impatient.EFCore.Tests;

public class LoadImpatientTest : LoadTestBase<LoadImpatientFixture>
{
    public LoadImpatientTest(LoadImpatientFixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    public class LoadImpatientFixture : LoadFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;
    }
}
