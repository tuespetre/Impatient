using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class ComplexNavigationsQueryImpatientTest : ComplexNavigationsQueryRelationalTestBase<ComplexNavigationsQueryImpatientFixture>
    {
        public ComplexNavigationsQueryImpatientTest(ComplexNavigationsQueryImpatientFixture fixture) : base(fixture)
        {
        }
    }

    public class ComplexNavigationsQueryImpatientFixture : ComplexNavigationsQueryRelationalFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
