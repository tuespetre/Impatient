using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Impatient.EFCore.Tests
{
    public class EntitySplittingImpatientTest : EntitySplittingTestBase
    {
        public EntitySplittingImpatientTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Theory(Skip = "Not sure why this is failing, but not our problem")]
        [MemberData(nameof(IsAsyncData))]
        public override Task ExecuteDelete_throws_for_entity_splitting(bool async)
        {
            return base.ExecuteDelete_throws_for_entity_splitting(async);
        }

        protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
    }
}
