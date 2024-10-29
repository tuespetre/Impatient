using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query;

public class ComplexNavigationsSharedTypeQueryImpatientTest : ComplexNavigationsSharedTypeQueryRelationalTestBase<ComplexNavigationsSharedTypeQueryImpatientTest.Fixture>
{
    public ComplexNavigationsSharedTypeQueryImpatientTest(Fixture fixture) : base(fixture)
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public override Task Include1(bool async)
    {
        return base.Include1(async);
    }

    protected override QueryAsserter CreateQueryAsserter(Fixture fixture) =>
        new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);

    public new class Fixture : ComplexNavigationsSharedTypeQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}