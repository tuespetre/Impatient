using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests
{
    public class FiltersInheritanceImpatientTest : FiltersInheritanceTestBase<ImpatientTestStore, FiltersInheritanceImpatientTest.FiltersInheritanceImpatientFixture>
    {
        public FiltersInheritanceImpatientTest(FiltersInheritanceImpatientFixture fixture) : base(fixture)
        {
        }

        public class FiltersInheritanceImpatientFixture : InheritanceImpatientFixture
        {
            protected override bool EnableFilters => true;
        }
    }
}
