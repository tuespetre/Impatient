using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class AsyncGearsOfWarQueryImpatientTest : AsyncGearsOfWarQueryTestBase<GearsOfWarQueryImpatientFixture>
    {
        public AsyncGearsOfWarQueryImpatientTest(GearsOfWarQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override Task ThenInclude_collection_on_derived_after_derived_collection()
        {
            return base.ThenInclude_collection_on_derived_after_derived_collection();
        }

        [Fact(Skip = EFCoreSkipReasons.ManualLeftJoinNullabilityPropagation)]
        [Trait("Impatient", "Feature Difference")]
        public override Task Correlated_collections_deeply_nested_left_join()
        {
            return base.Correlated_collections_deeply_nested_left_join();
        }

        [Fact(Skip = EFCoreSkipReasons.ManualLeftJoinNullabilityPropagation)]
        [Trait("Impatient", "Feature Difference")]
        public override Task Correlated_collections_on_left_join_with_predicate()
        {
            return base.Correlated_collections_on_left_join_with_predicate();
        }
    }
}
