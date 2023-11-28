using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;

namespace Impatient.EFCore.Tests
{
    public class SingletonInterceptorsImpatientTest : SingletonInterceptorsTestBase<DbContext>
    {
        public SingletonInterceptorsImpatientTest(SingletonInterceptorsFixtureBase fixture) : base(fixture)
        {
        }

        public override LibraryContext CreateContext(IEnumerable<ISingletonInterceptor> interceptors, bool inject)
        {
            throw new NotImplementedException();
        }
    }
}
