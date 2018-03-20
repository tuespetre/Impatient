using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.Infrastructure
{
    internal static class KeyPlaceholderGrouping
    {
        public static MemberInitExpression Create(Expression expression, Expression keySelector)
        {
            var typeArguments
                = expression.Type.IsGenericType(typeof(IGrouping<,>))
                    ? expression.Type.GenericTypeArguments
                    : new[] { keySelector.Type, typeof(object) };

            var groupingType
                = typeof(KeyPlaceholderGrouping<,>)
                    .MakeGenericType(typeArguments);

            return Expression.MemberInit(
                Expression.New(groupingType),
                Expression.Bind(
                    groupingType.GetRuntimeProperty("Key"),
                    keySelector));
        }
    }

    internal class KeyPlaceholderGrouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        public TKey Key { get; set; }

        public IEnumerator<TElement> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
