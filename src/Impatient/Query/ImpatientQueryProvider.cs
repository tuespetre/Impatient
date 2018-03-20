using Impatient.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient
{
    public class ImpatientQueryProvider : IQueryProvider
    {
        public ImpatientQueryProvider(IImpatientQueryExecutor queryExecutor)
        {
            QueryExecutor = queryExecutor ?? throw new ArgumentNullException(nameof(queryExecutor));
        }

        public IImpatientQueryExecutor QueryExecutor { get; }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (!typeof(IEnumerable<TElement>).IsAssignableFrom(expression.Type))
            {
                throw new ArgumentException("Invalid expression for CreateQuery", nameof(expression));
            }

            if (typeof(IOrderedQueryable<TElement>).IsAssignableFrom(expression.Type))
            {
                return new ImpatientOrderedQueryable<TElement>(expression, this);
            }

            return new ImpatientQueryable<TElement>(expression, this);
        }

        IQueryable IQueryProvider.CreateQuery(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            var elementType = expression.Type.GetSequenceType();

            if (elementType == null)
            {
                throw new ArgumentException("Invalid expression for CreateQuery", nameof(expression));
            }

            if (typeof(IOrderedQueryable).IsAssignableFrom(expression.Type))
            {
                var orderedQueryableType = typeof(ImpatientOrderedQueryable<>).MakeGenericType(elementType);

                return (IQueryable)Activator.CreateInstance(orderedQueryableType, expression, this);
            }

            var queryableType = typeof(ImpatientQueryable<>).MakeGenericType(elementType);

            return (IQueryable)Activator.CreateInstance(queryableType, expression, this);
        }

        object IQueryProvider.Execute(Expression expression) => QueryExecutor.Execute(this, expression);

        TResult IQueryProvider.Execute<TResult>(Expression expression) => (TResult)QueryExecutor.Execute(this, expression);

        private class ImpatientOrderedQueryable<TElement> : ImpatientQueryable<TElement>, IOrderedQueryable<TElement>
        {
            public ImpatientOrderedQueryable(Expression expression, ImpatientQueryProvider provider)
                : base(expression, provider)
            {
            }
        }
    }
}
