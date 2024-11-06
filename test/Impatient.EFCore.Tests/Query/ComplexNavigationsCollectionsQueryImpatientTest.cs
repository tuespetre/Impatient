using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;
using static Impatient.EFCore.Tests.Query.ComplexNavigationsCollectionsQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class ComplexNavigationsCollectionsQueryImpatientTest : ComplexNavigationsCollectionsQueryRelationalTestBase<Fixture>
{
    public ComplexNavigationsCollectionsQueryImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => base.Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(Fixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : ComplexNavigationsQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Include_after_Select(bool async)
    {
        return base.Include_after_Select(async);
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Include_after_SelectMany_and_reference_navigation(bool async)
    {
        return base.Include_after_SelectMany_and_reference_navigation(async);
    }

    [Theory(Skip = EFCoreSkipReasons.ClientEval)]
    public override Task SelectMany_over_conditional_empty_source(bool async)
    {
        return base.SelectMany_over_conditional_empty_source(async);
    }
}