using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Optimizing
{
    public class QueryableInliningExpressionVisitor : PartialEvaluatingExpressionVisitor
    {
        private readonly IQueryProvider queryProvider;
        private readonly ExpressionVisitor replacingVisitor;

        public QueryableInliningExpressionVisitor(
            IQueryProvider queryProvider,
            IReadOnlyDictionary<object, ParameterExpression> parameterMapping)
        {
            this.queryProvider = queryProvider ?? throw new ArgumentNullException(nameof(queryProvider));

            if (parameterMapping == null)
            {
                throw new ArgumentNullException(nameof(parameterMapping));
            }

            replacingVisitor
                = new ExpressionReplacingExpressionVisitor(
                    parameterMapping.ToDictionary(
                        kvp => kvp.Value as Expression,
                        kvp => Expression.Constant(kvp.Key) as Expression));
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return null;
            }

            // The replacing visitor is run on the node only if it is an IQueryable
            // so that IQueryables coming from a closure can be properly swapped in
            // and taken for their expressions. If the replacing visitor were run on
            // all nodes, actual parameters (think `where customer.Id == <closure>.customerId`)
            // would be replaced, which messes up parameterization and query caching.

            if (typeof(IQueryable).IsAssignableFrom(node.Type))
            {
                var evaluated = base.Visit(replacingVisitor.Visit(node)) as ConstantExpression;

                if (evaluated?.Value is IQueryable queryable && queryable.Provider == queryProvider)
                {
                    return queryable.Expression;
                }
            }

            return base.Visit(node);
        }
    }
}
