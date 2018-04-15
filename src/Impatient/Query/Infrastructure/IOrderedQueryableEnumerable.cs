using System.Linq;

namespace Impatient.Query.Infrastructure
{
    public interface IOrderedQueryableEnumerable<T> : IOrderedEnumerable<T>, IOrderedQueryable<T>
    {
    }
}
