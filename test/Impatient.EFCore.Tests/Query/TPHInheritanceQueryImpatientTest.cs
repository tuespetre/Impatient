using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;
using Xunit.Abstractions;

namespace Impatient.EFCore.Tests.Query
{
    public class TPHInheritanceQueryImpatientTest : TPHInheritanceQueryTestBase<TPHInheritanceQueryImpatientTest.Fixture>
    {
        public TPHInheritanceQueryImpatientTest(Fixture fixture, ITestOutputHelper testOutputHelper) : base(fixture, testOutputHelper)
        {
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void Casting_to_base_type_joining_with_query_type_works()
        {
            base.Casting_to_base_type_joining_with_query_type_works();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void FromSql_on_derived()
        {
            base.FromSql_on_derived();
        }

        [Fact(Skip = EFCoreSkipReasons.FromSql)]
        public override void FromSql_on_root()
        {
            base.FromSql_on_root();
        }

        public new class Fixture : TPHInheritanceQueryFixture
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
