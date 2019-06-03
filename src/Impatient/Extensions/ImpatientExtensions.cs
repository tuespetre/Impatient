using Impatient.Query.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace Impatient.Extensions
{
    internal static class ImpatientExtensions
    {
        #region Naive implementations of Enumerable operators from .NET Core 2.0

        public static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source, int count)
        {
            return source.Reverse().Skip(count).Reverse();
        }

        public static IEnumerable<TSource> TakeLast<TSource>(this IEnumerable<TSource> source, int count)
        {
            return source.Reverse().Take(count).Reverse();
        }

        #endregion Naive implementations of Enumerable operators from .NET Core 2.0

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