using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;

namespace Impatient.EFCore.Tests
{
    public class FindImpatientTest : FindTestBase<FindImpatientTest.Fixture>
    {
        public FindImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.ListLoggerFactory.Clear();
        }

        protected override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
        {
            return context.Find<TEntity>(keyValues);
        }

        protected override ValueTask<TEntity> FindAsync<TEntity>(DbContext context, params object[] keyValues)
        {
            return context.FindAsync<TEntity>(keyValues);
        }

        public class Fixture : FindFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
