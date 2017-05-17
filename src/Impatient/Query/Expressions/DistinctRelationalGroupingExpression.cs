using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class DistinctRelationalGroupingExpression : RelationalGroupingExpression
    {
        public DistinctRelationalGroupingExpression(
            EnumerableRelationalQueryExpression underlyingQuery,
            Expression keySelector, 
            Expression elementSelector)
            : base(underlyingQuery, keySelector, elementSelector)
        {
        }
    }
}
