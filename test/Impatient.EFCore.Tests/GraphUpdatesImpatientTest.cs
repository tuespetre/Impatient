using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Impatient.EFCore.Tests
{
    public class GraphUpdatesImpatientTest : GraphUpdatesTestBase<GraphUpdatesImpatientFixture>
    {
        public GraphUpdatesImpatientTest(GraphUpdatesImpatientFixture fixture) : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());
    }
}
