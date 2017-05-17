using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class JoinPredicateExpression : AnnotationExpression
    {
        public JoinPredicateExpression(Expression expression, LambdaExpression outerKeySelector, LambdaExpression innerKeySelector) : base(expression)
        {
            OuterKeySelector = outerKeySelector ?? throw new ArgumentNullException(nameof(outerKeySelector));
            InnerKeySelector = innerKeySelector ?? throw new ArgumentNullException(nameof(innerKeySelector));
        }

        public LambdaExpression OuterKeySelector { get; }

        public LambdaExpression InnerKeySelector { get; }

        protected override AnnotationExpression Recreate(Expression expression) => new JoinPredicateExpression(expression, OuterKeySelector, InnerKeySelector);
    }
}
