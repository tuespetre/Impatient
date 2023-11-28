using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.EntityFrameworkCore.Update;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Impatient.EFCore.Tests.Update
{
    public class JsonUpdateImpatientTest : JsonUpdateTestBase<JsonUpdateImpatientTest.ConcreteFixture>
    {
        public JsonUpdateImpatientTest(ConcreteFixture fixture) : base(fixture)
        {
        }

        protected override void ClearLog() => Fixture.TestSqlLoggerFactory.Clear();

        public class ConcreteFixture : JsonUpdateFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
