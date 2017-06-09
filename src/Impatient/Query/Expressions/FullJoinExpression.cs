using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class FullJoinExpression : PredicateJoinExpression
    {
        public FullJoinExpression(
            TableExpression outerTable,
            AliasedTableExpression innerTable,
            Expression predicate,
            Type type)
            : base(outerTable, innerTable, predicate, type)
        {
        }

        protected override PredicateJoinExpression Recreate(
            TableExpression outerTable,
            AliasedTableExpression innerTable,
            Expression predicate,
            Type type)
        {
            return new FullJoinExpression(
                outerTable,
                innerTable,
                predicate,
                type);
        }
    }
}
