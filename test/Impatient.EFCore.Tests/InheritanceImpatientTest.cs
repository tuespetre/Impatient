using Microsoft.EntityFrameworkCore.Query;

namespace Impatient.EFCore.Tests
{
    public class InheritanceImpatientTest : InheritanceTestBase<ImpatientTestStore, InheritanceImpatientFixture>
    {
        public InheritanceImpatientTest(InheritanceImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
