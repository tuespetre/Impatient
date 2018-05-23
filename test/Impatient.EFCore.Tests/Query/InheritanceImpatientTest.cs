using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class InheritanceImpatientTest : InheritanceRelationalTestBase<InheritanceImpatientFixture>
    {
        public InheritanceImpatientTest(InheritanceImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void FromSql_on_derived()
        {
            base.FromSql_on_derived();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void FromSql_on_root()
        {
            base.FromSql_on_root();
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());
    }
}
