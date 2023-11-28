using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System;

namespace Impatient.EFCore.Tests.Query
{
    public class JsonQueryAdhocImpatientTest : JsonQueryAdHocTestBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

        protected override void Seed29219(MyContext29219 ctx)
        {
            throw new NotImplementedException();
        }

        protected override void Seed30028(MyContext30028 ctx)
        {
            throw new NotImplementedException();
        }

        protected override void SeedArrayOfPrimitives(MyContextArrayOfPrimitives ctx)
        {
            throw new NotImplementedException();
        }

        protected override void SeedJunkInJson(MyContextJunkInJson ctx)
        {
            throw new NotImplementedException();
        }

        protected override void SeedNotICollection(MyContextNotICollection ctx)
        {
            throw new NotImplementedException();
        }

        protected override void SeedShadowProperties(MyContextShadowProperties ctx)
        {
            throw new NotImplementedException();
        }
    }
}
