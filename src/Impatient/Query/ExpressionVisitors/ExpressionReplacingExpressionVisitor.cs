using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors
{
    public class ExpressionReplacingExpressionVisitor : ExpressionVisitor
    {
        private readonly Dictionary<Expression, Expression> mapping;

        public ExpressionReplacingExpressionVisitor(Expression target, Expression replacement)
        {
            mapping = new Dictionary<Expression, Expression>
            {
                { target, replacement }
            };
        }

        public ExpressionReplacingExpressionVisitor(IEnumerable<(Expression, Expression)> mapping)
        {
            this.mapping = mapping.ToDictionary(m => m.Item1, m => m.Item2);
        }

        public override Expression Visit(Expression node) 
            => node is null 
                ? node 
                : mapping.TryGetValue(node, out var replacement) 
                    ? replacement 
                    : base.Visit(node);
    }
}
