using Impatient.EFCore.Tests.Fixtures;
using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindDbFunctionsImpatientTest : NorthwindDbFunctionsQueryRelationalTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindDbFunctionsImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
        Fixture.TestSqlLoggerFactory.Clear();
    }

    protected override string CaseInsensitiveCollation => "Latin1_General_CI_AI";

    protected override string CaseSensitiveCollation => "Latin1_General_CS_AS";
}
