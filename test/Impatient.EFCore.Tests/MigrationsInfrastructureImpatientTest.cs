using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class MigrationsInfrastructureImpatientTest : MigrationsInfrastructureTestBase<MigrationsInfrastructureImpatientTest.Fixture>
    {
        public MigrationsInfrastructureImpatientTest(Fixture fixture) : base(fixture)
        {
            fixture.ListLoggerFactory.Clear();
        }

        public override void Can_diff_against_2_1_ASP_NET_Identity_model()
        {
            throw new System.NotImplementedException();
        }

        public override void Can_diff_against_2_2_ASP_NET_Identity_model()
        {
            throw new System.NotImplementedException();
        }

        public override void Can_diff_against_2_2_model()
        {
            throw new System.NotImplementedException();
        }

        public override void Can_diff_against_3_0_ASP_NET_Identity_model()
        {
            throw new System.NotImplementedException();
        }

        public class Fixture : MigrationsInfrastructureFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
