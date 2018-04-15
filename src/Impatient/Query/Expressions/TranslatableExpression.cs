using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class TranslatableExpression : AnnotationExpression
    {
        public TranslatableExpression(Expression expression) : base(expression)
        {
        }

        public override int GetAnnotationHashCode() => 0;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = visitor.Visit(Expression);

            if (expression != Expression)
            {
                return new TranslatableExpression(expression);
            }

            return this;
        }
    }
}
