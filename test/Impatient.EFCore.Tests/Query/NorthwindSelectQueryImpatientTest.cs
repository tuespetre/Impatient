using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class NorthwindSelectQueryImpatientTest : NorthwindSelectQueryRelationalTestBase<NorthwindQueryImpatientFixture>
{
    public NorthwindSelectQueryImpatientTest(NorthwindQueryImpatientFixture fixture) : base(fixture)
    {
    }

    // We compute a default ordering.
    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    [MemberData(nameof(IsAsyncData))]
    public override Task Reverse_without_explicit_ordering(bool async)
    {
        return base.Reverse_without_explicit_ordering(async);
    }

    protected override QueryAsserter CreateQueryAsserter(NorthwindQueryImpatientFixture fixture) =>
        new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);
}
