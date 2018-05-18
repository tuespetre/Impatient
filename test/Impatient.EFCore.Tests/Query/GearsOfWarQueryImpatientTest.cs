using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.GearsOfWarModel;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class GearsOfWarQueryImpatientTest : GearsOfWarQueryTestBase<GearsOfWarQueryImpatientFixture>
    {
        public GearsOfWarQueryImpatientTest(GearsOfWarQueryImpatientFixture fixture) : base(fixture)
        {
        }

        #region skips

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void ThenInclude_collection_on_derived_after_derived_collection()
        {
            // There seems to be an issue with the test trying to cast a collection to a reference in an include operator.
            base.ThenInclude_collection_on_derived_after_derived_collection();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1()
        {
            // The immediate results are correct, but the secondary results (the includes)
            // do not match those of the test because we are materializing them before the coalesce
            // operator takes place. 

            // So the expression is g2 ?? g1. 
            // Say result 0 has a null g2. 
            // Result 0 will produce the g1 with its includes. 
            // Then say result 1 has a non-null g2, and that g2 happens to be the same entity as result 0's g1.
            // Result 1 will then produce a g2 that is the same reference as the g1 from result 0, includes and all.
            // I think this is technically correct, because what else are you going to do unless change tracking is off?

            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result1();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Select_null_propagation_optimization8()
        {
            base.Select_null_propagation_optimization8();
        }

        [Fact(Skip = EFCoreSkipReasons.NullNavigationProtection)]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Optional_navigation_type_compensation_works_with_predicate_negated_complex1()
        {
            base.Optional_navigation_type_compensation_works_with_predicate_negated_complex1();
        }

        [Fact(Skip = EFCoreSkipReasons.NullNavigationProtection)]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Optional_navigation_type_compensation_works_with_predicate_negated_complex2()
        {
            base.Optional_navigation_type_compensation_works_with_predicate_negated_complex2();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        [Trait("Impatient", "Feature Difference")]
        public override void Order_by_entity_qsre()
        {
            base.Order_by_entity_qsre();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        [Trait("Impatient", "Feature Difference")]
        public override void Order_by_entity_qsre_composite_key()
        {
            base.Order_by_entity_qsre_composite_key();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        [Trait("Impatient", "Feature Difference")]
        public override void Order_by_entity_qsre_with_other_orderbys()
        {
            base.Order_by_entity_qsre_with_other_orderbys();
        }

        [Fact(Skip = EFCoreSkipReasons.ManualLeftJoinNullabilityPropagation)]
        [Trait("Impatient", "Feature Difference")]
        public override void Correlated_collections_deeply_nested_left_join()
        {
            base.Correlated_collections_deeply_nested_left_join();
        }

        [Fact(Skip = EFCoreSkipReasons.ManualLeftJoinNullabilityPropagation)]
        [Trait("Impatient", "Feature Difference")]
        public override void Correlated_collections_on_left_join_with_predicate()
        {
            base.Correlated_collections_on_left_join_with_predicate();
        }

        #endregion

        [Fact]
        [Trait("Impatient", "Overridden for sorting")]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_coalesce_result2()
        {
            using (var context = CreateContext())
            {
                var query = from g1 in context.Gears
                            join g2 in context.Gears.Include(g => g.Weapons)
                                on g1.LeaderNickname equals g2.Nickname into grouping
                            from g2 in grouping.DefaultIfEmpty()
                            orderby g2.FullName
                            select g2 ?? g1;

                var result = query.ToList();

                Assert.Equal("Marcus", result[0].Nickname);
                Assert.Equal(2, result[0].Weapons.Count);
                Assert.Equal("Baird", result[1].Nickname);
                Assert.Equal(2, result[1].Weapons.Count);
                Assert.Equal("Marcus", result[2].Nickname);
                Assert.Equal("Marcus", result[3].Nickname);
                Assert.Equal("Marcus", result[4].Nickname);
            }
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Include_collection_group_by_reference()
        {
            base.Include_collection_group_by_reference();
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result()
        {
            base.Include_on_GroupJoin_SelectMany_DefaultIfEmpty_with_complex_projection_result();
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Where_compare_anonymous_types()
        {
            base.Where_compare_anonymous_types();
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Where_subquery_distinct_lastordefault_boolean()
        {
            base.Where_subquery_distinct_lastordefault_boolean();
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Where_subquery_distinct_last_boolean()
        {
            base.Where_subquery_distinct_last_boolean();
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Concat_with_scalar_projection()
        {
            base.Concat_with_scalar_projection();
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat()
        {
            base.Non_unicode_string_literals_is_used_for_non_unicode_column_with_concat();
        }

        [Fact]
        [Trait("Impatient", "Skipped by EFCore")]
        public override void Comparing_entities_using_Equals_inheritance()
        {
            base.Comparing_entities_using_Equals_inheritance();
        }
        
        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Include_with_group_by_distinct()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.Gears.Include(g => g.Weapons).OrderBy(g => g.Nickname).Distinct().GroupBy(g => g.HasSoulPatch).ToList();
                var expected = Fixture.QueryAsserter.ExpectedData.Set<Gear>().OrderBy(g => g.Nickname).Distinct().GroupBy(g => g.HasSoulPatch).ToList();

                // Overridden to apply ordering.
                actual = actual.OrderBy(g => g.Key).ToList();
                expected = expected.OrderBy(g => g.Key).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Key, actual[i].Key);
                    Assert.Equal(expected[i].Count(), actual[i].Count());
                }
            }
        }

        [Fact]
        [Trait("Impatient", "Adjusted entry count")]
        public override void Include_with_group_by_order_by_take()
        {
            using (var ctx = CreateContext())
            {
                var actual = ctx.Gears.Include(g => g.Weapons).OrderBy(g => g.Nickname).Take(3).GroupBy(g => g.HasSoulPatch).ToList();
                var expected = Fixture.QueryAsserter.ExpectedData.Set<Gear>().OrderBy(g => g.Nickname).Take(3).GroupBy(g => g.HasSoulPatch).ToList();

                // Overridden to apply ordering.
                actual = actual.OrderBy(g => g.Key).ToList();
                expected = expected.OrderBy(g => g.Key).ToList();

                Assert.Equal(expected.Count, actual.Count);
                for (var i = 0; i < expected.Count; i++)
                {
                    Assert.Equal(expected[i].Key, actual[i].Key);
                    Assert.Equal(expected[i].Count(), actual[i].Count());
                }
            }
        }
    }
}
