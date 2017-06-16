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
                    || methodCallExpression.Method.DeclaringType == typeof(Enumerable)))
            {
                switch (methodCallExpression.Method.Name)
                {
                    case nameof(Queryable.First):
                    case nameof(Queryable.FirstOrDefault):
                    case nameof(Queryable.Last):
                    case nameof(Queryable.LastOrDefault):
                    case nameof(Queryable.Single):
                    case nameof(Queryable.SingleOrDefault):
                    {
                        if (methodCallExpression.Arguments.Count != 1)
                        {
                            // TODO: also optimize predicate overloads
                            break;
                        }

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

                    case nameof(Queryable.ElementAt):
                    case nameof(Queryable.ElementAtOrDefault):
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
                                        parameter))),
                            methodCallExpression.Arguments[1]);
                    }
                }
            }

            return base.VisitMember(node);
        }

        private static readonly MethodInfo enumerableSelect
            = GetGenericMethodDefinition((IEnumerable<object> e) => e.Select(x => x));

        private static readonly MethodInfo queryableSelect
            = GetGenericMethodDefinition((IQueryable<object> e) => e.Select(x => x));
    }
}
