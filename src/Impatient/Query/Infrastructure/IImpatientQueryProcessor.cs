using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    public interface IImpatientQueryProcessor
    {
        object Execute(IQueryProvider provider, Expression expression);
    }
}
