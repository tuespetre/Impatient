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

        public static bool IsExpandedGrouping(
            Expression expression,
            out Expression keyExpression,
            out EnumerableRelationalQueryExpression elementsExpression)
        {
            keyExpression = default;
            elementsExpression = default;

            if (expression is NewExpression newExpression
                && newExpression.Constructor.DeclaringType.IsGenericType(typeof(ExpandedGrouping<,>)))
            {
                keyExpression = newExpression.Arguments[0];
                elementsExpression = (EnumerableRelationalQueryExpression)newExpression.Arguments[1];

                return true;
            }

            return false;
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
