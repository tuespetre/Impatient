using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading;
using System.Threading.Tasks;

namespace Impatient.EFCore.Tests
{
    public class FindImpatientTest : FindTestBase<FindImpatientTest.Fixture>
    {
        public FindImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.ListLoggerFactory.Clear();
        }

        protected override TestFinder Finder => new TestFinder2();

        public class TestFinder2 : TestFinder
        {
            public override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
            {
                return context.Find<TEntity>(keyValues);
            }

            public override ValueTask<TEntity> FindAsync<TEntity>(CancellationType cancellationType, DbContext context, object[] keyValues, CancellationToken cancellationToken = default)
            {
                return context.FindAsync<TEntity>(keyValues, cancellationToken);
            }
        }

        public class Fixture : FindFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
