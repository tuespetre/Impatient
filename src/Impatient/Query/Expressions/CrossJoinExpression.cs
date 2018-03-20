using Impatient.Query.ExpressionVisitors;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Linq;
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
                if (outerTable != OuterTable)
                {
                    var oldTables = OuterTable.Flatten().ToArray();
                    var newTables = outerTable.Flatten().ToArray();

                    var updater = new TableUpdatingExpressionVisitor(oldTables, newTables);

                    innerTable = updater.VisitAndConvert(innerTable, nameof(VisitChildren));
                }

                return new CrossJoinExpression(outerTable, innerTable, Type);
            }

            return this;
        }
    }
}
