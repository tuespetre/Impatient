using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class ComplexNavigationsCollectionsQueryImpatientTest : ComplexNavigationsCollectionsQueryRelationalTestBase<ComplexNavigationsCollectionsQueryImpatientTest.Fixture>
    {
        public ComplexNavigationsCollectionsQueryImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        /*protected override QueryAsserter CreateQueryAsserter(Fixture fixture) =>
            new ImpatientQueryAsserter(fixture, RewriteExpectedQueryExpression, RewriteServerQueryExpression);*/

        public class Fixture : ComplexNavigationsQueryFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}