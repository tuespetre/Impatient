using Impatient.Query.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.ImpatientExtensions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class RelationalGroupingExpansionRewritingExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            if (node is RelationalGroupingExpression relationalGroupingExpression)
            {
                var uniquifier = new TableUniquifyingExpressionVisitor();

                var underlyingQuery = relationalGroupingExpression.UnderlyingQuery;

                underlyingQuery = uniquifier.VisitAndConvert(underlyingQuery, nameof(Visit));

                var filtered
                    = Expression.Call(
                        enumerableWhereMethodInfo.MakeGenericMethod(underlyingQuery.Type.GetSequenceType()),
                        underlyingQuery,
                        Expression.Lambda(
                            Expression.Equal(
                                relationalGroupingExpression.KeySelector,
                                relationalGroupingExpression.KeySelector))); // TODO: Fix
            }

            return base.Visit(node);
        }

        private static readonly MethodInfo enumerableWhereMethodInfo
            = GetGenericMethodDefinition((IEnumerable<object> e) => e.Where(x => true));
    }
}
