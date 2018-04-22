using Microsoft.EntityFrameworkCore.Query;
using Xunit;

namespace Impatient.EFCore.Tests
{
    public class FiltersImpatientTest : FiltersTestBase<NorthwindQueryImpatientFixture>
    {
        public FiltersImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = EFCoreSkipReasons.CompiledQueries)]
        public override void Compiled_query()
        {
            base.Compiled_query();
        }
    }
}
