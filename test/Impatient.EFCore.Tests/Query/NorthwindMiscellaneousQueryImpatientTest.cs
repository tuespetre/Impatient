using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindMiscellaneousQueryImpatientTest : NorthwindMiscellaneousQueryRelationalTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindMiscellaneousQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task All_client_or_server_top_level(bool async)
    {
        return base.All_client_or_server_top_level(async);
    }

    [Trait("Translation", "Disagree with EF Core")]
    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Client_OrderBy_GroupBy_Group_ordering_works(bool async)
    {
        return base.Client_OrderBy_GroupBy_Group_ordering_works(async);
    }

    [Trait("Translation", "Disagree with EF Core")]
    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Default_if_empty_top_level_arg(bool async)
    {
        return base.Default_if_empty_top_level_arg(async);
    }

    [Trait("Translation", "Disagree with EF Core")]
    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Default_if_empty_top_level_arg_followed_by_projecting_constant(bool async)
    {
        return base.Default_if_empty_top_level_arg_followed_by_projecting_constant(async);
    }

    [Fact(Skip = "Bad test, says so in source implementation. The query is fine.")]
    public override Task Mixed_sync_async_query()
    {
        return base.Mixed_sync_async_query();
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2(bool async)
    {
        return base.No_orderby_added_for_client_side_GroupJoin_dependent_to_principal_LOJ_with_additional_join_condition2(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ(bool async)
    {
        return base.Orderby_added_for_client_side_GroupJoin_principal_to_dependent_LOJ(async);
    }

    // Again, I just don't agree with the EF Core approach on this one.
    [Trait("Translation", "Disagree with EF Core")]
    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task OrderBy_client_mixed(bool async)
    {
        return base.OrderBy_client_mixed(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Random_next_is_not_funcletized_2(bool async)
    {
        return base.Random_next_is_not_funcletized_2(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Random_next_is_not_funcletized_3(bool async)
    {
        return base.Random_next_is_not_funcletized_3(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Random_next_is_not_funcletized_5(bool async)
    {
        return base.Random_next_is_not_funcletized_5(async);
    }

    [Theory(Skip = EFCoreSkipReasons.BadMaterialization)]
    public override Task Select_correlated_subquery_filtered_returning_queryable_throws(bool async)
    {
        return base.Select_correlated_subquery_filtered_returning_queryable_throws(async);
    }

    [Theory(Skip = EFCoreSkipReasons.BadMaterialization)]
    public override Task Select_correlated_subquery_ordered_returning_queryable_throws(bool async)
    {
        return base.Select_correlated_subquery_ordered_returning_queryable_throws(async);
    }

    [Theory(Skip = EFCoreSkipReasons.BadMaterialization)]
    public override Task Select_subquery_recursive_trivial_returning_queryable(bool async)
    {
        return base.Select_subquery_recursive_trivial_returning_queryable(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task SelectMany_after_client_method(bool async)
    {
        return base.SelectMany_after_client_method(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Using_string_Equals_with_StringComparison_throws_informative_error(bool async)
    {
        return base.Using_string_Equals_with_StringComparison_throws_informative_error(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Using_static_string_Equals_with_StringComparison_throws_informative_error(bool async)
    {
        return base.Using_static_string_Equals_with_StringComparison_throws_informative_error(async);
    }

    // TODO: Translate List.Exists
    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Where_Join_Not_Exists(bool async)
    {
        return base.Where_Join_Not_Exists(async);
    }
}
