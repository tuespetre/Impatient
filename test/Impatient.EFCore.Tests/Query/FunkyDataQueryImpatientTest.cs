using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests.Query;

public class FunkyDataQueryImpatientTest : FunkyDataQueryTestBase<FunkyDataQueryImpatientFixture>
{
    public FunkyDataQueryImpatientTest(FunkyDataQueryImpatientFixture fixture) : base(fixture)
    {
    }

    public override async Task String_ends_with_on_argument_with_wildcard_constant(bool async)
    {
        /*
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith("%r")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.EndsWith("%r")) == true).Select(c => c.FirstName));
        
        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith("r_")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.EndsWith("r_")) == true).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith(null)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => false).Select(c => c.FirstName),
            assertEmpty: true);

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith("")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName != null).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.EndsWith("_r_")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.EndsWith("_r_")) == true).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.EndsWith("")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.EndsWith("")) != true).Select(c => c.FirstName));

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.EndsWith(null)).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => true).Select(c => c.FirstName));
        */

        await AssertQuery(
            async,
            ss => ss.Set<FunkyCustomer>().Where(c => !c.FirstName.EndsWith("a%r%")).Select(c => c.FirstName),
            ss => ss.Set<FunkyCustomer>().Where(c => c.FirstName.MaybeScalar(x => x.EndsWith("a%r%")) != true).Select(c => c.FirstName));
    }
}

public class FunkyDataQueryImpatientFixture : FunkyDataQueryTestBase<FunkyDataQueryImpatientFixture>.FunkyDataQueryFixtureBase
{
    protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

    public override FunkyDataContext CreateContext()
    {
        var context = base.CreateContext();
        context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        return context;
    }
}
