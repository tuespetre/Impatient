using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;

namespace Impatient.EFCore.Tests
{
    public class InheritanceImpatientTest : InheritanceTestBase<ImpatientTestStore, InheritanceImpatientFixture>
    {
        public InheritanceImpatientTest(InheritanceImpatientFixture fixture) : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());
    }
}
