using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System;
using System.Reflection;

namespace Impatient.EFCore.Tests
{
    public class DesignTimeImpatientTest : DesignTimeTestBase<DesignTimeImpatientTest.Fixture>
    {
        public DesignTimeImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        protected override Assembly ProviderAssembly => throw new NotImplementedException();

        public new class Fixture : DesignTimeFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
