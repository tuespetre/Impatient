using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class LeftJoinExpression : JoinExpression
    {
        public LeftJoinExpression(TableExpression outerTable, AliasedTableExpression innerTable, Expression predicate, Type type)
            : base(outerTable, innerTable, type)
        {
            Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        public Expression Predicate { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var outerTable = visitor.VisitAndConvert(OuterTable, nameof(VisitChildren));
            var innerTable = visitor.VisitAndConvert(InnerTable, nameof(VisitChildren));
            var predicate = visitor.Visit(Predicate);

            if (outerTable != OuterTable || innerTable != InnerTable || predicate != Predicate)
            {
                return new LeftJoinExpression(outerTable, innerTable, predicate, Type);
            }

            return this;
        }
    }
}
