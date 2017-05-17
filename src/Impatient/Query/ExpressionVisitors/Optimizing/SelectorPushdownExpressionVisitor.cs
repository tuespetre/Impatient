using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.ImpatientExtensions;

namespace Impatient.Query.ExpressionVisitors.Optimizing
{
    public class SelectorPushdownExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            var expression = Visit(node.Expression);

            if (expression is MethodCallExpression methodCallExpression
                && (methodCallExpression.Method.DeclaringType == typeof(Queryable)
                    || methodCallExpression.Method.DeclaringType == typeof(Enumerable))
                && eligibleMethodNames.Contains(methodCallExpression.Method.Name)
                && methodCallExpression.Arguments.Count == 1)
            {
                var sequenceType = methodCallExpression.Arguments[0].Type.GetSequenceType();

                var parameter = Expression.Parameter(sequenceType, "x");

                return Expression.Call(
                    methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(node.Type),
                    Expression.Call(
                        methodCallExpression.Method.DeclaringType == typeof(Queryable)
                            ? queryableSelect.MakeGenericMethod(sequenceType, node.Type)
                            : enumerableSelect.MakeGenericMethod(sequenceType, node.Type),
                        methodCallExpression.Arguments[0],
                        Expression.Quote(
                            Expression.Lambda(
                                Expression.MakeMemberAccess(parameter, node.Member),
                                parameter))));
            }

            return base.VisitMember(node);
        }

        private static readonly MethodInfo enumerableSelect
            = GetGenericMethodDefinition((IEnumerable<object> e) => e.Select(x => x));

        private static readonly MethodInfo queryableSelect
            = GetGenericMethodDefinition((IQueryable<object> e) => e.Select(x => x));

        private static readonly string[] eligibleMethodNames = new[]
        {
            //nameof(Queryable.ElementAt),
            //nameof(Queryable.ElementAtOrDefault),
            nameof(Queryable.First),
            nameof(Queryable.FirstOrDefault),
            nameof(Queryable.Last),
            nameof(Queryable.LastOrDefault),
            nameof(Queryable.Single),
            nameof(Queryable.SingleOrDefault),
        };
    }
}
