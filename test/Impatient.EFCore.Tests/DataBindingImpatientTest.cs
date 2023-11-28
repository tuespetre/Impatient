using Microsoft.EntityFrameworkCore;

namespace Impatient.EFCore.Tests
{
    public class DataBindingImpatientTest : DataBindingTestBase<F1ImpatientFixture>
    {
        public DataBindingImpatientTest(F1ImpatientFixture fixture) : base(fixture)
        {
        }
    }
}
