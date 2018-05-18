using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class LeftJoinTableExpression : PredicateJoinTableExpression
    {
        public LeftJoinTableExpression(
            TableExpression outerTable,
            AliasedTableExpression innerTable,
            Expression predicate,
            Type type)
            : base(outerTable, innerTable, predicate, type)
        {
        }

        protected override PredicateJoinTableExpression Recreate(
            TableExpression outerTable,
            AliasedTableExpression innerTable,
            Expression predicate,
            Type type)
        {
            return new LeftJoinTableExpression(
                outerTable,
                innerTable,
                predicate,
                type);
        }
    }
}
