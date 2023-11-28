using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;

namespace Impatient.EFCore.Tests
{
    public class MaterializationInterceptionImpatientTest : MaterializationInterceptionTestBase<DbContext>
    {
        public MaterializationInterceptionImpatientTest(SingletonInterceptorsTestBase<DbContext>.SingletonInterceptorsFixtureBase fixture) : base(fixture)
        {
        }

        public override SingletonInterceptorsTestBase<DbContext>.LibraryContext CreateContext(IEnumerable<ISingletonInterceptor> interceptors, bool inject)
        {
            throw new NotImplementedException();
        }
    }
}
