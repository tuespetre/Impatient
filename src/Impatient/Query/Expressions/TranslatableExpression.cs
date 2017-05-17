using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class TranslatableExpression : AnnotationExpression
    {
        public TranslatableExpression(Expression expression) : base(expression)
        {
        }

        protected override AnnotationExpression Recreate(Expression expression) => new TranslatableExpression(expression);
    }
}
