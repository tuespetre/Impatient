using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using static Impatient.EFCore.Tests.Query.ComplexTypeQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class ComplexTypeQueryImpatientTest(Fixture fixture) : ComplexTypeQueryRelationalTestBase<Fixture>(fixture)
{
    public new class Fixture : ComplexTypeQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    public override Task Complex_type_equals_constant(bool async)
    {
        return base.Complex_type_equals_constant(async);
    }
}
