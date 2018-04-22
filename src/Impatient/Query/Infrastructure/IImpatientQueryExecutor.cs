using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public interface IImpatientQueryExecutor
    {
        object Execute(IQueryProvider provider, Expression expression);
    }
}
