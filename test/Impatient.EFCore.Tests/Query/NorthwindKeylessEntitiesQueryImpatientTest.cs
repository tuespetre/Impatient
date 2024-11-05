using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Query;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindKeylessEntitiesQueryImpatientTest : NorthwindKeylessEntitiesQueryRelationalTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindKeylessEntitiesQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task Auto_initialized_view_set(bool async)
    {
        return base.Auto_initialized_view_set(async);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task Collection_correlated_with_keyless_entity_in_predicate_works(bool async)
    {
        return base.Collection_correlated_with_keyless_entity_in_predicate_works(async);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task Collection_of_entities_projecting_correlated_collection_of_keyless_entities(bool async)
    {
        return base.Collection_of_entities_projecting_correlated_collection_of_keyless_entities(async);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task Count_over_keyless_entity(bool async)
    {
        return base.Count_over_keyless_entity(async);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task Count_over_keyless_entity_with_pushdown(bool async)
    {
        return base.Count_over_keyless_entity_with_pushdown(async);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task Count_over_keyless_entity_with_pushdown_empty_projection(bool async)
    {
        return base.Count_over_keyless_entity_with_pushdown_empty_projection(async);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task KeylessEntity_groupby(bool async)
    {
        return base.KeylessEntity_groupby(async);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task KeylessEntity_select_where_navigation(bool async)
    {
        return base.KeylessEntity_select_where_navigation(async);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task KeylessEntity_select_where_navigation_multi_level(bool async)
    {
        return base.KeylessEntity_select_where_navigation_multi_level(async);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task KeylessEntity_simple(bool async)
    {
        return base.KeylessEntity_simple(async);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task KeylessEntity_where_simple(bool async)
    {
        return base.KeylessEntity_where_simple(async);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task KeylessEntity_with_defining_query(bool async)
    {
        return base.KeylessEntity_with_defining_query(async);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task KeylessEntity_with_defining_query_and_correlated_collection(bool async)
    {
        return base.KeylessEntity_with_defining_query_and_correlated_collection(async);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task KeylessEntity_with_included_nav(bool async)
    {
        return base.KeylessEntity_with_included_nav(async);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task KeylessEntity_with_included_navs_multi_level(bool async)
    {
        return base.KeylessEntity_with_included_navs_multi_level(async);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task KeylessEntity_with_mixed_tracking(bool async)
    {
        return base.KeylessEntity_with_mixed_tracking(async);
    }

    public override async Task KeylessEntity_with_nav_defining_query(bool async)
    {
        await Assert.ThrowsAsync<SqlException>(() => base.KeylessEntity_with_nav_defining_query(async));

        AssertSql("""
            SELECT [cq].[CompanyName] AS [CompanyName], [cq].[OrderCount] AS [OrderCount], [cq].[SearchTerm] AS [SearchTerm]
            FROM [CustomerQueryWithQueryFilter] AS [cq]
            WHERE [cq].[OrderCount] > 0
            """);
    }

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task Projecting_collection_correlated_with_keyless_entity_throws(bool async)
    {
        return base.Projecting_collection_correlated_with_keyless_entity_throws(async);
    }

    private void AssertSql(string expected) => Fixture.TestSqlLoggerFactory.AssertSql(expected);
}
