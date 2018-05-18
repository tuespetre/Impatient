using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit.Abstractions;

#pragma warning disable xUnit1024 // Test methods cannot have overloads

namespace Impatient.EFCore.Tests
{
    public class TableSplittingImpatientTest : TableSplittingTestBase
    {
        public TableSplittingImpatientTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
