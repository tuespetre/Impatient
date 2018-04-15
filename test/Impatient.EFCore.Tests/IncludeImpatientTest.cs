using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.Northwind;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class IncludeImpatientTest : IncludeTestBase<NorthwindQueryImpatientFixture>
    {
        public IncludeImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        public override void GroupJoin_Include_collection_GroupBy_Select(bool useString)
        {
            base.GroupJoin_Include_collection_GroupBy_Select(useString);
        }

        public override void GroupJoin_Include_reference_GroupBy_Select(bool useString)
        {
            base.GroupJoin_Include_reference_GroupBy_Select(useString);
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Include_bad_navigation_property()
        {
            base.Include_bad_navigation_property();
        }

        public override void Include_closes_reader(bool useString)
        {
            base.Include_closes_reader(useString);
        }

        public override void Include_collection(bool useString)
        {
            base.Include_collection(useString);
        }

        public override void Include_collection_alias_generation(bool useString)
        {
            base.Include_collection_alias_generation(useString);
        }

        public override void Include_collection_and_reference(bool useString)
        {
            base.Include_collection_and_reference(useString);
        }

        public override void Include_collection_as_no_tracking(bool useString)
        {
            base.Include_collection_as_no_tracking(useString);
        }

        public override void Include_collection_as_no_tracking2(bool useString)
        {
            base.Include_collection_as_no_tracking2(useString);
        }

        public override void Include_collection_dependent_already_tracked(bool useString)
        {
            using (var context = CreateContext())
            {
                var orders
                    = context.Set<Order>()
                        .Where(o => o.CustomerID == "ALFKI")
                        .ToList();

                Assert.Equal(6, context.ChangeTracker.Entries().Count());

                var customer
                    = useString
                        ? context.Set<Customer>()
                            .Include("Orders")
                            .Single(c => c.CustomerID == "ALFKI")
                        : context.Set<Customer>()
                            .Include(c => c.Orders)
                            .Single(c => c.CustomerID == "ALFKI");

                Assert.Equal(orders, customer.Orders, ReferenceEqualityComparer.Instance);
                Assert.Equal(6, customer.Orders.Count);
                Assert.True(customer.Orders.All(o => o.Customer != null));
                Assert.Equal(6 + 1, context.ChangeTracker.Entries().Count());
                
            }
        }

        public override void Include_collection_dependent_already_tracked_as_no_tracking(bool useString)
        {
            base.Include_collection_dependent_already_tracked_as_no_tracking(useString);
        }

        public override void Include_collection_force_alias_uniquefication(bool useString)
        {
            base.Include_collection_force_alias_uniquefication(useString);
        }

        public override void Include_collection_GroupBy_Select(bool useString)
        {
            base.Include_collection_GroupBy_Select(useString);
        }

        public override void Include_collection_GroupJoin_GroupBy_Select(bool useString)
        {
            base.Include_collection_GroupJoin_GroupBy_Select(useString);
        }

        public override void Include_collection_Join_GroupBy_Select(bool useString)
        {
            base.Include_collection_Join_GroupBy_Select(useString);
        }

        public override void Include_collection_on_additional_from_clause(bool useString)
        {
            base.Include_collection_on_additional_from_clause(useString);
        }

        public override void Include_collection_on_additional_from_clause2(bool useString)
        {
            base.Include_collection_on_additional_from_clause2(useString);
        }

        public override void Include_collection_on_additional_from_clause_no_tracking(bool useString)
        {
            base.Include_collection_on_additional_from_clause_no_tracking(useString);
        }

        public override void Include_collection_on_additional_from_clause_with_filter(bool useString)
        {
            base.Include_collection_on_additional_from_clause_with_filter(useString);
        }

        public override void Include_collection_on_group_join_clause_with_filter(bool useString)
        {
            base.Include_collection_on_group_join_clause_with_filter(useString);
        }

        public override void Include_collection_on_inner_group_join_clause_with_filter(bool useString)
        {
            base.Include_collection_on_inner_group_join_clause_with_filter(useString);
        }

        public override void Include_collection_on_join_clause_with_filter(bool useString)
        {
            base.Include_collection_on_join_clause_with_filter(useString);
        }

        public override void Include_collection_on_join_clause_with_order_by_and_filter(bool useString)
        {
            base.Include_collection_on_join_clause_with_order_by_and_filter(useString);
        }

        public override void Include_collection_order_by_collection_column(bool useString)
        {
            base.Include_collection_order_by_collection_column(useString);
        }

        public override void Include_collection_order_by_key(bool useString)
        {
            base.Include_collection_order_by_key(useString);
        }

        public override void Include_collection_order_by_non_key(bool useString)
        {
            base.Include_collection_order_by_non_key(useString);
        }

        public override void Include_collection_order_by_non_key_with_first_or_default(bool useString)
        {
            base.Include_collection_order_by_non_key_with_first_or_default(useString);
        }

        public override void Include_collection_order_by_non_key_with_skip(bool useString)
        {
            base.Include_collection_order_by_non_key_with_skip(useString);
        }

        public override void Include_collection_order_by_non_key_with_take(bool useString)
        {
            base.Include_collection_order_by_non_key_with_take(useString);
        }

        public override void Include_collection_order_by_subquery(bool useString)
        {
            base.Include_collection_order_by_subquery(useString);
        }

        public override void Include_collection_principal_already_tracked(bool useString)
        {
            base.Include_collection_principal_already_tracked(useString);
        }

        public override void Include_collection_principal_already_tracked_as_no_tracking(bool useString)
        {
            base.Include_collection_principal_already_tracked_as_no_tracking(useString);
        }

        public override void Include_collection_SelectMany_GroupBy_Select(bool useString)
        {
            base.Include_collection_SelectMany_GroupBy_Select(useString);
        }

        public override void Include_collection_single_or_default_no_result(bool useString)
        {
            base.Include_collection_single_or_default_no_result(useString);
        }

        public override void Include_collection_skip_no_order_by(bool useString)
        {
            base.Include_collection_skip_no_order_by(useString);
        }

        public override void Include_collection_skip_take_no_order_by(bool useString)
        {
            base.Include_collection_skip_take_no_order_by(useString);
        }

        public override void Include_collection_take_no_order_by(bool useString)
        {
            base.Include_collection_take_no_order_by(useString);
        }

        public override void Include_collection_then_include_collection(bool useString)
        {
            base.Include_collection_then_include_collection(useString);
        }

        public override void Include_collection_then_include_collection_predicate(bool useString)
        {
            base.Include_collection_then_include_collection_predicate(useString);
        }

        public override void Include_collection_then_include_collection_then_include_reference(bool useString)
        {
            base.Include_collection_then_include_collection_then_include_reference(useString);
        }

        public override void Include_collection_then_reference(bool useString)
        {
            base.Include_collection_then_reference(useString);
        }

        public override void Include_collection_when_groupby(bool useString)
        {
            base.Include_collection_when_groupby(useString);
        }

        public override void Include_collection_when_groupby_subquery(bool useString)
        {
            base.Include_collection_when_groupby_subquery(useString);
        }

        public override void Include_collection_when_projection(bool useString)
        {
            base.Include_collection_when_projection(useString);
        }

        public override void Include_collection_with_client_filter(bool useString)
        {
            base.Include_collection_with_client_filter(useString);
        }

        public override void Include_collection_with_conditional_order_by(bool useString)
        {
            base.Include_collection_with_conditional_order_by(useString);
        }

        public override void Include_collection_with_filter(bool useString)
        {
            base.Include_collection_with_filter(useString);
        }

        public override void Include_collection_with_filter_reordered(bool useString)
        {
            base.Include_collection_with_filter_reordered(useString);
        }

        public override void Include_collection_with_last(bool useString)
        {
            base.Include_collection_with_last(useString);
        }

        public override void Include_collection_with_last_no_orderby(bool useString)
        {
            base.Include_collection_with_last_no_orderby(useString);
        }

        public override void Include_duplicate_collection(bool useString)
        {
            base.Include_duplicate_collection(useString);
        }

        public override void Include_duplicate_collection_result_operator(bool useString)
        {
            base.Include_duplicate_collection_result_operator(useString);
        }

        public override void Include_duplicate_collection_result_operator2(bool useString)
        {
            base.Include_duplicate_collection_result_operator2(useString);
        }

        public override void Include_duplicate_reference(bool useString)
        {
            base.Include_duplicate_reference(useString);
        }

        public override void Include_duplicate_reference2(bool useString)
        {
            base.Include_duplicate_reference2(useString);
        }

        public override void Include_duplicate_reference3(bool useString)
        {
            base.Include_duplicate_reference3(useString);
        }

        public override void Include_list(bool useString)
        {
            base.Include_list(useString);
        }

        public override void Include_multiple_references(bool useString)
        {
            base.Include_multiple_references(useString);
        }

        public override void Include_multiple_references_and_collection_multi_level(bool useString)
        {
            base.Include_multiple_references_and_collection_multi_level(useString);
        }

        public override void Include_multiple_references_and_collection_multi_level_reverse(bool useString)
        {
            base.Include_multiple_references_and_collection_multi_level_reverse(useString);
        }

        public override void Include_multiple_references_multi_level(bool useString)
        {
            base.Include_multiple_references_multi_level(useString);
        }

        public override void Include_multiple_references_multi_level_reverse(bool useString)
        {
            base.Include_multiple_references_multi_level_reverse(useString);
        }

        public override void Include_multiple_references_then_include_collection_multi_level(bool useString)
        {
            base.Include_multiple_references_then_include_collection_multi_level(useString);
        }

        public override void Include_multiple_references_then_include_collection_multi_level_reverse(bool useString)
        {
            base.Include_multiple_references_then_include_collection_multi_level_reverse(useString);
        }

        public override void Include_multiple_references_then_include_multi_level(bool useString)
        {
            base.Include_multiple_references_then_include_multi_level(useString);
        }

        public override void Include_multiple_references_then_include_multi_level_reverse(bool useString)
        {
            base.Include_multiple_references_then_include_multi_level_reverse(useString);
        }

        public override void Include_multi_level_collection_and_then_include_reference_predicate(bool useString)
        {
            base.Include_multi_level_collection_and_then_include_reference_predicate(useString);
        }

        public override void Include_multi_level_reference_and_collection_predicate(bool useString)
        {
            base.Include_multi_level_reference_and_collection_predicate(useString);
        }

        public override void Include_multi_level_reference_then_include_collection_predicate(bool useString)
        {
            base.Include_multi_level_reference_then_include_collection_predicate(useString);
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Include_property_expression_invalid()
        {
            base.Include_property_expression_invalid();
        }

        public override void Include_reference(bool useString)
        {
            base.Include_reference(useString);
        }

        public override void Include_references_and_collection_multi_level(bool useString)
        {
            base.Include_references_and_collection_multi_level(useString);
        }

        public override void Include_references_and_collection_multi_level_predicate(bool useString)
        {
            base.Include_references_and_collection_multi_level_predicate(useString);
        }

        public override void Include_references_multi_level(bool useString)
        {
            base.Include_references_multi_level(useString);
        }

        public override void Include_references_then_include_collection(bool useString)
        {
            base.Include_references_then_include_collection(useString);
        }

        public override void Include_references_then_include_collection_multi_level(bool useString)
        {
            base.Include_references_then_include_collection_multi_level(useString);
        }

        public override void Include_references_then_include_collection_multi_level_predicate(bool useString)
        {
            base.Include_references_then_include_collection_multi_level_predicate(useString);
        }

        public override void Include_references_then_include_multi_level(bool useString)
        {
            base.Include_references_then_include_multi_level(useString);
        }

        public override void Include_reference_alias_generation(bool useString)
        {
            base.Include_reference_alias_generation(useString);
        }

        public override void Include_reference_and_collection(bool useString)
        {
            base.Include_reference_and_collection(useString);
        }

        public override void Include_reference_and_collection_order_by(bool useString)
        {
            base.Include_reference_and_collection_order_by(useString);
        }

        public override void Include_reference_as_no_tracking(bool useString)
        {
            base.Include_reference_as_no_tracking(useString);
        }

        public override void Include_reference_dependent_already_tracked(bool useString)
        {
            base.Include_reference_dependent_already_tracked(useString);
        }

        public override void Include_reference_GroupBy_Select(bool useString)
        {
            base.Include_reference_GroupBy_Select(useString);
        }

        public override void Include_reference_GroupJoin_GroupBy_Select(bool useString)
        {
            base.Include_reference_GroupJoin_GroupBy_Select(useString);
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Include_reference_invalid()
        {
            base.Include_reference_invalid();
        }

        public override void Include_reference_Join_GroupBy_Select(bool useString)
        {
            base.Include_reference_Join_GroupBy_Select(useString);
        }

        public override void Include_reference_SelectMany_GroupBy_Select(bool useString)
        {
            base.Include_reference_SelectMany_GroupBy_Select(useString);
        }

        public override void Include_reference_single_or_default_when_no_result(bool useString)
        {
            base.Include_reference_single_or_default_when_no_result(useString);
        }

        public override void Include_reference_when_entity_in_projection(bool useString)
        {
            base.Include_reference_when_entity_in_projection(useString);
        }

        public override void Include_reference_when_projection(bool useString)
        {
            base.Include_reference_when_projection(useString);
        }

        public override void Include_reference_with_filter(bool useString)
        {
            base.Include_reference_with_filter(useString);
        }

        public override void Include_reference_with_filter_reordered(bool useString)
        {
            base.Include_reference_with_filter_reordered(useString);
        }

        [Theory(Skip = EFCoreSkipReasons.Punt)]
        [InlineData(true)]
        [InlineData(false)]
        public override void Include_specified_on_non_entity_not_supported(bool useString)
        {
            base.Include_specified_on_non_entity_not_supported(useString);
        }

        public override void Include_when_result_operator(bool useString)
        {
            base.Include_when_result_operator(useString);
        }

        public override void Include_where_skip_take_projection(bool useString)
        {
            base.Include_where_skip_take_projection(useString);
        }

        public override void Include_with_complex_projection(bool useString)
        {
            base.Include_with_complex_projection(useString);
        }

        public override void Include_with_skip(bool useString)
        {
            base.Include_with_skip(useString);
        }

        public override void Include_with_take(bool useString)
        {
            base.Include_with_take(useString);
        }

        public override void Join_Include_collection_GroupBy_Select(bool useString)
        {
            base.Join_Include_collection_GroupBy_Select(useString);
        }

        public override void Join_Include_reference_GroupBy_Select(bool useString)
        {
            base.Join_Include_reference_GroupBy_Select(useString);
        }

        public override void SelectMany_Include_collection_GroupBy_Select(bool useString)
        {
            base.SelectMany_Include_collection_GroupBy_Select(useString);
        }

        public override void SelectMany_Include_reference_GroupBy_Select(bool useString)
        {
            base.SelectMany_Include_reference_GroupBy_Select(useString);
        }

        public override void Then_include_collection_order_by_collection_column(bool useString)
        {
            base.Then_include_collection_order_by_collection_column(useString);
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Then_include_property_expression_invalid()
        {
            base.Then_include_property_expression_invalid();
        }
    }
}
