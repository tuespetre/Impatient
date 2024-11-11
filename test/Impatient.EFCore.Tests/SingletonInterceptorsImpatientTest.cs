using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Impatient.EFCore.Tests;

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
