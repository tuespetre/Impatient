using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class DbFunctionsImpatientTest : NorthwindDbFunctionsQueryRelationalTestBase<NorthwindQueryImpatientFixture>
    {
        public DbFunctionsImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        protected override string CaseInsensitiveCollation => throw new System.NotImplementedException();

        protected override string CaseSensitiveCollation => throw new System.NotImplementedException();
    }
}
