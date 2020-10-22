using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable xUnit1003 // Theory methods must have test data
namespace Impatient.EFCore.Tests.Query
{
    public class ComplexNavigationsQueryImpatientTest : ComplexNavigationsQueryRelationalTestBase<ComplexNavigationsQueryImpatientTest.Fixture>
    {
        public ComplexNavigationsQueryImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        protected override QueryAsserter CreateQueryAsserter(Fixture fixture) =>
            new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_after_different_filtered_include_different_level(bool async)
        {
            return base.Filtered_include_after_different_filtered_include_different_level(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_after_different_filtered_include_different_level_split(bool async)
        {
            return base.Filtered_include_after_different_filtered_include_different_level_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_after_different_filtered_include_same_level(bool async)
        {
            return base.Filtered_include_after_different_filtered_include_same_level(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_after_different_filtered_include_same_level_split(bool async)
        {
            return base.Filtered_include_after_different_filtered_include_same_level_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_basic_Where(bool async)
        {
            return base.Filtered_include_basic_Where(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_OrderBy(bool async)
        {
            return base.Filtered_include_OrderBy(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_ThenInclude_OrderBy(bool async)
        {
            return base.Filtered_ThenInclude_OrderBy(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_ThenInclude_OrderBy(bool async)
        {
            return base.Filtered_include_ThenInclude_OrderBy(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_basic_OrderBy_Take(bool async)
        {
            return base.Filtered_include_basic_OrderBy_Take(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_basic_OrderBy_Skip(bool async)
        {
            return base.Filtered_include_basic_OrderBy_Skip(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_basic_OrderBy_Skip_Take(bool async)
        {
            return base.Filtered_include_basic_OrderBy_Skip_Take(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override void Filtered_include_Skip_without_OrderBy()
        {
            base.Filtered_include_Skip_without_OrderBy();
        }

        [Fact(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override void Filtered_include_Take_without_OrderBy()
        {
            base.Filtered_include_Take_without_OrderBy();
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_on_ThenInclude(bool async)
        {
            return base.Filtered_include_on_ThenInclude(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_after_reference_navigation(bool async)
        {
            return base.Filtered_include_after_reference_navigation(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_different_filter_set_on_same_navigation_twice(bool async)
        {
            return base.Filtered_include_different_filter_set_on_same_navigation_twice(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_different_filter_set_on_same_navigation_twice_multi_level(bool async)
        {
            return base.Filtered_include_different_filter_set_on_same_navigation_twice_multi_level(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_same_filter_set_on_same_navigation_twice(bool async)
        {
            return base.Filtered_include_same_filter_set_on_same_navigation_twice(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(bool async)
        {
            return base.Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(bool async)
        {
            return base.Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_and_non_filtered_include_on_same_navigation1(bool async)
        {
            return base.Filtered_include_and_non_filtered_include_on_same_navigation1(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_and_non_filtered_include_on_same_navigation2(bool async)
        {
            return base.Filtered_include_and_non_filtered_include_on_same_navigation2(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(bool async)
        {
            return base.Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_complex_three_level_with_middle_having_filter1(bool async)
        {
            return base.Filtered_include_complex_three_level_with_middle_having_filter1(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_complex_three_level_with_middle_having_filter2(bool async)
        {
            return base.Filtered_include_complex_three_level_with_middle_having_filter2(async);
        }

        [Fact(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override void Filtered_include_variable_used_inside_filter()
        {
            base.Filtered_include_variable_used_inside_filter();
        }

        [Fact(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override void Filtered_include_context_accessed_inside_filter()
        {
            base.Filtered_include_context_accessed_inside_filter();
        }

        [Fact(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override void Filtered_include_context_accessed_inside_filter_correlated()
        {
            base.Filtered_include_context_accessed_inside_filter_correlated();
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_include_parameter_used_inside_filter_throws(bool async)
        {
            return base.Filtered_include_include_parameter_used_inside_filter_throws(async);
        }

        [Fact(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override void Filtered_include_outer_parameter_used_inside_filter()
        {
            base.Filtered_include_outer_parameter_used_inside_filter();
        }

        [Fact(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override void Filtered_include_is_considered_loaded()
        {
            base.Filtered_include_is_considered_loaded();
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_with_Distinct_throws(bool async)
        {
            return base.Filtered_include_with_Distinct_throws(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_calling_methods_directly_on_parameter_throws(bool async)
        {
            return base.Filtered_include_calling_methods_directly_on_parameter_throws(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_after_reference_navigation_split(bool async)
        {
            return base.Filtered_include_after_reference_navigation_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation_split(bool async)
        {
            return base.Filtered_include_and_non_filtered_include_followed_by_then_include_on_same_navigation_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_and_non_filtered_include_on_same_navigation1_split(bool async)
        {
            return base.Filtered_include_and_non_filtered_include_on_same_navigation1_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_and_non_filtered_include_on_same_navigation2_split(bool async)
        {
            return base.Filtered_include_and_non_filtered_include_on_same_navigation2_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_basic_OrderBy_Skip_split(bool async)
        {
            return base.Filtered_include_basic_OrderBy_Skip_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_basic_OrderBy_Skip_Take_split(bool async)
        {
            return base.Filtered_include_basic_OrderBy_Skip_Take_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_basic_OrderBy_Take_split(bool async)
        {
            return base.Filtered_include_basic_OrderBy_Take_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_basic_Where_split(bool async)
        {
            return base.Filtered_include_basic_Where_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_calling_methods_directly_on_parameter_throws_split(bool async)
        {
            return base.Filtered_include_calling_methods_directly_on_parameter_throws_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_complex_three_level_with_middle_having_filter1_split(bool async)
        {
            return base.Filtered_include_complex_three_level_with_middle_having_filter1_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_complex_three_level_with_middle_having_filter2_split(bool async)
        {
            return base.Filtered_include_complex_three_level_with_middle_having_filter2_split(async);
        }

        [Fact(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override void Filtered_include_context_accessed_inside_filter_correlated_split()
        {
            base.Filtered_include_context_accessed_inside_filter_correlated_split();
        }

        [Fact(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override void Filtered_include_context_accessed_inside_filter_split()
        {
            base.Filtered_include_context_accessed_inside_filter_split();
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_different_filter_set_on_same_navigation_twice_multi_level_split(bool async)
        {
            return base.Filtered_include_different_filter_set_on_same_navigation_twice_multi_level_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_different_filter_set_on_same_navigation_twice_split(bool async)
        {
            return base.Filtered_include_different_filter_set_on_same_navigation_twice_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_include_parameter_used_inside_filter_throws_split(bool async)
        {
            return base.Filtered_include_include_parameter_used_inside_filter_throws_split(async);
        }

        [Fact(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override void Filtered_include_is_considered_loaded_split()
        {
            base.Filtered_include_is_considered_loaded_split();
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only_split(bool async)
        {
            return base.Filtered_include_multiple_multi_level_includes_with_first_level_using_filter_include_on_one_of_the_chains_only_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_on_ThenInclude_split(bool async)
        {
            return base.Filtered_include_on_ThenInclude_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_OrderBy_split(bool async)
        {
            return base.Filtered_include_OrderBy_split(async);
        }

        [Fact(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override void Filtered_include_outer_parameter_used_inside_filter_split()
        {
            base.Filtered_include_outer_parameter_used_inside_filter_split();
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes_split(bool async)
        {
            return base.Filtered_include_same_filter_set_on_same_navigation_twice_followed_by_ThenIncludes_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_same_filter_set_on_same_navigation_twice_split(bool async)
        {
            return base.Filtered_include_same_filter_set_on_same_navigation_twice_split(async);
        }

        [Fact(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override void Filtered_include_Skip_without_OrderBy_split()
        {
            base.Filtered_include_Skip_without_OrderBy_split();
        }

        [Fact(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override void Filtered_include_Take_without_OrderBy_split()
        {
            base.Filtered_include_Take_without_OrderBy_split();
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_ThenInclude_OrderBy_split(bool async)
        {
            return base.Filtered_include_ThenInclude_OrderBy_split(async);
        }

        [Fact(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override void Filtered_include_variable_used_inside_filter_split()
        {
            base.Filtered_include_variable_used_inside_filter_split();
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_include_with_Distinct_throws_split(bool async)
        {
            return base.Filtered_include_with_Distinct_throws_split(async);
        }

        [Theory(Skip = EFCoreSkipReasons.SpecialIncludes)]
        public override Task Filtered_ThenInclude_OrderBy_split(bool async)
        {
            return base.Filtered_ThenInclude_OrderBy_split(async);
        }

        public class Fixture : ComplexNavigationsQueryRelationalFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
#pragma warning restore xUnit1003 // Theory methods must have test data
