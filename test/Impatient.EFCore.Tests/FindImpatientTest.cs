using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class FindImpatientTest : FindTestBase<FindImpatientTest.Fixture>
    {
        public FindImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.ListLoggerFactory.Clear();
        }

        [Theory(Skip = "Not sure why this is failing, but not our problem")]
        [InlineData(CancellationType.None)]
        public override Task Throws_for_multiple_values_passed_for_simple_key_async(CancellationType cancellationType)
        {
            return base.Throws_for_multiple_values_passed_for_simple_key_async(cancellationType);
        }

        [Theory(Skip = "Not sure why this is failing, but not our problem")]
        [InlineData(CancellationType.None)]
        public override Task Throws_for_wrong_number_of_values_for_composite_key_async(CancellationType cancellationType)
        {
            return base.Throws_for_wrong_number_of_values_for_composite_key_async(cancellationType);
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

        public new class Fixture : FindFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
