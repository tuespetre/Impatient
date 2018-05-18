using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class InnerJoinTableExpression : PredicateJoinTableExpression
    {
        public InnerJoinTableExpression(
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
            return new InnerJoinTableExpression(
                outerTable,
                innerTable,
                predicate,
                type);
        }
    }
}
