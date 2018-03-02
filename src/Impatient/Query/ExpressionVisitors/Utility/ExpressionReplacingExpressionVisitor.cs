using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    public class ExpressionReplacingExpressionVisitor : ExpressionVisitor
    {
        private readonly IDictionary<Expression, Expression> mapping;

        public ExpressionReplacingExpressionVisitor(Expression target, Expression replacement)
        {
            mapping = new Dictionary<Expression, Expression>
            {
                {
                    target ?? throw new ArgumentNullException(nameof(target)),
                    replacement ?? throw new ArgumentNullException(nameof(replacement))
                }
            };
        }

        public ExpressionReplacingExpressionVisitor(IDictionary<Expression, Expression> mapping)
        {
            this.mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
        }

        public override Expression Visit(Expression node)
            => node is null
                ? node
                : mapping.TryGetValue(node, out var replacement)
                    ? replacement
                    : base.Visit(node);
    }
}
