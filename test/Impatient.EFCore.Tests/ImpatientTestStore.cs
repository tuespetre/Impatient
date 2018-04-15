using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace Impatient.EFCore.Tests
{
    public class ImpatientTestStore : RelationalTestStore
    {
        public override DbConnection Connection => null;

        public override DbTransaction Transaction => null;

        public override string ConnectionString => null;
    }
}
