using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;
using static Impatient.EFCore.Tests.Query.ComplexNavigationsCollectionsSplitSharedTypeQueryImpatientTest;

namespace Impatient.EFCore.Tests.Query;

public class ComplexNavigationsCollectionsSplitSharedTypeQueryImpatientTest : ComplexNavigationsCollectionsSplitSharedTypeQueryRelationalTestBase<Fixture>
{
    public ComplexNavigationsCollectionsSplitSharedTypeQueryImpatientTest(Fixture fixture) : base(fixture)
    {
        fixture.TestSqlLoggerFactory.Clear();
    }

    private void AssertSql(string expected) => base.Fixture.TestSqlLoggerFactory.AssertSql(expected);

    protected override QueryAsserter CreateQueryAsserter(Fixture fixture)
        => new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : ComplexNavigationsSharedTypeQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }

    [Theory(Skip = EFCoreSkipReasons.TranslationBeyondEF)]
    public override Task Include_inside_subquery(bool async)
    {
        return base.Include_inside_subquery(async);
    }
}