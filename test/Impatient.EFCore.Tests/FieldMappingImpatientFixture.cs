using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class FieldMappingImpatientFixture : FieldMappingImpatientTest.FieldMappingFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
