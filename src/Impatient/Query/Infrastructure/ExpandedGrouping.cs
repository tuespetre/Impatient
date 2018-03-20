using Impatient.Query.Expressions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.Infrastructure
{
    public static class ExpandedGrouping
    {
        public static Expression Create(
            Expression expression,
            Expression keyExpression,
            EnumerableRelationalQueryExpression elementsExpression)
        {
            if (expression.Type.IsGenericType(typeof(IGrouping<,>)))
            {
                var groupingType
                    = typeof(ExpandedGrouping<,>)
                        .MakeGenericType(expression.Type.GenericTypeArguments);

                return Expression.New(
                    groupingType.GetTypeInfo().DeclaredConstructors.Single(),
                    new[] { keyExpression, elementsExpression },
                    new[] { groupingType.GetRuntimeProperty("Key"), groupingType.GetRuntimeProperty("Elements") });
            }
            else
            {
                return elementsExpression;
            }
        }
    }

    internal class ExpandedGrouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        public ExpandedGrouping(TKey key, IEnumerable<TElement> elements)
        {
            Key = key;
            Elements = elements;
        }

        public TKey Key { get; }

        public IEnumerable<TElement> Elements { get; }

        public IEnumerator<TElement> GetEnumerator()
            => Elements?.GetEnumerator() ?? Enumerable.Empty<TElement>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Elements?.GetEnumerator() ?? Enumerable.Empty<TElement>().GetEnumerator();
    }
}
