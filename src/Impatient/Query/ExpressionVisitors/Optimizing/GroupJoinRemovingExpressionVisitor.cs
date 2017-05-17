using Impatient.Query.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.ImpatientExtensions;

namespace Impatient.Query.ExpressionVisitors.Optimizing
{
    public class GroupJoinRemovingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.DeclaringType == typeof(Queryable) 
                && node.Method.Name == nameof(Queryable.GroupJoin))
            {
                var arguments = Visit(node.Arguments).ToArray();

                var outerSequence = arguments[0];
                var innerSequence = arguments[1];
                var outerKeySelector = arguments[2].UnwrapLambda();
                var innerKeySelector = arguments[3].UnwrapLambda();
                var resultSelector = arguments[4].UnwrapLambda();

                var genericArguments = node.Method.GetGenericArguments();

                var predicate
                    = Expression.Equal(
                        outerKeySelector.Body,
                        innerKeySelector.Body);

                var filtered
                    = Expression.Call(
                        whereMethodInfo.MakeGenericMethod(genericArguments[1]),
                        innerSequence,
                        Expression.Lambda(
                            new JoinPredicateExpression(
                                predicate,
                                outerKeySelector,
                                innerKeySelector),
                            innerKeySelector.Parameters[0]));

                var projected
                    = Expression.Call(
                        selectMethodInfo.MakeGenericMethod(genericArguments[0], genericArguments[3]),
                        outerSequence,
                        Expression.Quote(
                            Expression.Lambda(
                                resultSelector.Body
                                    .Replace(resultSelector.Parameters[0], outerKeySelector.Parameters[0])
                                    .Replace(resultSelector.Parameters[1], filtered),
                                outerKeySelector.Parameters[0])));

                return projected;
            }

            return base.VisitMethodCall(node);
        }

        private static readonly MethodInfo whereMethodInfo
            = GetGenericMethodDefinition((IEnumerable<object> e) => e.Where(x => true));

        private static readonly MethodInfo selectMethodInfo
            = GetGenericMethodDefinition((IQueryable<object> e) => e.Select(x => x));
    }
}