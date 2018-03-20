using System;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    /// <summary>
    /// An <see cref="ExpressionVisitor"/> that, given a target <see cref="Expression"/>,
    /// will count references to it in the visited expression tree. This visitor is
    /// stateful and should be instantiated for a single use.
    /// </summary>
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
