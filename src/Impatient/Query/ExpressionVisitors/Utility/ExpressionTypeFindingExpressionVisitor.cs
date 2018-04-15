using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    public class ExpressionTypeFindingExpressionVisitor<TExpression> : ExpressionVisitor
        where TExpression : Expression
    {
        public bool FoundExpressionType { get; private set; }

        public override Expression Visit(Expression node)
        {
            if (FoundExpressionType)
            {
                return node;
            }

            switch (node)
            {
                case TExpression _:
                {
                    FoundExpressionType = true;

                    return node;
                }

                default:
                {

                    return base.Visit(node);
                }
            }
        }
    }
}
