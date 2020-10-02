using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

namespace Impatient.EFCore.Tests
{
    public class TPTTableSplittingImpatientTest : TPTTableSplittingTestBase
    {
        public TPTTableSplittingImpatientTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
