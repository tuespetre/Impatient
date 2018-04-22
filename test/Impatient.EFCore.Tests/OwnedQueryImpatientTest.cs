using Impatient.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Impatient.EFCore.Tests
{
    // These tests seem to be broken at the fixture level
    public class OwnedQueryImpatientTest : OwnedQueryTestBase, IClassFixture<OwnedQueryImpatientFixture>
    {
        private readonly OwnedQueryImpatientFixture fixture;

        public OwnedQueryImpatientTest(OwnedQueryImpatientFixture fixture)
        {
            this.fixture = fixture;
        }

        protected override DbContext CreateContext()
        {
            return fixture.CreateContext();
        }

        [Fact(Skip = EFCoreSkipReasons.TestMaybeBroken)]
        public override void Query_for_base_type_loads_all_owned_navs()
        {
            base.Query_for_base_type_loads_all_owned_navs();
        }

        [Fact(Skip = EFCoreSkipReasons.TestMaybeBroken)]
        public override void Query_for_branch_type_loads_all_owned_navs()
        {
            base.Query_for_branch_type_loads_all_owned_navs();
        }

        [Fact(Skip = EFCoreSkipReasons.TestMaybeBroken)]
        public override void Query_for_leaf_type_loads_all_owned_navs()
        {
            base.Query_for_leaf_type_loads_all_owned_navs();
        }

        [Fact(Skip = EFCoreSkipReasons.TestMaybeBroken)]
        public override void Query_when_group_by()
        {
            base.Query_when_group_by();
        }

        [Fact(Skip = EFCoreSkipReasons.TestMaybeBroken)]
        public override void Query_when_subquery()
        {
            base.Query_when_subquery();
        }
    }

    public class OwnedQueryImpatientFixture : OwnedQueryFixtureBase
    {
        private readonly DbContextOptions options;

        public OwnedQueryImpatientFixture()
        {
            var services = new ServiceCollection();

            new ImpatientDbContextOptionsExtension().ApplyServices(services);
            
            options
                = new DbContextOptionsBuilder()
                    .UseSqlServer(@"Server=.\sqlexpress; Database=efcore-impatient-owned; Trusted_Connection=true; MultipleActiveResultSets=True")
                    .UseInternalServiceProvider(services
                        .AddEntityFrameworkSqlServer()
                        .AddImpatientEFCoreQueryCompiler()
                        .AddSingleton(TestModelSource.GetFactory(base.OnModelCreating))
                        .BuildServiceProvider())
                    .Options;

            using (var context = new DbContext(options))
            {
                // context.Database.EnsureDeleted();
                if (context.Database.EnsureCreated())
                {
                    AddTestData(context);
                }
            }
        }

        public DbContext CreateContext()
        {
            return new DbContext(options);
        }
    }
}
