using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class GroupByResultExpression : Expression
    {
        public GroupByResultExpression(
            SelectExpression selectExpression,
            Expression outerKeySelector,
            Expression innerKeySelector,
            LambdaExpression innerKeyLambda,
            Expression elementSelector,
            bool isDistinct)
        {
            SelectExpression = selectExpression ?? throw new ArgumentNullException(nameof(selectExpression));
            OuterKeySelector = outerKeySelector ?? throw new ArgumentNullException(nameof(outerKeySelector));
            InnerKeySelector = innerKeySelector ?? throw new ArgumentNullException(nameof(innerKeySelector));
            InnerKeyLambda = innerKeyLambda ?? throw new ArgumentNullException(nameof(innerKeyLambda));
            ElementSelector = elementSelector ?? throw new ArgumentNullException(nameof(elementSelector));
            IsDistinct = isDistinct;

            Type = typeof(IGrouping<,>).MakeGenericType(innerKeySelector.Type, elementSelector.Type);
        }

        public SelectExpression SelectExpression { get; }

        public Expression OuterKeySelector { get; }

        public Expression InnerKeySelector { get; }

        public LambdaExpression InnerKeyLambda { get; }

        public Expression ElementSelector { get; }

        public bool IsDistinct { get; }

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            // VisitChildren must leave SelectExpression, InnerKeySelector, and ElementSelector alone
            // or else they may end up pointing to new table expression instances, and then when
            // an aggregation based on the GroupByResultExpression is rewritten into a SqlAggregateExpression
            // it will be referencing a disconnected/orphaned table expression instance.
            // There should be nothing that would need to legitimately be rewritten within
            // those properties anyways; that all would have been done before the GroupByResultExpression
            // was constructed, and anything that happens later will be after it has been
            // replaced by a GroupedRelationalQueryExpression and the aggregate concern is no longer relevant.

            var outerKeySelector = visitor.VisitAndConvert(OuterKeySelector, nameof(VisitChildren));
            var innerKeyLambda = visitor.VisitAndConvert(InnerKeyLambda, nameof(VisitChildren));
            
            if (outerKeySelector != OuterKeySelector || innerKeyLambda != InnerKeyLambda)
            {
                return new GroupByResultExpression(
                    SelectExpression,
                    outerKeySelector,
                    InnerKeySelector,
                    innerKeyLambda,
                    ElementSelector,
                    IsDistinct);
            }

            return this;
        }
    }
}
