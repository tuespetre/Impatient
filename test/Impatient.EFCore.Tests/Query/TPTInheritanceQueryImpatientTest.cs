using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query;

public class TPTInheritanceQueryImpatientTest : TPTInheritanceQueryTestBase<TPTInheritanceQueryImpatientTest.Fixture>
{
    public TPTInheritanceQueryImpatientTest(Fixture fixture, Xunit.Abstractions.ITestOutputHelper helper) : base(fixture, helper)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    public new class Fixture : TPTInheritanceQueryFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
