using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class ToSqlQueryImpatientTest : ToSqlQueryTestBase
{
    protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

    [Theory(Skip = EFCoreSkipReasons.FromSql)]
    public override Task Entity_type_with_navigation_mapped_to_SqlQuery(bool async)
    {
        return base.Entity_type_with_navigation_mapped_to_SqlQuery(async);
    }
}
