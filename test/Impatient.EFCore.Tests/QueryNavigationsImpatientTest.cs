using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class QueryNavigationsImpatientTest : QueryNavigationsTestBase<NorthwindQueryImpatientFixture>
    {
        public QueryNavigationsImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesGroupedEntitiesAreNotTracked)]
        public override void Navigation_projection_on_groupjoin_qsre()
        {
            base.Navigation_projection_on_groupjoin_qsre();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesGroupedEntitiesAreNotTracked)]
        public override void Navigation_projection_on_groupjoin_qsre_no_outer_in_final_result()
        {
            base.Navigation_projection_on_groupjoin_qsre_no_outer_in_final_result();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesGroupedEntitiesAreNotTracked)]
        public override void Navigation_projection_on_groupjoin_qsre_with_empty_grouping()
        {
            base.Navigation_projection_on_groupjoin_qsre_with_empty_grouping();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesIntermediateNavigationsAreTracked)]
        public override void Select_collection_navigation_multi_part()
        {
            base.Select_collection_navigation_multi_part();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesIntermediateNavigationsAreTracked)]
        public override void Select_collection_navigation_simple()
        {
            base.Select_collection_navigation_simple();
        }

        [Fact(Skip = "EF Core doesn't track the entities but that seems incorrect")]
        public override void Select_collection_FirstOrDefault_project_entity()
        {
            base.Select_collection_FirstOrDefault_project_entity();
        }

        [Fact(Skip = "EF Core doesn't track the entities but that seems incorrect")]
        public override void Take_Select_Navigation()
        {
            base.Take_Select_Navigation();
        }

        [Fact(Skip = "EF Core doesn't track the entities but that seems incorrect")]
        public override void Skip_Select_Navigation()
        {
            base.Skip_Select_Navigation();
        }
    }
}
