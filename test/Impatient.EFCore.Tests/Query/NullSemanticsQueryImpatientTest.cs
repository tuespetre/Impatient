using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestModels.NullSemanticsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Impatient.EFCore.Tests.Query
{
    public class NullSemanticsQueryImpatientTest : NullSemanticsQueryTestBase<NullSemanticsQueryImpatientFixture>
    {
        public NullSemanticsQueryImpatientTest(NullSemanticsQueryImpatientFixture fixture) : base(fixture)
        {
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void From_sql_composed_with_relational_null_comparison()
        {
            base.From_sql_composed_with_relational_null_comparison();
        }

        protected override NullSemanticsContext CreateContext(bool useRelationalNulls = false)
        {
            var options = new DbContextOptionsBuilder(Fixture.CreateOptions());

            if (useRelationalNulls)
            {
                new SqlServerDbContextOptionsBuilder(options).UseRelationalNulls();
            }

            var context = new NullSemanticsContext(options.Options);

            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            return context;
        }
    }

    public class NullSemanticsQueryImpatientFixture : NullSemanticsQueryRelationalFixture
    {
        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
