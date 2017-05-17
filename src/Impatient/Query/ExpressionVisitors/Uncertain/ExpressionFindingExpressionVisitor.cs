using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace Impatient.Query.ExpressionVisitors.Generalized
{
    public class ExpressionFindingExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression target;

        public bool FoundExpression { get; private set; }

        public ExpressionFindingExpressionVisitor(Expression target)
        {
            this.target = target ?? throw new ArgumentNullException(nameof(target));
        }

        public override Expression Visit(Expression node)
        {
            if (FoundExpression)
            {
                return node;
            }

            if (node == target)
            {
                FoundExpression = true;
                return node;
            }

            return base.Visit(node);
        }
    }
}
