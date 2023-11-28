using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class ComplexNavigationsCollectionsSharedTypeQueryImpatientTest : ComplexNavigationsCollectionsSharedTypeQueryRelationalTestBase<ComplexNavigationsCollectionsSharedTypeQueryImpatientTest.Fixture>
    {
        public ComplexNavigationsCollectionsSharedTypeQueryImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        /*protected override QueryAsserter CreateQueryAsserter(Fixture fixture) =>
            new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);*/

        public class Fixture : ComplexNavigationsSharedTypeQueryRelationalFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}