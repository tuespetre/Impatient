using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class FiltersImpatientTest : FiltersTestBase<NorthwindQueryFiltersImpatientFixture>
    {
        public FiltersImpatientTest(NorthwindQueryFiltersImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void Compiled_query()
        {
            base.Compiled_query();
        }

        [Fact(Skip = EFCoreSkipReasons.Punt)]
        public override void Included_many_to_one_query()
        {
            // I don't know if the base test is right.
            // If the orders have a filter that their customer's companyname is not null,
            // then an order with a null customer should not match the filter IMO.
            // Needs clarification

            // https://github.com/aspnet/EntityFrameworkCore/issues/11957
            
            // base.Included_many_to_one_query();
        }
    }
    
    public class NorthwindQueryFiltersImpatientFixture : NorthwindQueryImpatientFixtureBase<NorthwindFiltersCustomizer>
    {
    }
}
