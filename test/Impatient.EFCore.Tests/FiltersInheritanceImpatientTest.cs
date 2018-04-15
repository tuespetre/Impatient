using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests
{
    public class FiltersInheritanceImpatientTest : FiltersInheritanceTestBase<ImpatientTestStore, FiltersInheritanceImpatientTest.FiltersInheritanceImpatientFixture>
    {
        public FiltersInheritanceImpatientTest(FiltersInheritanceImpatientFixture fixture) : base(fixture)
        {
        }

        public override void Can_use_derived_set()
        {
            base.Can_use_derived_set();
        }

        public override void Can_use_IgnoreQueryFilters_and_GetDatabaseValues()
        {
            base.Can_use_IgnoreQueryFilters_and_GetDatabaseValues();
        }

        public override void Can_use_is_kiwi()
        {
            base.Can_use_is_kiwi();
        }

        public override void Can_use_is_kiwi_in_projection()
        {
            base.Can_use_is_kiwi_in_projection();
        }

        public override void Can_use_is_kiwi_with_other_predicate()
        {
            base.Can_use_is_kiwi_with_other_predicate();
        }

        public override void Can_use_of_type_animal()
        {
            base.Can_use_of_type_animal();
        }

        public override void Can_use_of_type_bird()
        {
            base.Can_use_of_type_bird();
        }

        public override void Can_use_of_type_bird_first()
        {
            base.Can_use_of_type_bird_first();
        }

        public override void Can_use_of_type_bird_predicate()
        {
            base.Can_use_of_type_bird_predicate();
        }

        public override void Can_use_of_type_bird_with_projection()
        {
            base.Can_use_of_type_bird_with_projection();
        }

        public override void Can_use_of_type_kiwi()
        {
            base.Can_use_of_type_kiwi();
        }

        public class FiltersInheritanceImpatientFixture : InheritanceImpatientFixture
        {
            protected override bool EnableFilters => true;
        }
    }
}
