using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.FunkyDataModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Linq;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class FunkyDataQueryImpatientTest : FunkyDataQueryTestBase<FunkyDataQueryImpatientFixture>
    {
        public FunkyDataQueryImpatientTest(FunkyDataQueryImpatientFixture fixture) : base(fixture)
        {
        }

        // TODO: this
        /*
        [Fact]
        public override void String_starts_with_on_argument_with_wildcard_constant()
        {
            using (var ctx = CreateContext())
            {
                var result1 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith("%B")).Select(c => c.FirstName).ToList();
                var expected1 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.StartsWith("%B"));
                Assert.True(expected1.Count() == result1.Count);

                var result2 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith("a_")).Select(c => c.FirstName).ToList();
                var expected2 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.StartsWith("a_"));
                Assert.True(expected2.Count() == result2.Count);

                var result3 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith(null)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result3.Count);

                var result4 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith("")).Select(c => c.FirstName).ToList();
                Assert.True(ctx.FunkyCustomers.Count() == result4.Count);

                var result5 = ctx.FunkyCustomers.Where(c => c.FirstName.StartsWith("_Ba_")).Select(c => c.FirstName).ToList();
                var expected5 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.StartsWith("_Ba_"));
                Assert.True(expected5.Count() == result5.Count);

                var result6 = ctx.FunkyCustomers.Where(c => !c.FirstName.StartsWith("%B%a%r")).Select(c => c.FirstName).ToList();
                var expected6 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && !c.StartsWith("%B%a%r"));
                Assert.True(expected6.Count() == result6.Count);

                var result7 = ctx.FunkyCustomers.Where(c => !c.FirstName.StartsWith("")).Select(c => c.FirstName).ToList();
                Assert.True(0 == result7.Count);

                var result8 = ctx.FunkyCustomers.Where(c => !c.FirstName.StartsWith(null)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result8.Count);
            }
        }

        [Fact]
        public override void String_contains_on_argument_with_wildcard_constant()
        {
            using (var ctx = CreateContext())
            {
                var result1 = ctx.FunkyCustomers.Where(c => c.FirstName.Contains("%B")).Select(c => c.FirstName).ToList();
                var expected1 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.Contains("%B"));
                Assert.True(expected1.Count() == result1.Count);

                var result2 = ctx.FunkyCustomers.Where(c => c.FirstName.Contains("a_")).Select(c => c.FirstName).ToList();
                var expected2 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.Contains("a_"));
                Assert.True(expected2.Count() == result2.Count);

                var result3 = ctx.FunkyCustomers.Where(c => c.FirstName.Contains(null)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result3.Count);

                var result4 = ctx.FunkyCustomers.Where(c => c.FirstName.Contains("")).Select(c => c.FirstName).ToList();
                Assert.True(ctx.FunkyCustomers.Count() == result4.Count);

                var result5 = ctx.FunkyCustomers.Where(c => c.FirstName.Contains("_Ba_")).Select(c => c.FirstName).ToList();
                var expected5 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && c.Contains("_Ba_"));
                Assert.True(expected5.Count() == result5.Count);

                var result6 = ctx.FunkyCustomers.Where(c => !c.FirstName.Contains("%B%a%r")).Select(c => c.FirstName).ToList();
                var expected6 = ctx.FunkyCustomers.Select(c => c.FirstName).ToList().Where(c => c != null && !c.Contains("%B%a%r"));
                Assert.True(expected6.Count() == result6.Count);

                var result7 = ctx.FunkyCustomers.Where(c => !c.FirstName.Contains("")).Select(c => c.FirstName).ToList();
                Assert.True(0 == result7.Count);

                var result8 = ctx.FunkyCustomers.Where(c => !c.FirstName.Contains(null)).Select(c => c.FirstName).ToList();
                Assert.True(0 == result8.Count);
            }
        }
        */
    }

    public class FunkyDataQueryImpatientFixture : FunkyDataQueryTestBase<FunkyDataQueryImpatientFixture>.FunkyDataQueryFixtureBase
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;

        public override FunkyDataContext CreateContext()
        {
            var context = base.CreateContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return context;
        }
    }
}
