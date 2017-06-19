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
                        var targetSequence = methodCallExpression.Arguments[0];
                        var targetSequenceType = methodCallExpression.Arguments[0].Type.GetSequenceType();
                        var targetMethod = methodCallExpression.Method.GetGenericMethodDefinition();
                        var selectorParameter = Expression.Parameter(targetSequenceType, "x");

                        if (methodCallExpression.Arguments.Count == 2)
                        {
                            targetMethod
                                = (from m in methodCallExpression.Method.DeclaringType.GetMethods()
                                   where m.Name == targetMethod.Name
                                   where m.GetParameters().Length == 1
                                   select m).Single();

                            targetSequence
                                = Expression.Call(
                                    methodCallExpression.Method.DeclaringType == typeof(Queryable)
                                        ? queryableWhere.MakeGenericMethod(targetSequenceType)
                                        : enumerableWhere.MakeGenericMethod(targetSequenceType),
                                    targetSequence,
                                    methodCallExpression.Arguments[1]);
                        }

                        var selectCall
                            = Expression.Call(
                                methodCallExpression.Method.DeclaringType == typeof(Queryable)
                                    ? queryableSelect.MakeGenericMethod(targetSequenceType, node.Type)
                                    : enumerableSelect.MakeGenericMethod(targetSequenceType, node.Type),
                                targetSequence,
                                Expression.Quote(
                                    Expression.Lambda(
                                        Expression.MakeMemberAccess(selectorParameter, node.Member),
                                        selectorParameter)));

                        return Expression.Call(
                            targetMethod.MakeGenericMethod(node.Type),
                            selectCall);
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

        private static readonly MethodInfo enumerableWhere
            = GetGenericMethodDefinition((IEnumerable<bool> e) => e.Where(x => x));

        private static readonly MethodInfo queryableSelect
            = GetGenericMethodDefinition((IQueryable<object> e) => e.Select(x => x));

        private static readonly MethodInfo queryableWhere
            = GetGenericMethodDefinition((IQueryable<bool> e) => e.Where(x => x));
    }
}
