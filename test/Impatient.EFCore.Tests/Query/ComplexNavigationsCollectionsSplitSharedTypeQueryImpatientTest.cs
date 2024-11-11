using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using static Impatient.EFCore.Tests.Query.ComplexNavigationsCollectionsSplitSharedTypeQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class ComplexNavigationsCollectionsSplitSharedTypeQueryImpatientTest : ComplexNavigationsCollectionsSplitSharedTypeQueryRelationalTestBase<Fixture>
{
    public ComplexNavigationsCollectionsSplitSharedTypeQueryImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => base.Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(Fixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : ComplexNavigationsSharedTypeQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_after_different_filtered_include_different_level(bool async)
    {
        return base.Filtered_include_after_different_filtered_include_different_level(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_after_different_filtered_include_same_level(bool async)
    {
        return base.Filtered_include_after_different_filtered_include_same_level(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_after_reference_navigation(bool async)
    {
        return base.Filtered_include_after_reference_navigation(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(bool async)
    {
        return base.Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_and_non_filtered_include_on_same_navigation1(bool async)
    {
        return base.Filtered_include_and_non_filtered_include_on_same_navigation1(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_and_non_filtered_include_on_same_navigation2(bool async)
    {
        return base.Filtered_include_and_non_filtered_include_on_same_navigation2(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_basic_OrderBy_Skip(bool async)
    {
        return base.Filtered_include_basic_OrderBy_Skip(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_basic_OrderBy_Skip_Take(bool async)
    {
        return base.Filtered_include_basic_OrderBy_Skip_Take(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_basic_OrderBy_Skip_Take_EF_Property(bool async)
    {
        return base.Filtered_include_basic_OrderBy_Skip_Take_EF_Property(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_basic_OrderBy_Take(bool async)
    {
        return base.Filtered_include_basic_OrderBy_Take(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_basic_Where(bool async)
    {
        return base.Filtered_include_basic_Where(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_basic_Where_EF_Property(bool async)
    {
        return base.Filtered_include_basic_Where_EF_Property(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_calling_methods_directly_on_parameter_throws(bool async)
    {
        return base.Filtered_include_calling_methods_directly_on_parameter_throws(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_complex_three_level_with_middle_having_filter1(bool async)
    {
        return base.Filtered_include_complex_three_level_with_middle_having_filter1(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_complex_three_level_with_middle_having_filter2(bool async)
    {
        return base.Filtered_include_complex_three_level_with_middle_having_filter2(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_context_accessed_inside_filter(bool async)
    {
        return base.Filtered_include_context_accessed_inside_filter(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_context_accessed_inside_filter_correlated(bool async)
    {
        return base.Filtered_include_context_accessed_inside_filter_correlated(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_different_filter_set_on_same_navigation_twice(bool async)
    {
        return base.Filtered_include_different_filter_set_on_same_navigation_twice(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_different_filter_set_on_same_navigation_twice_multi_level(bool async)
    {
        return base.Filtered_include_different_filter_set_on_same_navigation_twice_multi_level(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_include_parameter_used_inside_filter_throws(bool async)
    {
        return base.Filtered_include_include_parameter_used_inside_filter_throws(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_is_considered_loaded(bool async)
    {
        return base.Filtered_include_is_considered_loaded(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(bool async)
    {
        return base.Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_on_ThenInclude(bool async)
    {
        return base.Filtered_include_on_ThenInclude(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_on_ThenInclude_EF_Property(bool async)
    {
        return base.Filtered_include_on_ThenInclude_EF_Property(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_OrderBy(bool async)
    {
        return base.Filtered_include_OrderBy(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_OrderBy_EF_Property(bool async)
    {
        return base.Filtered_include_OrderBy_EF_Property(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_outer_parameter_used_inside_filter(bool async)
    {
        return base.Filtered_include_outer_parameter_used_inside_filter(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_same_filter_set_on_same_navigation_twice(bool async)
    {
        return base.Filtered_include_same_filter_set_on_same_navigation_twice(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(bool async)
    {
        return base.Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_Skip_Take_with_another_Skip_Take_on_top_level(bool async)
    {
        return base.Filtered_include_Skip_Take_with_another_Skip_Take_on_top_level(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_Skip_without_OrderBy(bool async)
    {
        return base.Filtered_include_Skip_without_OrderBy(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_Take_without_OrderBy(bool async)
    {
        return base.Filtered_include_Take_without_OrderBy(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_Take_with_another_Take_on_top_level(bool async)
    {
        return base.Filtered_include_Take_with_another_Take_on_top_level(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_ThenInclude_OrderBy(bool async)
    {
        return base.Filtered_include_ThenInclude_OrderBy(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_variable_used_inside_filter(bool async)
    {
        return base.Filtered_include_variable_used_inside_filter(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_with_Distinct_throws(bool async)
    {
        return base.Filtered_include_with_Distinct_throws(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_FirstOrDefault_on_top_level(bool async)
    {
        return base.Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_FirstOrDefault_on_top_level(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_unordered_Take_on_top_level(bool async)
    {
        return base.Filtered_include_with_Take_without_order_by_followed_by_ThenInclude_and_unordered_Take_on_top_level(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Filtered_ThenInclude_OrderBy(bool async)
    {
        return base.Filtered_ThenInclude_OrderBy(async);
    }

    [Theory(Skip = TranslationBeyondEF)]
    public override Task Include_inside_subquery(bool async)
    {
        return base.Include_inside_subquery(async);
    }

    [Theory(Skip = SpecialIncludes)]
    public override Task Include_partially_added_before_Where_and_then_build_upon_with_filtered_include(bool async)
    {
        return base.Include_partially_added_before_Where_and_then_build_upon_with_filtered_include(async);
    }
}