using Microsoft.EntityFrameworkCore;

namespace Impatient.EFCore.Tests
{
    public class LazyLoadProxyImpatientTest : LazyLoadProxyTestBase<LazyLoadProxyImpatientFixture>
    {
        public LazyLoadProxyImpatientTest(LazyLoadProxyImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
