using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class InnerJoinExpression : PredicateJoinExpression
    {
        public InnerJoinExpression(
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
            return new InnerJoinExpression(
                outerTable,
                innerTable,
                predicate,
                type);
        }
    }
}
