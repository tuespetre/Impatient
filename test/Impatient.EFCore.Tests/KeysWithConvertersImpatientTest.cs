using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests;

public class KeysWithConvertersImpatientTest(KeysWithConvertersImpatientTest.KeysWithConvertersImpatientFixture fixture) 
    : KeysWithConvertersTestBase<KeysWithConvertersImpatientTest.KeysWithConvertersImpatientFixture>(fixture)
{
    public class KeysWithConvertersImpatientFixture : KeysWithConvertersFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => builder.UseSqlServer(b => b.MinBatchSize(1));
    }
}
