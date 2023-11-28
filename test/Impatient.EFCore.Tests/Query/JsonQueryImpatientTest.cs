using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query
{
    public class JsonQueryImpatientTest : JsonQueryTestBase<JsonQueryImpatientTest.Fixture>
    {
        public JsonQueryImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        public new class Fixture : JsonQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
