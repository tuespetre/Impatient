using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using static Impatient.EFCore.Tests.Query.TPTManyToManyNoTrackingQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class TPTManyToManyNoTrackingQueryImpatientTest : TPTManyToManyNoTrackingQueryRelationalTestBase<Fixture>
{
    public TPTManyToManyNoTrackingQueryImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    protected override QueryAsserter CreateQueryAsserter(Fixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : TPTManyToManyQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_on_navigation_then_filtered_include_on_skip_navigation(bool async)
    {
        return base.Filtered_include_on_navigation_then_filtered_include_on_skip_navigation(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_on_navigation_then_filtered_include_on_skip_navigation_split(bool async)
    {
        return base.Filtered_include_on_navigation_then_filtered_include_on_skip_navigation_split(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_on_skip_navigation_then_filtered_include_on_navigation(bool async)
    {
        return base.Filtered_include_on_skip_navigation_then_filtered_include_on_navigation(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_on_skip_navigation_then_filtered_include_on_navigation_split(bool async)
    {
        return base.Filtered_include_on_skip_navigation_then_filtered_include_on_navigation_split(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_order_by(bool async)
    {
        return base.Filtered_include_skip_navigation_order_by(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_order_by_skip(bool async)
    {
        return base.Filtered_include_skip_navigation_order_by_skip(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_order_by_skip_split(bool async)
    {
        return base.Filtered_include_skip_navigation_order_by_skip_split(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_order_by_skip_take(bool async)
    {
        return base.Filtered_include_skip_navigation_order_by_skip_take(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_order_by_skip_take_split(bool async)
    {
        return base.Filtered_include_skip_navigation_order_by_skip_take_split(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(bool async)
    {
        return base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_EF_Property(bool async)
    {
        return base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_EF_Property(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_split(bool async)
    {
        return base.Filtered_include_skip_navigation_order_by_skip_take_then_include_skip_navigation_where_split(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_order_by_skip_take_unidirectional(bool async)
    {
        return base.Filtered_include_skip_navigation_order_by_skip_take_unidirectional(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_order_by_skip_unidirectional(bool async)
    {
        return base.Filtered_include_skip_navigation_order_by_skip_unidirectional(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_order_by_split(bool async)
    {
        return base.Filtered_include_skip_navigation_order_by_split(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_order_by_take(bool async)
    {
        return base.Filtered_include_skip_navigation_order_by_take(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_order_by_take_EF_Property(bool async)
    {
        return base.Filtered_include_skip_navigation_order_by_take_EF_Property(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_order_by_take_split(bool async)
    {
        return base.Filtered_include_skip_navigation_order_by_take_split(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_order_by_take_unidirectional(bool async)
    {
        return base.Filtered_include_skip_navigation_order_by_take_unidirectional(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_order_by_unidirectional(bool async)
    {
        return base.Filtered_include_skip_navigation_order_by_unidirectional(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_where(bool async)
    {
        return base.Filtered_include_skip_navigation_where(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_where_split(bool async)
    {
        return base.Filtered_include_skip_navigation_where_split(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_where_then_include_skip_navigation(bool async)
    {
        return base.Filtered_include_skip_navigation_where_then_include_skip_navigation(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take(bool async)
    {
        return base.Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take_split(bool async)
    {
        return base.Filtered_include_skip_navigation_where_then_include_skip_navigation_order_by_skip_take_split(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_where_then_include_skip_navigation_split(bool async)
    {
        return base.Filtered_include_skip_navigation_where_then_include_skip_navigation_split(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_where_then_include_skip_navigation_unidirectional(bool async)
    {
        return base.Filtered_include_skip_navigation_where_then_include_skip_navigation_unidirectional(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_skip_navigation_where_unidirectional(bool async)
    {
        return base.Filtered_include_skip_navigation_where_unidirectional(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_then_include_skip_navigation_order_by_skip_take(bool async)
    {
        return base.Filtered_then_include_skip_navigation_order_by_skip_take(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_then_include_skip_navigation_order_by_skip_take_split(bool async)
    {
        return base.Filtered_then_include_skip_navigation_order_by_skip_take_split(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_then_include_skip_navigation_where(bool async)
    {
        return base.Filtered_then_include_skip_navigation_where(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_then_include_skip_navigation_where_split(bool async)
    {
        return base.Filtered_then_include_skip_navigation_where_split(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filter_include_on_skip_navigation_combined_with_filtered_then_includes(bool async)
    {
        return base.Filter_include_on_skip_navigation_combined_with_filtered_then_includes(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filter_include_on_skip_navigation_combined_with_filtered_then_includes_split(bool async)
    {
        return base.Filter_include_on_skip_navigation_combined_with_filtered_then_includes_split(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filter_include_on_skip_navigation_combined(bool async)
    {
        return base.Filter_include_on_skip_navigation_combined(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filter_include_on_skip_navigation_combined_split(bool async)
    {
        return base.Filter_include_on_skip_navigation_combined_split(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Throws_when_different_filtered_include(bool async)
    {
        return base.Throws_when_different_filtered_include(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Throws_when_different_filtered_include_unidirectional(bool async)
    {
        return base.Throws_when_different_filtered_include_unidirectional(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Throws_when_different_filtered_then_include(bool async)
    {
        return base.Throws_when_different_filtered_then_include(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Throws_when_different_filtered_then_include_via_different_paths(bool async)
    {
        return base.Throws_when_different_filtered_then_include_via_different_paths(async);
    }
}
