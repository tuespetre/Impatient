using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class DefaultIfEmptyExpression : AnnotationExpression
    {
        public DefaultIfEmptyExpression(Expression expression) : base(expression)
        {
            Flag = Constant(0, typeof(int?));
        }

        public DefaultIfEmptyExpression(Expression expression, Expression flag) : base(expression)
        {
            Flag = flag ?? throw new ArgumentNullException(nameof(flag));
        }

        public Expression Flag { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = visitor.Visit(Expression);
            var flag = visitor.Visit(Flag);

            if (expression != Expression || flag != Flag)
            {
                return new DefaultIfEmptyExpression(expression, flag);
            }

            return this;
        }

        public DefaultIfEmptyExpression Update(Expression expression, Expression flag)
        {
            expression = expression ?? throw new ArgumentNullException(nameof(expression));
            flag = flag ?? throw new ArgumentNullException(nameof(flag));

            if (expression != Expression || flag != Flag)
            {
                return new DefaultIfEmptyExpression(expression, flag);
            }

            return this;
        }
    }
}
