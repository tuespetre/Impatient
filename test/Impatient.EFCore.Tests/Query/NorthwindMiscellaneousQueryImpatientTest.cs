using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindMiscellaneousQueryImpatientTest : NorthwindMiscellaneousQueryRelationalTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindMiscellaneousQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
    }

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Using_string_Equals_with_StringComparison_throws_informative_error(bool async)
    {
        return base.Using_string_Equals_with_StringComparison_throws_informative_error(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Using_static_string_Equals_with_StringComparison_throws_informative_error(bool async)
    {
        return base.Using_static_string_Equals_with_StringComparison_throws_informative_error(async);
    }
}
