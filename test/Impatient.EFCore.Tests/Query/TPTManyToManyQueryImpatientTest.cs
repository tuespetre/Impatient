using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable xUnit1003 // Theory methods must have test data
namespace Impatient.EFCore.Tests.Query
{
    public class TPTManyToManyQueryImpatientTest : TPTManyToManyQueryRelationalTestBase<TPTManyToManyQueryImpatientTest.Fixture>
    {
        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_on_navigation_then_filtered_include_on_skip_navigation(bool async)
        {
            return base.Filtered_include_on_navigation_then_filtered_include_on_skip_navigation(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_on_navigation_then_filtered_include_on_skip_navigation_split(bool async)
        {
            return base.Filtered_include_on_navigation_then_filtered_include_on_skip_navigation_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_on_skip_navigation_then_filtered_include_on_navigation(bool async)
        {
            return base.Filtered_include_on_skip_navigation_then_filtered_include_on_navigation(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_on_skip_navigation_then_filtered_include_on_navigation_split(bool async)
        {
            return base.Filtered_include_on_skip_navigation_then_filtered_include_on_navigation_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_skip_navigation_order_by(bool async)
        {
            return base.Filtered_include_skip_navigation_order_by(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_skip_navigation_order_by_skip(bool async)
        {
            return base.Filtered_include_skip_navigation_order_by_skip(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_skip_navigation_order_by_skip_split(bool async)
        {
            return base.Filtered_include_skip_navigation_order_by_skip_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_skip_navigation_order_by_skip_take(bool async)
        {
            return base.Filtered_include_skip_navigation_order_by_skip_take(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_skip_navigation_order_by_skip_take_split(bool async)
        {
            return base.Filtered_include_skip_navigation_order_by_skip_take_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(bool async)
        {
            return base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_split(bool async)
        {
            return base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_skip_navigation_order_by_take(bool async)
        {
            return base.Filtered_include_skip_navigation_order_by_take(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_skip_navigation_order_by_split(bool async)
        {
            return base.Filtered_include_skip_navigation_order_by_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_skip_navigation_order_by_take_split(bool async)
        {
            return base.Filtered_include_skip_navigation_order_by_take_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_skip_navigation_where(bool async)
        {
            return base.Filtered_include_skip_navigation_where(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_skip_navigation_where_split(bool async)
        {
            return base.Filtered_include_skip_navigation_where_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_skip_navigation_where_then_include_skip_navigation(bool async)
        {
            return base.Filtered_include_skip_navigation_where_then_include_skip_navigation(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take(bool async)
        {
            return base.Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take_split(bool async)
        {
            return base.Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_skip_navigation_where_then_include_skip_navigation_split(bool async)
        {
            return base.Filtered_include_skip_navigation_where_then_include_skip_navigation_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_then_include_skip_navigation_order_by_skip_take(bool async)
        {
            return base.Filtered_then_include_skip_navigation_order_by_skip_take(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_then_include_skip_navigation_order_by_skip_take_split(bool async)
        {
            return base.Filtered_then_include_skip_navigation_order_by_skip_take_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_then_include_skip_navigation_where(bool async)
        {
            return base.Filtered_then_include_skip_navigation_where(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_then_include_skip_navigation_where_split(bool async)
        {
            return base.Filtered_then_include_skip_navigation_where_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filter_include_on_skip_navigation_combined_with_filtered_then_includes(bool async)
        {
            return base.Filter_include_on_skip_navigation_combined_with_filtered_then_includes(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filter_include_on_skip_navigation_combined_with_filtered_then_includes_split(bool async)
        {
            return base.Filter_include_on_skip_navigation_combined_with_filtered_then_includes_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filter_include_on_skip_navigation_combined(bool async)
        {
            return base.Filter_include_on_skip_navigation_combined(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filter_include_on_skip_navigation_combined_split(bool async)
        {
            return base.Filter_include_on_skip_navigation_combined_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Throws_when_different_filtered_include(bool async)
        {
            return base.Throws_when_different_filtered_include(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Throws_when_different_filtered_then_include(bool async)
        {
            return base.Throws_when_different_filtered_then_include(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Throws_when_different_filtered_then_include_via_different_paths(bool async)
        {
            return base.Throws_when_different_filtered_then_include_via_different_paths(async);
        }

        public TPTManyToManyQueryImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }

        public class Fixture : TPTManyToManyQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
#pragma warning restore xUnit1003 // Theory methods must have test data
