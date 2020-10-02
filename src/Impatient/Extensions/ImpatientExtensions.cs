using Impatient.Query.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace Impatient.Extensions
{
    internal static class ImpatientExtensions
    {
        public static IOrderedQueryable<TSource> AsOrderedQueryable<TSource>(this IQueryable<TSource> source)
        {
            return new StubOrderedQueryableEnumerable<TSource>(source);
        }

        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}
