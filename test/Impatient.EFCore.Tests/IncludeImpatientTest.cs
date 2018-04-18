using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class IncludeImpatientTest : IncludeTestBase<NorthwindQueryImpatientFixture>
    {
        public IncludeImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Include_bad_navigation_property()
        {
            base.Include_bad_navigation_property();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Include_property_expression_invalid()
        {
            base.Include_property_expression_invalid();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Include_reference_invalid()
        {
            base.Include_reference_invalid();
        }

        [Theory(Skip = EFCoreSkipReasons.Punt)]
        [InlineData(true)]
        [InlineData(false)]
        public override void Include_specified_on_non_entity_not_supported(bool useString)
        {
            base.Include_specified_on_non_entity_not_supported(useString);
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Then_include_property_expression_invalid()
        {
            base.Then_include_property_expression_invalid();
        }

        [Theory(Skip = EFCoreSkipReasons.Punt)]
        [InlineData(true)]
        [InlineData(false)]
        public override void GroupJoin_Include_reference_GroupBy_Select(bool useString)
        {
            // We load more than is necessary here due to an issue with the
            // ProjectionBubblingExpressionVisitor and how its result is
            // used by the ResultTrackingComposingExpressionVisitor.
            base.GroupJoin_Include_reference_GroupBy_Select(useString);
        }
    }
}
