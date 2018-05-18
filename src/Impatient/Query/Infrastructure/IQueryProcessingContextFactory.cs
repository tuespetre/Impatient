using Impatient.Metadata;
using System.Linq;

namespace Impatient.Query.Infrastructure
{
    public interface IQueryProcessingContextFactory
    {
        QueryProcessingContext CreateQueryProcessingContext(IQueryProvider queryProvider);
    }
}
