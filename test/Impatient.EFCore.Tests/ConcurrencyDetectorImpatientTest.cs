using System.Threading.Tasks;
using Impatient.EFCore.Tests.Query;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class ConcurrencyDetectorImpatientTest : ConcurrencyDetectorRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public ConcurrencyDetectorImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override Task FromSql_logs_concurrent_access_async()
        {
            return base.FromSql_logs_concurrent_access_async();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override Task FromSql_logs_concurrent_access_nonasync()
        {
            return base.FromSql_logs_concurrent_access_nonasync();
        }
    }
}
