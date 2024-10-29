using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests;

public class ConferencePlannerImpatientTest : ConferencePlannerTestBase<ConferencePlannerImpatientTest.ConferencePlannerFixture>
{
    public ConferencePlannerImpatientTest(ConferencePlannerFixture fixture) : base(fixture)
    {
    }

    // had to add this override for the tests to work...
    protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    public class ConferencePlannerFixture : ConferencePlannerFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
