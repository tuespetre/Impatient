using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class GraphUpdatesImpatientTest : GraphUpdatesTestBase<GraphUpdatesImpatientTest.Fixture>
    {
        public GraphUpdatesImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());

        public class Fixture : GraphUpdatesFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

            protected override string StoreName => throw new System.NotImplementedException();
        }
    }
}
