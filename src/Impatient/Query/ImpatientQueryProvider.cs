using Impatient.Query;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient
{
    public class ImpatientQueryProvider : IQueryProvider
    {
        public ImpatientQueryProvider(
            IImpatientDbConnectionFactory connectionFactory,
            IImpatientQueryCache queryCache,
            IImpatientExpressionVisitorProvider expressionVisitorProvider)
        {
            ConnectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            QueryCache = queryCache ?? throw new ArgumentNullException(nameof(queryCache));
            ExpressionVisitorProvider = expressionVisitorProvider ?? throw new ArgumentNullException(nameof(expressionVisitorProvider));
        }

        public Action<DbCommand> DbCommandInterceptor { get; set; }

        public IImpatientDbConnectionFactory ConnectionFactory { get; }

        public IImpatientQueryCache QueryCache { get; }

        public IImpatientExpressionVisitorProvider ExpressionVisitorProvider { get; }

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
            try
            {
                var hashingVisitor = new HashingExpressionVisitor();
                expression = hashingVisitor.Visit(expression);

                var constantParameterizingVisitor = new ConstantParameterizingExpressionVisitor();
                expression = constantParameterizingVisitor.Visit(expression);

                var parameterMapping = constantParameterizingVisitor.Mapping;

                if (!QueryCache.TryGetValue(hashingVisitor.HashCode, out var compiled))
                {
                    expression = new QueryableExpandingExpressionVisitor(parameterMapping).Visit(expression);

                    expression = ExpressionVisitorProvider.OptimizingExpressionVisitors.Aggregate(expression, (e, v) => v.Visit(e));

                    expression = new QueryActivatingExpressionVisitor(this).Visit(expression);

                    expression = new QueryBuildingExpressionVisitor(this).Visit(expression);

                    if (expression is EnumerableRelationalQueryExpression possiblyOrdered)
                    {
                        expression = possiblyOrdered.AsUnordered();
                    }

                    var queryProviderParameter = Expression.Parameter(typeof(ImpatientQueryProvider), "queryProvider");
                    
                    expression = new QueryCompilingExpressionVisitor(queryProviderParameter).Visit(expression);

                    var parameters = new ParameterExpression[parameterMapping.Count + 1];

                    parameters[0] = queryProviderParameter;

                    parameterMapping.Values.CopyTo(parameters, 1);

                    compiled = Expression.Lambda(expression, parameters).Compile();

                    QueryCache.Add(hashingVisitor.HashCode, compiled);
                }

                var arguments = new object[parameterMapping.Count + 1];

                arguments[0] = this;

                parameterMapping.Keys.CopyTo(arguments, 1);

                return compiled.DynamicInvoke(arguments);
            }
            catch (TargetInvocationException targetInvocationException)
            {
                throw targetInvocationException.InnerException;
            }
        }

        TResult IQueryProvider.Execute<TResult>(Expression expression)
        {
            return (TResult)((IQueryProvider)this).Execute(expression);
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
