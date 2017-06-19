using Impatient.Query.ExpressionVisitors;
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
            var selectExpression = visitor.VisitAndConvert(SelectExpression, nameof(VisitChildren));
            var outerKeySelector = visitor.VisitAndConvert(OuterKeySelector, nameof(VisitChildren));
            var innerKeySelector = visitor.VisitAndConvert(InnerKeySelector, nameof(VisitChildren));
            var innerKeyLambda = visitor.VisitAndConvert(InnerKeyLambda, nameof(VisitChildren));
            var elementSelector = visitor.VisitAndConvert(ElementSelector, nameof(VisitChildren));

            if (selectExpression != SelectExpression)
            {
                var oldTables = SelectExpression.Table.Flatten().ToArray();
                var newTables = selectExpression.Table.Flatten().ToArray();

                var updater = new TableUpdatingExpressionVisitor(oldTables, newTables);

                innerKeySelector = updater.VisitAndConvert(innerKeySelector, nameof(VisitChildren));
                elementSelector = updater.VisitAndConvert(elementSelector, nameof(VisitChildren));
            }

            if (selectExpression != SelectExpression
                || outerKeySelector != OuterKeySelector
                || innerKeySelector != InnerKeySelector
                || innerKeyLambda != InnerKeyLambda
                || elementSelector != ElementSelector)
            {
                return new GroupByResultExpression(
                    selectExpression,
                    outerKeySelector,
                    innerKeySelector,
                    innerKeyLambda,
                    elementSelector,
                    IsDistinct);
            }

            return this;
        }
    }
}
