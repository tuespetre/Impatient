using Impatient.EFCore.Tests.Fixtures;
using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindWhereQueryImpatientTest : NorthwindWhereQueryRelationalTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindWhereQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
    }

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    // just added this override because the base test only uses InlineData(false) for some reason,
    // and Test Explorer saved playlist would then not show it for some reason?
    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Where_bitwise_xor(bool async)
    {
        return base.Where_bitwise_xor(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task Where_equals_method_string_with_ignore_case(bool async)
    {
        return base.Where_equals_method_string_with_ignore_case(async);
    }
}
