using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class CrossApplyExpression : JoinExpression
    {
        public CrossApplyExpression(TableExpression outerTable, AliasedTableExpression innerTable, Type type)
            : base(outerTable, innerTable, type)
        {
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var outerTable = visitor.VisitAndConvert(OuterTable, nameof(VisitChildren));
            var innerTable = visitor.VisitAndConvert(InnerTable, nameof(VisitChildren));

            if (outerTable != OuterTable || innerTable != InnerTable)
            {
                if (outerTable != OuterTable)
                {
                    var oldTables = OuterTable.Flatten().Cast<Expression>();
                    var newTables = outerTable.Flatten().Cast<Expression>();

                    var replacingVisitor
                        = new ExpressionReplacingExpressionVisitor(
                            oldTables.Zip(newTables, ValueTuple.Create)
                                .ToDictionary(t => t.Item1, t => t.Item2));

                    innerTable = replacingVisitor.VisitAndConvert(innerTable, nameof(VisitChildren));
                }

                return new CrossApplyExpression(outerTable, innerTable, Type);
            }

            return this;
        }
    }
}
