using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class GroupByResultExpression : Expression
    {
        public GroupByResultExpression(
            SelectExpression selectExpression,
            Expression exteriorKeySelector,
            Expression interiorKeySelector,
            Expression elementSelector,
            bool isDistinct)
        {
            SelectExpression = selectExpression ?? throw new ArgumentNullException(nameof(selectExpression));
            ExteriorKeySelector = exteriorKeySelector ?? throw new ArgumentNullException(nameof(exteriorKeySelector));
            InteriorKeySelector = interiorKeySelector ?? throw new ArgumentNullException(nameof(interiorKeySelector));
            ElementSelector = elementSelector ?? throw new ArgumentNullException(nameof(elementSelector));
            IsDistinct = isDistinct;

            Type = typeof(IGrouping<,>).MakeGenericType(interiorKeySelector.Type, elementSelector.Type);
        }

        public SelectExpression SelectExpression { get; }

        public Expression ExteriorKeySelector { get; }

        public Expression InteriorKeySelector { get; }

        public Expression ElementSelector { get; }

        public bool IsDistinct { get; }

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var selectExpression = visitor.VisitAndConvert(SelectExpression, nameof(VisitChildren));
            var exteriorKeySelector = visitor.VisitAndConvert(ExteriorKeySelector, nameof(VisitChildren));
            var interiorKeySelector = visitor.VisitAndConvert(InteriorKeySelector, nameof(VisitChildren));
            var elementSelector = visitor.VisitAndConvert(ElementSelector, nameof(VisitChildren));

            if (selectExpression != SelectExpression)
            {
                var oldTables = SelectExpression.Table.Flatten().Cast<Expression>();
                var newTables = selectExpression.Table.Flatten().Cast<Expression>();

                var replacingVisitor
                    = new ExpressionReplacingExpressionVisitor(
                        oldTables.Zip(newTables, ValueTuple.Create)
                            .ToDictionary(t => t.Item1, t => t.Item2));

                interiorKeySelector = replacingVisitor.VisitAndConvert(interiorKeySelector, nameof(VisitChildren));
                elementSelector = replacingVisitor.VisitAndConvert(elementSelector, nameof(VisitChildren));
            }

            if (selectExpression != SelectExpression
                || exteriorKeySelector != ExteriorKeySelector
                || interiorKeySelector != InteriorKeySelector
                || elementSelector != ElementSelector)
            {
                return new GroupByResultExpression(
                    selectExpression,
                    exteriorKeySelector,
                    interiorKeySelector,
                    elementSelector,
                    IsDistinct);
            }

            return this;
        }
    }
}
