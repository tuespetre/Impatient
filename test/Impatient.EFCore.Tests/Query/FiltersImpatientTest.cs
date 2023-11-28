using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class FiltersImpatientTest : NorthwindQueryFiltersQueryTestBase<NorthwindQueryFiltersImpatientFixture>
    {
        public FiltersImpatientTest(NorthwindQueryFiltersImpatientFixture fixture) : base(fixture)
        {
        }
    }
    
    public class NorthwindQueryFiltersImpatientFixture : NorthwindQueryImpatientFixtureBase<NorthwindQueryFiltersCustomizer>
    {
    }
}
