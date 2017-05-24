using Impatient.Query.ExpressionVisitors.Utility;
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
            Type type) 
            : base(selectExpression)
        {
            OuterKeySelector = outerKeySelector ?? throw new ArgumentNullException(nameof(outerKeySelector));
            InnerKeySelector = innerKeySelector ?? throw new ArgumentNullException(nameof(innerKeySelector));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public Expression OuterKeySelector { get; }

        public Expression InnerKeySelector { get; }

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var selectExpression = visitor.VisitAndConvert(SelectExpression, nameof(VisitChildren));
            var innerKeySelector = visitor.VisitAndConvert(InnerKeySelector, nameof(VisitChildren));
            var outerKeySelector = visitor.VisitAndConvert(OuterKeySelector, nameof(VisitChildren));

            if (selectExpression != SelectExpression)
            {
                var oldTables = SelectExpression.Table.Flatten().Cast<Expression>();
                var newTables = selectExpression.Table.Flatten().Cast<Expression>();

                var replacingVisitor
                    = new ExpressionReplacingExpressionVisitor(
                        oldTables.Zip(newTables, ValueTuple.Create)
                            .ToDictionary(t => t.Item1, t => t.Item2));

                innerKeySelector = replacingVisitor.VisitAndConvert(innerKeySelector, nameof(VisitChildren));
                outerKeySelector = replacingVisitor.VisitAndConvert(outerKeySelector, nameof(VisitChildren));
            }

            if (selectExpression != SelectExpression
                || outerKeySelector != OuterKeySelector
                || innerKeySelector != InnerKeySelector)
            {
                return new GroupedRelationalQueryExpression(
                    selectExpression,
                    outerKeySelector,
                    innerKeySelector,
                    Type);
            }

            return this;
        }
    }
}
