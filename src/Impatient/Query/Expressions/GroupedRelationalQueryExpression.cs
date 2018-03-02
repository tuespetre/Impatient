using Impatient.Query.ExpressionVisitors;
using System;
using System.Linq;
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
            var selectExpression = visitor.VisitAndConvert(SelectExpression, nameof(VisitChildren));
            var outerKeySelector = visitor.VisitAndConvert(OuterKeySelector, nameof(VisitChildren));
            var innerKeySelector = visitor.VisitAndConvert(InnerKeySelector, nameof(VisitChildren));
            var innerKeyLambda = visitor.VisitAndConvert(InnerKeyLambda, nameof(VisitChildren));

            if (selectExpression != SelectExpression)
            {
                var oldTables = SelectExpression.Table.Flatten().ToArray();
                var newTables = selectExpression.Table.Flatten().ToArray();

                var updater = new TableUpdatingExpressionVisitor(oldTables, newTables);

                outerKeySelector = updater.VisitAndConvert(outerKeySelector, nameof(VisitChildren));
                innerKeySelector = updater.VisitAndConvert(innerKeySelector, nameof(VisitChildren));
            }

            if (selectExpression != SelectExpression
                || outerKeySelector != OuterKeySelector
                || innerKeySelector != InnerKeySelector
                || innerKeyLambda != InnerKeyLambda)
            {
                return new GroupedRelationalQueryExpression(
                    selectExpression,
                    outerKeySelector,
                    innerKeySelector,
                    innerKeyLambda,
                    RequiresDenullification,
                    Type);
            }

            return this;
        }
    }
}
