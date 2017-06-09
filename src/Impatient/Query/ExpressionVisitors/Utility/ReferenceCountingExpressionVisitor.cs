using System;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    public class ReferenceCountingExpressionVisitor : ExpressionVisitor
    {
        private readonly Expression targetExpression;

        public int ReferenceCount { get; private set; }

        public ReferenceCountingExpressionVisitor(Expression targetExpression)
        {
            this.targetExpression = targetExpression ?? throw new ArgumentNullException(nameof(targetExpression));
        }

        public override Expression Visit(Expression node)
        {
            if (node == targetExpression)
            {
                ReferenceCount++;
            }

            return base.Visit(node);
        }
    }
}
