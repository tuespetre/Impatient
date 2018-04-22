using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class AsyncQueryImpatientTest : AsyncQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public AsyncQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override Task Default_if_empty_top_level_arg()
        {
            return base.Default_if_empty_top_level_arg();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override Task Where_nested_field_access_closure_via_query_cache_error_method_null()
        {
            return base.Where_nested_field_access_closure_via_query_cache_error_method_null();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesGroupedEntitiesAreNotTracked)]
        public override Task GroupBy_with_element_selector2()
        {
            return base.GroupBy_with_element_selector2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestAssumesGroupedEntitiesAreNotTracked)]
        public override Task GroupBy_with_element_selector3()
        {
            return base.GroupBy_with_element_selector3();
        }

        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override Task OrderBy_multiple()
        {
            return base.OrderBy_multiple();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override Task Join_client_new_expression()
        {
            return base.Join_client_new_expression();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override Task GroupBy_Shadow2()
        {
            return base.GroupBy_Shadow2();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override Task OrderBy_multiple_queries()
        {
            return base.OrderBy_multiple_queries();
        }

        [Fact(Skip = EFCoreSkipReasons.TestIncorrectlyAssumesReturnedEntitiesAreNotTracked)]
        public override Task Where_subquery_on_collection()
        {
            return base.Where_subquery_on_collection();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override Task Mixed_sync_async_query()
        {
            return base.Mixed_sync_async_query();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override Task Where_nested_field_access_closure_via_query_cache_error_null()
        {
            return base.Where_nested_field_access_closure_via_query_cache_error_null();
        }

        [Fact(Skip = EFCoreSkipReasons.TestReliesOnUnguaranteedOrder)]
        public override Task GroupJoin_tracking_groups()
        {
            return base.GroupJoin_tracking_groups();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override Task Throws_on_concurrent_query_list()
        {
            return base.Throws_on_concurrent_query_list();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override Task Throws_on_concurrent_query_first()
        {
            return base.Throws_on_concurrent_query_first();
        }

        [Fact]
        public override Task String_Contains_Literal()
        {
            // Overridden; EFCore SQL Server test does this too
            return AssertQuery<Customer>(
                cs => cs.Where(c => c.ContactName.Contains("M")), // case-insensitive
                cs => cs.Where(c => c.ContactName.Contains("M") || c.ContactName.Contains("m")), // case-sensitive
                entryCount: 34);
        }
    }
}
