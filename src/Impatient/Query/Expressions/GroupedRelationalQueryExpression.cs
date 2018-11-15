using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class GroupedRelationalQueryExpression : EnumerableRelationalQueryExpression
    {
        public GroupedRelationalQueryExpression(
            SelectExpression selectExpression,
            Expression outerKeySelector,
            Expression innerKeySelector,
            LambdaExpression innerKeyLambda,
            bool requiresDenullification,
            Type type)
            : base(selectExpression)
        {
            OuterKeySelector = outerKeySelector ?? throw new ArgumentNullException(nameof(outerKeySelector));
            InnerKeySelector = innerKeySelector ?? throw new ArgumentNullException(nameof(innerKeySelector));
            InnerKeyLambda = innerKeyLambda ?? throw new ArgumentNullException(nameof(innerKeyLambda));
            RequiresDenullification = requiresDenullification;
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public Expression OuterKeySelector { get; }

        public Expression InnerKeySelector { get; }

        public LambdaExpression InnerKeyLambda { get; }

        public bool RequiresDenullification { get; }

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var outerKeySelector = visitor.VisitAndConvert(OuterKeySelector, nameof(VisitChildren));
            var innerKeyLambda = visitor.VisitAndConvert(InnerKeyLambda, nameof(VisitChildren));

            if (outerKeySelector != OuterKeySelector
                || innerKeyLambda != InnerKeyLambda)
            {
                return new GroupedRelationalQueryExpression(
                    SelectExpression,
                    outerKeySelector,
                    InnerKeySelector,
                    innerKeyLambda,
                    RequiresDenullification,
                    Type);
            }

            return this;
        }
    }
}
