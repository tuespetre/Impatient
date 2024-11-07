using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Impatient.EFCore.Tests;

public class TPTTableSplittingImpatientTest : TPTTableSplittingTestBase
{
    public TPTTableSplittingImpatientTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

    // From efcore/test/EFCore.SqlServer.FunctionalTests/TPTTableSplittingSqlServerTest.cs
    public override Task Can_insert_dependent_with_just_one_parent()
        // This scenario is not valid for TPT
        => Task.CompletedTask;
}
