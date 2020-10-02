using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class InheritanceImpatientTest : InheritanceRelationalQueryTestBase<InheritanceImpatientTest.Fixture>
    {
        public InheritanceImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
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

        public class Fixture : InheritanceQueryRelationalFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
