using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update;

namespace Impatient.EFCore.Tests.Update
{
    public class NonsharedModelUpdatesImpatientTest : NonSharedModelUpdatesTestBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
