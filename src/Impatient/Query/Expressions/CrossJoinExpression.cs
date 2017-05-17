using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class CrossJoinExpression : JoinExpression
    {
        public CrossJoinExpression(TableExpression outerTable, AliasedTableExpression innerTable, Type type)
            : base(outerTable, innerTable, type)
        {
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var outerTable = visitor.VisitAndConvert(OuterTable, nameof(VisitChildren));
            var innerTable = visitor.VisitAndConvert(InnerTable, nameof(VisitChildren));

            if (outerTable != OuterTable || innerTable != InnerTable)
            {
                return new CrossJoinExpression(outerTable, innerTable, Type);
            }

            return this;
        }
    }
}
