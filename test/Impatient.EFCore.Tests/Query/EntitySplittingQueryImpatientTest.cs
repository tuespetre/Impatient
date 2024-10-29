using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class EntitySplittingQueryImpatientTest : EntitySplittingQueryTestBase
{
    protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Normal_entity_owning_a_split_reference_with_main_fragment_not_sharing(bool async)
    {
        return base.Normal_entity_owning_a_split_reference_with_main_fragment_not_sharing(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Normal_entity_owning_a_split_reference_with_main_fragment_not_sharing_custom_projection(bool async)
    {
        return base.Normal_entity_owning_a_split_reference_with_main_fragment_not_sharing_custom_projection(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Normal_entity_owning_a_split_collection(bool async)
    {
        return base.Normal_entity_owning_a_split_collection(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Split_entity_owning_a_split_reference_without_table_sharing(bool async)
    {
        return base.Split_entity_owning_a_split_reference_without_table_sharing(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Split_entity_owning_a_split_collection(bool async)
    {
        return base.Split_entity_owning_a_split_collection(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Tph_entity_owning_a_split_reference_on_base_without_table_sharing(bool async)
    {
        return base.Tph_entity_owning_a_split_reference_on_base_without_table_sharing(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Tpt_entity_owning_a_split_reference_on_base_without_table_sharing(bool async)
    {
        return base.Tpt_entity_owning_a_split_reference_on_base_without_table_sharing(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Tph_entity_owning_a_split_reference_on_middle_without_table_sharing(bool async)
    {
        return base.Tph_entity_owning_a_split_reference_on_middle_without_table_sharing(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Tpt_entity_owning_a_split_reference_on_middle_without_table_sharing(bool async)
    {
        return base.Tpt_entity_owning_a_split_reference_on_middle_without_table_sharing(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Tph_entity_owning_a_split_reference_on_leaf_without_table_sharing(bool async)
    {
        return base.Tph_entity_owning_a_split_reference_on_leaf_without_table_sharing(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Tpt_entity_owning_a_split_reference_on_leaf_without_table_sharing(bool async)
    {
        return base.Tpt_entity_owning_a_split_reference_on_leaf_without_table_sharing(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Tpc_entity_owning_a_split_reference_on_leaf_without_table_sharing(bool async)
    {
        return base.Tpc_entity_owning_a_split_reference_on_leaf_without_table_sharing(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Tph_entity_owning_a_split_collection_on_base(bool async)
    {
        return base.Tph_entity_owning_a_split_collection_on_base(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Tpt_entity_owning_a_split_collection_on_base(bool async)
    {
        return base.Tpt_entity_owning_a_split_collection_on_base(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Tph_entity_owning_a_split_collection_on_middle(bool async)
    {
        return base.Tph_entity_owning_a_split_collection_on_middle(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Tpt_entity_owning_a_split_collection_on_middle(bool async)
    {
        return base.Tpt_entity_owning_a_split_collection_on_middle(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Tph_entity_owning_a_split_collection_on_leaf(bool async)
    {
        return base.Tph_entity_owning_a_split_collection_on_leaf(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Tpt_entity_owning_a_split_collection_on_leaf(bool async)
    {
        return base.Tpt_entity_owning_a_split_collection_on_leaf(async);
    }

    [ConditionalTheory(Skip = "Issue29075")]
    public override Task Tpc_entity_owning_a_split_collection_on_leaf(bool async)
    {
        return base.Tpc_entity_owning_a_split_collection_on_leaf(async);
    }
}
