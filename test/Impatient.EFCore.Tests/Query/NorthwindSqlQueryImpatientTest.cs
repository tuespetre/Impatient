using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System;
using System.Data.Common;

namespace Impatient.EFCore.Tests.Query
{
    public class NorthwindSqlQueryImpatientTest : NorthwindSqlQueryTestBase<NorthwindSqlQueryImpatientTest.Fixture>
    {
        public NorthwindSqlQueryImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        protected override DbParameter CreateDbParameter(string name, object value)
        {
            throw new NotImplementedException();
        }

        public new class Fixture : NorthwindQueryRelationalFixture<NoopModelCustomizer>
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
