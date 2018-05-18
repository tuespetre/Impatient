using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Impatient.EFCore.Tests
{
    public class FieldMappingImpatientTest : FieldMappingTestBase<FieldMappingImpatientFixture>
    {
        public FieldMappingImpatientTest(FieldMappingImpatientFixture fixture) : base(fixture)
        {
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
            => facade.UseTransaction(transaction.GetDbTransaction());
    }
}
