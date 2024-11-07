using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;

namespace Impatient.EFCore.Tests.Query;

public class TPCRelationshipsQueryImpatientTest : TPCRelationshipsQueryTestBase<TPCRelationshipsQueryImpatientTest.Fixture>
{
    public TPCRelationshipsQueryImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => base.Fixture.TestSqlLoggerFactory.AssertSql(expected);

    public new class Fixture : TPCRelationshipsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    public override async Task Include_collection_with_inheritance(bool async)
    {
        await base.Include_collection_with_inheritance(async);

        AssertSql("""
            SELECT (
                SELECT [o].[Id] AS [Id], [o].[Name] AS [Name]
                FROM [OwnedCollections] AS [o]
                WHERE [set].[Item1] = [o].[BaseInheritanceRelationshipEntityId]
                FOR JSON PATH
            ) AS [OwnedCollectionOnBase], (
                SELECT [o_0].[Id] AS [Id], [o_0].[Name] AS [Name]
                FROM [DerivedEntities_OwnedCollectionOnDerived] AS [o_0]
                WHERE [set].[Item1] = [o_0].[DerivedInheritanceRelationshipEntityId]
                FOR JSON PATH
            ) AS [<DerivedInheritanceRelationshipEntity>OwnedCollectionOnDerived], (
                SELECT [set_0].[Item1] AS [Item1], [set_0].[Item2] AS [Item2], [set_0].[Item3] AS [Item3], [set_0].[Item4] AS [Item4], [set_0].[Item5] AS [Item5]
                FROM (
                    SELECT [b].[Id] AS [Item1], [b].[BaseParentId] AS [Item2], [b].[Name] AS [Item3], NULL AS [Item4], N'BaseCollectionOnBase' AS [Item5]
                    FROM [BaseCollectionsOnBase] AS [b]
                    UNION ALL
                    SELECT [d].[Id] AS [Item1], [d].[BaseParentId] AS [Item2], [d].[Name] AS [Item3], [d].[DerivedProperty] AS [Item4], N'DerivedCollectionOnBase' AS [Item5]
                    FROM [DerivedCollectionsOnBase] AS [d]
                ) AS [set_0]
                WHERE [set].[Item1] = [set_0].[Item2]
                FOR JSON PATH
            ) AS [BaseCollectionOnBase], [set].[Item1] AS [Item1], [set].[Item2] AS [Item2], [set].[Item3] AS [Item3], [set].[Item4] AS [Item4], [set].[Item5] AS [Item5], [set].[Item6] AS [Item6], [set].[Item7] AS [Item7], [set].[Rest.Item1] AS [Rest.Item1], [set].[Rest.Item2] AS [Rest.Item2], [set].[Rest.Item3] AS [Rest.Item3]
            FROM (
                SELECT [b_0].[Id] AS [Item1], [b_0].[Name] AS [Item2], NULL AS [Item3], [o_1].[BaseInheritanceRelationshipEntityId] AS [Item4], [o_1].[Id] AS [Item5], [o_1].[Name] AS [Item6], NULL AS [Item7], NULL AS [Rest.Item1], NULL AS [Rest.Item2], N'BaseInheritanceRelationshipEntity' AS [Rest.Item3]
                FROM [BaseEntities] AS [b_0]
                LEFT JOIN [OwnedReferences] AS [o_1] ON [b_0].[Id] = [o_1].[BaseInheritanceRelationshipEntityId]
                UNION ALL
                SELECT [d_0].[Id] AS [Item1], [d_0].[Name] AS [Item2], [d_0].[BaseId] AS [Item3], [o_2].[BaseInheritanceRelationshipEntityId] AS [Item4], [o_2].[Id] AS [Item5], [o_2].[Name] AS [Item6], [d_0].[Id] AS [Item7], [d_0].[OwnedReferenceOnDerived_Id] AS [Rest.Item1], [d_0].[OwnedReferenceOnDerived_Name] AS [Rest.Item2], N'DerivedInheritanceRelationshipEntity' AS [Rest.Item3]
                FROM [DerivedEntities] AS [d_0]
                LEFT JOIN [OwnedReferences] AS [o_2] ON [d_0].[Id] = [o_2].[BaseInheritanceRelationshipEntityId]
            ) AS [set]
            """);
    }
}
