using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public abstract class AnnotationExpression : Expression
    {
        public AnnotationExpression(Expression expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public Expression Expression { get; }

        public override Type Type => Expression.Type;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override bool CanReduce => true;

        public override Expression Reduce() => Expression;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = visitor.Visit(Expression);

            return expression != Expression ? Recreate(expression) : this;
        }

        protected abstract AnnotationExpression Recreate(Expression expression);
    }
}
