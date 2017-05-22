using Impatient.Query.ExpressionVisitors.Optimizing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors
{
    public class QueryableExpandingExpressionVisitor : ExpressionVisitor
    {
        private readonly QueryableExpandingPartialEvaluatingExpressionVisitor visitor;

        public QueryableExpandingExpressionVisitor(IDictionary<object, ParameterExpression> mapping)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException(nameof(mapping));
            }

            visitor = new QueryableExpandingPartialEvaluatingExpressionVisitor(
                mapping.ToDictionary(kvp => kvp.Value, kvp => kvp.Key));
        }

        public override Expression Visit(Expression node)
        {
            if (node == null)
            {
                return null;
            }

            if (typeof(IQueryable).IsAssignableFrom(node.Type))
            {
                switch (node)
                {
                    case MethodCallExpression methodCallExpression:
                    {
                        return visitor.Visit(methodCallExpression) as ConstantExpression ?? base.Visit(node);
                    }

                    case MemberExpression memberExpression:
                    {
                        return visitor.Visit(memberExpression) as ConstantExpression ?? base.Visit(node);
                    }
                }
            }

            return base.Visit(node);
        }

        private class QueryableExpandingPartialEvaluatingExpressionVisitor : PartialEvaluatingExpressionVisitor
        {
            private readonly IDictionary<ParameterExpression, object> mapping;

            public QueryableExpandingPartialEvaluatingExpressionVisitor(IDictionary<ParameterExpression, object> mapping)
            {
                this.mapping = mapping;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (mapping.TryGetValue(node, out var value))
                {
                    return Expression.Constant(value);
                }

                return base.VisitParameter(node);
            }
        }
    }
}
