using Impatient.Extensions;
using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient
{
    public class ImpatientQueryProvider : IQueryProvider
    {
        private readonly IImpatientQueryProcessor queryProcessor;

        public ImpatientQueryProvider(IImpatientQueryProcessor queryProcessor)
        {
            this.queryProcessor = queryProcessor ?? throw new ArgumentNullException(nameof(queryProcessor));
        }

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

        object IQueryProvider.Execute(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            return queryProcessor.Execute(this, expression);
        }

        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            return (TResult)queryProcessor.Execute(this, expression);
        }

        private class ImpatientOrderedQueryable<TElement> : ImpatientQueryable<TElement>, IOrderedQueryable<TElement>
        {
            public ImpatientOrderedQueryable(Expression expression, ImpatientQueryProvider provider)
                : base(expression, provider)
            {
            }
        }
    }
}
