using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query
{
    public class DbFunctionsImpatientTest : DbFunctionsTestBase<NorthwindQueryImpatientFixture>
    {
        public DbFunctionsImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
        {
            Fixture.TestSqlLoggerFactory.Clear();
        }

        public override void Like_identity()
        {
            base.Like_identity();

            Fixture.AssertSql(@"
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE [c].[ContactName]
");
        }

        public override void Like_literal()
        {
            base.Like_literal();

            Fixture.AssertSql(@"
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE N'%M%'
");
        }

        public override void Like_literal_with_escape()
        {
            base.Like_literal_with_escape();

            Fixture.AssertSql(@"
SELECT COUNT(*)
FROM [Customers] AS [c]
WHERE [c].[ContactName] LIKE N'!%' ESCAPE N'!'
");
        }
    }
}
