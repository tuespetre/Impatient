using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Impatient.EFCore.Tests
{
    public class FindImpatientTest : FindTestBase<FindImpatientFixture>
    {
        public FindImpatientTest(FindImpatientFixture fixture) : base(fixture)
        {
        }

        protected override TEntity Find<TEntity>(DbContext context, params object[] keyValues)
        {
            return context.Find<TEntity>(keyValues);
        }

        protected override Task<TEntity> FindAsync<TEntity>(DbContext context, params object[] keyValues)
        {
            return context.FindAsync<TEntity>(keyValues);
        }
    }
}
