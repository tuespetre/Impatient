using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

#pragma warning disable xUnit1003 // Theory methods must have test data

public class EntitySplittingQueryImpatientTest : EntitySplittingQueryTestBase
{
    protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

    public override async Task Normal_entity_owning_a_split_reference_with_main_fragment_sharing(bool async)
    {
        await base.Normal_entity_owning_a_split_reference_with_main_fragment_sharing(async);

        TestSqlLoggerFactory.AssertSql("""
SELECT [e].[EntityThreeId] AS [EntityThreeId], [e].[Id] AS [Id], [e].[IntValue1] AS [IntValue1], [e].[IntValue2] AS [IntValue2], [e].[IntValue3] AS [IntValue3], [e].[IntValue4] AS [IntValue4], [e].[StringValue1] AS [StringValue1], [e].[StringValue2] AS [StringValue2], [e].[StringValue3] AS [StringValue3], [e].[StringValue4] AS [StringValue4], [e].[OwnedReference_Id] AS [OwnedReference.$empty], [e].[Id] AS [OwnedReference.EntityOneId], [e].[OwnedReference_Id] AS [OwnedReference.Id], [e].[OwnedReference_OwnedIntValue1] AS [OwnedReference.OwnedIntValue1], [e].[OwnedReference_OwnedIntValue2] AS [OwnedReference.OwnedIntValue2], [o].[OwnedIntValue3] AS [OwnedReference.OwnedIntValue3], [o_0].[OwnedIntValue4] AS [OwnedReference.OwnedIntValue4], [e].[OwnedReference_OwnedStringValue1] AS [OwnedReference.OwnedStringValue1], [e].[OwnedReference_OwnedStringValue2] AS [OwnedReference.OwnedStringValue2], [o].[OwnedStringValue3] AS [OwnedReference.OwnedStringValue3], [o_0].[OwnedStringValue4] AS [OwnedReference.OwnedStringValue4]
FROM [EntityOne] AS [e]
LEFT JOIN [OwnedReferenceExtras2] AS [o_0] ON [e].[Id] = [o_0].[EntityOneId]
LEFT JOIN [OwnedReferenceExtras1] AS [o] ON [e].[Id] = [o].[EntityOneId]
""");
    }

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
