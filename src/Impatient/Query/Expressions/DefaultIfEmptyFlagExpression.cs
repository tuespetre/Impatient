using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class DefaultIfEmptyFlagExpression : AnnotationExpression
    {
        public DefaultIfEmptyFlagExpression(Expression expression) 
            : base(expression)
        {
        }

        protected override AnnotationExpression Recreate(Expression expression)
        {
            return new DefaultIfEmptyFlagExpression(expression);
        }
    }
}
