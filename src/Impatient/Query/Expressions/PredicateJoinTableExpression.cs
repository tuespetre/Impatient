using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public abstract class PredicateJoinTableExpression : JoinTableExpression
    {
        public PredicateJoinTableExpression(
            TableExpression outerTable,
            AliasedTableExpression innerTable,
            Expression predicate,
            Type type)
            : base(outerTable, innerTable, type)
        {
            Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        public Expression Predicate { get; }

        protected abstract PredicateJoinTableExpression Recreate(
            TableExpression outerTable,
            AliasedTableExpression innerTable,
            Expression predicate,
            Type type);

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var outerTable = visitor.VisitAndConvert(OuterTable, nameof(VisitChildren));
            var innerTable = visitor.VisitAndConvert(InnerTable, nameof(VisitChildren));
            var predicate = visitor.Visit(Predicate);

            if (outerTable != OuterTable || innerTable != InnerTable || predicate != Predicate)
            {
                if (outerTable != OuterTable)
                {
                    var oldTables = OuterTable.Flatten().ToArray();
                    var newTables = outerTable.Flatten().ToArray();

                    var updater = new TableUpdatingExpressionVisitor(oldTables, newTables);

                    innerTable = updater.VisitAndConvert(innerTable, nameof(VisitChildren));
                    predicate = updater.Visit(predicate);
                }

                if (innerTable != InnerTable)
                {
                    var oldTables = InnerTable.Flatten().ToArray();
                    var newTables = innerTable.Flatten().ToArray();

                    var updater = new TableUpdatingExpressionVisitor(oldTables, newTables);

                    predicate = updater.Visit(predicate);
                }

                return Recreate(outerTable, innerTable, predicate, Type);
            }

            return this;
        }
    }
}
