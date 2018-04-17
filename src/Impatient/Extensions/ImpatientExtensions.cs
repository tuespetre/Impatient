using System.Collections.Generic;
using System.Linq;

namespace Impatient.Extensions
{
    internal static class ImpatientExtensions
    {
        #region Naive implementations of Enumerable operators from .NET Core 2.0

        public static IEnumerable<TSource> Append<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            yield return element;
        }

        public static IEnumerable<TSource> Prepend<TSource>(this IEnumerable<TSource> source, TSource element)
        {
            yield return element;

            foreach (var item in source)
            {
                yield return item;
            }
        }

        public static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source, int count)
        {
            return source.Reverse().Skip(count).Reverse();
        }

        public static IEnumerable<TSource> TakeLast<TSource>(this IEnumerable<TSource> source, int count)
        {
            return source.Reverse().Take(count).Reverse();
        }

        #endregion

        public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value)
        {
            key = kvp.Key;
            value = kvp.Value;
        }
    }
}
