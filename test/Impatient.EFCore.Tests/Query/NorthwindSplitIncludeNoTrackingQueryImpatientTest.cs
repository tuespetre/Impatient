using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindSplitIncludeNoTrackingQueryImpatientTest : NorthwindSplitIncludeNoTrackingQueryTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindSplitIncludeNoTrackingQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Include_collection_with_client_filter(bool async)
    {
        return base.Include_collection_with_client_filter(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Include_collection_with_last_no_orderby(bool async)
    {
        return base.Include_collection_with_last_no_orderby(async);
    }

    const string IncludeCycleSkipReason = "Include cycles -- need to plan";

    [Theory(Skip = IncludeCycleSkipReason)]
    public override Task Include_multi_level_reference_and_collection_predicate(bool async)
    {
        return base.Include_multi_level_reference_and_collection_predicate(async);
    }

    [Theory(Skip = IncludeCycleSkipReason)]
    public override Task Include_multi_level_reference_then_include_collection_predicate(bool async)
    {
        return base.Include_multi_level_reference_then_include_collection_predicate(async);
    }

    [Theory(Skip = IncludeCycleSkipReason)]
    public override Task Include_multiple_references_and_collection_multi_level(bool async)
    {
        return base.Include_multiple_references_and_collection_multi_level(async);
    }

    [Theory(Skip = IncludeCycleSkipReason)]
    public override Task Include_multiple_references_and_collection_multi_level_reverse(bool async)
    {
        return base.Include_multiple_references_and_collection_multi_level_reverse(async);
    }

    [Theory(Skip = IncludeCycleSkipReason)]
    public override Task Include_multiple_references_then_include_collection_multi_level(bool async)
    {
        return base.Include_multiple_references_then_include_collection_multi_level(async);
    }

    [Theory(Skip = IncludeCycleSkipReason)]
    public override Task Include_multiple_references_then_include_collection_multi_level_reverse(bool async)
    {
        return base.Include_multiple_references_then_include_collection_multi_level_reverse(async);
    }

    [Theory(Skip = IncludeCycleSkipReason)]
    public override Task Include_reference_and_collection_order_by(bool async)
    {
        return base.Include_reference_and_collection_order_by(async);
    }

    [Theory(Skip = IncludeCycleSkipReason)]
    public override Task Include_references_and_collection_multi_level(bool async)
    {
        return base.Include_references_and_collection_multi_level(async);
    }

    [Theory(Skip = IncludeCycleSkipReason)]
    public override Task Include_references_and_collection_multi_level_predicate(bool async)
    {
        return base.Include_references_and_collection_multi_level_predicate(async);
    }

    [Theory(Skip = IncludeCycleSkipReason)]
    public override Task Include_references_then_include_collection(bool async)
    {
        return base.Include_references_then_include_collection(async);
    }

    [Theory(Skip = IncludeCycleSkipReason)]
    public override Task Include_references_then_include_collection_multi_level(bool async)
    {
        return base.Include_references_then_include_collection_multi_level(async);
    }

    [Theory(Skip = IncludeCycleSkipReason)]
    public override Task Include_references_then_include_collection_multi_level_predicate(bool async)
    {
        return base.Include_references_then_include_collection_multi_level_predicate(async);
    }

    [Theory(Skip = IncludeCycleSkipReason)]
    public override Task Include_with_cycle_does_not_throw_when_AsNoTrackingWithIdentityResolution(bool async)
    {
        return base.Include_with_cycle_does_not_throw_when_AsNoTrackingWithIdentityResolution(async);
    }

    [Theory(Skip = IncludeCycleSkipReason)]
    public override Task Include_with_cycle_does_not_throw_when_AsTracking_NoTrackingWithIdentityResolution(bool async)
    {
        return base.Include_with_cycle_does_not_throw_when_AsTracking_NoTrackingWithIdentityResolution(async);
    }
}
