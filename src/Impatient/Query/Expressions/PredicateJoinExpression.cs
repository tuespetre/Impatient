using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public abstract class PredicateJoinExpression : JoinExpression
    {
        public PredicateJoinExpression(
            TableExpression outerTable, 
            AliasedTableExpression innerTable, 
            Expression predicate, 
            Type type)
            : base(outerTable, innerTable, type)
        {
            Predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        public Expression Predicate { get; }

        protected abstract PredicateJoinExpression Recreate(
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
                    var oldTables = OuterTable.Flatten().Cast<Expression>();
                    var newTables = outerTable.Flatten().Cast<Expression>();

                    var replacingVisitor
                        = new ExpressionReplacingExpressionVisitor(
                            oldTables.Zip(newTables, ValueTuple.Create)
                                .ToDictionary(t => t.Item1, t => t.Item2));

                    innerTable = replacingVisitor.VisitAndConvert(innerTable, nameof(VisitChildren));
                    predicate = replacingVisitor.Visit(predicate);
                }

                if (innerTable != InnerTable)
                {
                    var oldTables = InnerTable.Flatten().Cast<Expression>();
                    var newTables = innerTable.Flatten().Cast<Expression>();

                    var replacingVisitor
                        = new ExpressionReplacingExpressionVisitor(
                            oldTables.Zip(newTables, ValueTuple.Create)
                                .ToDictionary(t => t.Item1, t => t.Item2));

                    predicate = replacingVisitor.Visit(predicate);
                }

                return Recreate(outerTable, innerTable, predicate, Type);
            }

            return this;
        }
    }
}
