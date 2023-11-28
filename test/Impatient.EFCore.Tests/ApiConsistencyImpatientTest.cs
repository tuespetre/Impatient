using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace Impatient.EFCore.Tests
{
    public class ApiConsistencyImpatientTest : ApiConsistencyTestBase<ApiConsistencyImpatientTest.Fixture>
    {
        public ApiConsistencyImpatientTest(Fixture fixture) : base(fixture)
        {
        }

        protected override Assembly TargetAssembly => throw new NotImplementedException();

        protected override void AddServices(ServiceCollection serviceCollection)
        {
            throw new NotImplementedException();
        }

        public new class Fixture : ApiConsistencyFixtureBase
        {
        }
    }
}
