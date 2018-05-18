using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class CompiledQueryImpatientTest : CompiledQueryTestBase<NorthwindQueryImpatientFixture>
    {
        public CompiledQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void DbQuery_query()
        {
            base.DbQuery_query();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override Task DbQuery_query_async()
        {
            return base.DbQuery_query_async();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void DbQuery_query_first()
        {
            base.DbQuery_query_first();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override Task DbQuery_query_first_async()
        {
            return base.DbQuery_query_first_async();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void DbSet_query()
        {
            base.DbSet_query();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override Task DbSet_query_async()
        {
            return base.DbSet_query_async();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void DbSet_query_first()
        {
            base.DbSet_query_first();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override Task DbSet_query_first_async()
        {
            return base.DbSet_query_first_async();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override Task First_query_with_cancellation_async()
        {
            return base.First_query_with_cancellation_async();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void First_query_with_single_parameter()
        {
            base.First_query_with_single_parameter();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override Task First_query_with_single_parameter_async()
        {
            return base.First_query_with_single_parameter_async();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void Multiple_queries()
        {
            base.Multiple_queries();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void Query_ending_with_include()
        {
            base.Query_ending_with_include();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void Query_with_array_parameter()
        {
            base.Query_with_array_parameter();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override Task Query_with_array_parameter_async()
        {
            return base.Query_with_array_parameter_async();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void Query_with_closure()
        {
            base.Query_with_closure();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override Task Query_with_closure_async()
        {
            return base.Query_with_closure_async();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override Task Query_with_closure_async_null()
        {
            return base.Query_with_closure_async_null();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void Query_with_closure_null()
        {
            base.Query_with_closure_null();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void Query_with_contains()
        {
            base.Query_with_contains();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void Query_with_single_parameter()
        {
            base.Query_with_single_parameter();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override Task Query_with_single_parameter_async()
        {
            return base.Query_with_single_parameter_async();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void Query_with_single_parameter_with_include()
        {
            base.Query_with_single_parameter_with_include();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void Query_with_three_parameters()
        {
            base.Query_with_three_parameters();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override Task Query_with_three_parameters_async()
        {
            return base.Query_with_three_parameters_async();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void Query_with_two_parameters()
        {
            base.Query_with_two_parameters();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override Task Query_with_two_parameters_async()
        {
            return base.Query_with_two_parameters_async();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void Untyped_context()
        {
            base.Untyped_context();
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override Task Untyped_context_async()
        {
            return base.Untyped_context_async();
        }
    }
}
