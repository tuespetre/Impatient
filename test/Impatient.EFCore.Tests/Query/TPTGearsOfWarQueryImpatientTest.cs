using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using static Impatient.EFCore.Tests.Query.TPTGearsOfWarQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class TPTGearsOfWarQueryImpatientTest(Fixture fixture) : TPTGearsOfWarQueryRelationalTestBase<Fixture>(fixture)
{
    protected override QueryAsserter CreateQueryAsserter(Fixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : TPTGearsOfWarQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    public override async Task Select_null_propagation_negative9(bool async)
    {
        await base.Select_null_propagation_negative9(async);
    }
}
