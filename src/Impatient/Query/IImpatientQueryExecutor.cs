using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query
{
    public interface IImpatientQueryExecutor
    {
        object Execute(IQueryProvider provider, Expression expression);
    }
}
