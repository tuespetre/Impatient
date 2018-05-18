using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class FiltersInheritanceImpatientTest : FiltersInheritanceTestBase<FiltersInheritanceImpatientTest.FiltersInheritanceImpatientFixture>
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
