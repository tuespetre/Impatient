using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Generalized
{
    public class EscapedReferenceCountingExpressionVisitor : ProjectionExpressionVisitor
    {
        private readonly Expression targetExpression;

        public int EscapedReferenceCount { get; private set; }

        public EscapedReferenceCountingExpressionVisitor(Expression targetExpression)
        {
            this.targetExpression = targetExpression;
        }

        public override Expression Visit(Expression node)
        {
            if (InLeaf && node == targetExpression)
            {
                EscapedReferenceCount++;
            }

            return base.Visit(node);
        }
    }
}
