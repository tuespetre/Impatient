using Impatient.EFCore.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Impatient.EFCore.Tests
{
    public class NotificationEntitiesImpatientTest : NotificationEntitiesTestBase<NotificationEntitiesImpatientTest.NotificationEntitiesImpatientFixture>
    {
        public NotificationEntitiesImpatientTest(NotificationEntitiesImpatientFixture fixture) : base(fixture)
        {
        }

        public class NotificationEntitiesImpatientFixture : NotificationEntitiesFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory => ImpatientTestStoreFactory.Instance;
        }
    }
}
