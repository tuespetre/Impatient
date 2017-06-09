using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class LeftJoinExpression : PredicateJoinExpression
    {
        public LeftJoinExpression(
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
            return new LeftJoinExpression(
                outerTable,
                innerTable,
                predicate,
                type);
        }
    }
}
