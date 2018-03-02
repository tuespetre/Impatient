using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query
{
    public class ImpatientQueryable<TElement> : IQueryable<TElement>
    {
        public ImpatientQueryable(Expression expression, ImpatientQueryProvider provider)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        public Type ElementType => typeof(TElement);

        public Expression Expression { get; }

        public IQueryProvider Provider { get; }

        IEnumerator<TElement> IEnumerable<TElement>.GetEnumerator()
            => Provider.Execute<IEnumerable<TElement>>(Expression).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Provider.Execute<IEnumerable>(Expression).GetEnumerator();
    }
}
