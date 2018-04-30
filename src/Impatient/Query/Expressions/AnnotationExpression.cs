using Impatient.Query.Infrastructure;
using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public abstract class AnnotationExpression : Expression, ISemanticHashCodeProvider
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

        public virtual int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            return comparer.GetHashCode(Expression);
        }
    }
}
