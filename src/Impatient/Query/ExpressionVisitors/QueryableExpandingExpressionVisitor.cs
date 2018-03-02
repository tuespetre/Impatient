using Impatient.Query.ExpressionVisitors.Optimizing;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors
{
    public class QueryableExpandingExpressionVisitor : PartialEvaluatingExpressionVisitor
    {
        private readonly ImpatientQueryProvider provider;
        private readonly ExpressionVisitor replacingVisitor;

        public QueryableExpandingExpressionVisitor(
            ImpatientQueryProvider provider,
            IDictionary<object, ParameterExpression> mapping)
        {
            this.provider = provider ?? throw new ArgumentNullException(nameof(provider));

            if (mapping == null)
            {
                throw new ArgumentNullException(nameof(mapping));
            }

            replacingVisitor
                = new ExpressionReplacingExpressionVisitor(
                    mapping.ToDictionary(
                        kvp => kvp.Value as Expression,
                        kvp => Expression.Constant(kvp.Key) as Expression));
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return null;
            }

            if (typeof(IQueryable).IsAssignableFrom(node.Type))
            {
                var evaluated = base.Visit(replacingVisitor.Visit(node)) as ConstantExpression;

                if (evaluated?.Value is IQueryable queryable && queryable.Provider == provider)
                {
                    return queryable.Expression;
                }
            }

            return base.Visit(node);
        }
    }
}
