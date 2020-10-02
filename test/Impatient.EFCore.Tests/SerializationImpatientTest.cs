using Microsoft.EntityFrameworkCore;

namespace Impatient.EFCore.Tests
{
    public class SerializationImpatientTest : SerializationTestBase<F1ImpatientFixture>
    {
        public SerializationImpatientTest(F1ImpatientFixture fixture) : base(fixture)
        {
            fixture.TestSqlLoggerFactory.Clear();
        }
    }
}
