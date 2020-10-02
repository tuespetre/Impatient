using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class ComplexNavigationsWeakQueryImpatientTest : ComplexNavigationsWeakQueryTestBase<ComplexNavigationsWeakQueryImpatientFixture>
    {
        public ComplexNavigationsWeakQueryImpatientTest(ComplexNavigationsWeakQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }

    public class ComplexNavigationsWeakQueryImpatientFixture : ComplexNavigationsWeakQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
