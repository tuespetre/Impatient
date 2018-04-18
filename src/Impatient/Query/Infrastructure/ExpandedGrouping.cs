using Impatient.Extensions;
using Impatient.Query.Expressions;
using System;
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
            Type type,
            Expression keyExpression,
            EnumerableRelationalQueryExpression elementsExpression)
        {
            if (type.IsGenericType(typeof(IGrouping<,>)))
            {
                var expandedGroupingType
                    = typeof(ExpandedGrouping<,>)
                        .MakeGenericType(type.GenericTypeArguments);

                return Expression.New(
                    expandedGroupingType.GetTypeInfo().DeclaredConstructors.Single(),
                    new[] { keyExpression, elementsExpression },
                    new[]
                    {
                        expandedGroupingType.GetRuntimeProperty("Key"),
                        expandedGroupingType.GetRuntimeProperty("Elements")
                    });
            }
            else
            {
                return elementsExpression;
            }
        }
    }

    internal class ExpandedGrouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        public ExpandedGrouping(TKey key, List<TElement> elements)
        {
            Key = key; // The key may very well be null
            Elements = elements ?? throw new ArgumentNullException(nameof(elements));
        }

        public TKey Key { get; }

        public List<TElement> Elements { get; }

        public IEnumerator<TElement> GetEnumerator() => Elements.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => Elements.GetEnumerator();
    }
}
