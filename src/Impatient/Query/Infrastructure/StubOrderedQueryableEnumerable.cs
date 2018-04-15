using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Infrastructure
{
    internal class StubOrderedQueryableEnumerable<T> : IOrderedQueryableEnumerable<T>
    {
        private readonly IQueryable<T> source;

        public StubOrderedQueryableEnumerable(IQueryable<T> source) => this.source = source;

        public Type ElementType => source.ElementType;

        public Expression Expression => source.Expression;

        public IQueryProvider Provider => source.Provider;

        public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool descending)
        {
            if (descending)
            {
                return source.AsEnumerable().OrderByDescending(keySelector, comparer);
            }
            else
            {
                return source.AsEnumerable().OrderBy(keySelector, comparer);
            }
        }

        public IEnumerator<T> GetEnumerator() => source.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => source.GetEnumerator();
    }
}
