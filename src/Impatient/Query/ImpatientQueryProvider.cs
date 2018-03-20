using Impatient.Query;
using Impatient.Query.ExpressionVisitors.Generating;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.ExpressionVisitors.Utility;
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
                // Parameterize the expression by substituting any ConstantExpression
                // that is not a literal constant (such as a closure instance) with a ParameterExpression.

                var constantParameterizingVisitor = new ConstantParameterizingExpressionVisitor();
                expression = constantParameterizingVisitor.Visit(expression);

                // Generate a hash code for the parameterized expression.
                // Because the expression is parameterized, the hash code will be identical
                // for expressions that are structurally equivalent apart from any closure instances.

                var hashingVisitor = new HashingExpressionVisitor();
                expression = hashingVisitor.Visit(expression);

                var parameterMapping = constantParameterizingVisitor.Mapping;

                if (!QueryCache.TryGetValue(hashingVisitor.HashCode, out var compiled))
                {
                    // Partially evaluate the expression. In addition to reducing evaluable nodes such 
                    // as `new DateTime(2000, 01, 01)` down to ConstantExpressions, this visitor also expands 
                    // IQueryable-producing expressions such as those found within calls to SelectMany
                    // so that the resulting IQueryable's expression tree will be integrated into the 
                    // current expression tree.

                    expression = new QueryableInliningExpressionVisitor(this, parameterMapping).Visit(expression);

                    // Apply all optimizing visitors before each composing visitor and then apply all
                    // optimizing visitors one last time.

                    expression
                        = ExpressionVisitorProvider.ComposingExpressionVisitors
                            .SelectMany(c => ExpressionVisitorProvider.OptimizingExpressionVisitors.Append(c))
                            .Concat(ExpressionVisitorProvider.OptimizingExpressionVisitors)
                            .Aggregate(expression, (e, v) => v.Visit(e));

                    // Transform the expression by rewriting all composed query expressions into 
                    // executable expressions that make database calls and perform result materialization.

                    var queryProviderParameter = Expression.Parameter(typeof(ImpatientQueryProvider), "queryProvider");

                    expression = new QueryCompilingExpressionVisitor(ExpressionVisitorProvider, queryProviderParameter).Visit(expression);

                    // Compile the resulting expression into an executable delegate.

                    var parameters = new ParameterExpression[parameterMapping.Count + 1];

                    parameters[0] = queryProviderParameter;

                    parameterMapping.Values.CopyTo(parameters, 1);

                    compiled = Expression.Lambda(expression, parameters).Compile();

                    // Cache the compiled delegate.

                    QueryCache.Add(hashingVisitor.HashCode, compiled);
                }

                // Invoke the compiled delegate.

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
