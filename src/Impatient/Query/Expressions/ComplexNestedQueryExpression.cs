using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class ComplexNestedQueryExpression : AnnotationExpression
    {
        public ComplexNestedQueryExpression(Expression queryExpression, Type type) : base(queryExpression)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public Expression OriginalExpression { get; }

        public override Type Type { get; }

        protected override AnnotationExpression Recreate(Expression expression)
        {
            return new ComplexNestedQueryExpression(expression, Type);
        }
    }
}
