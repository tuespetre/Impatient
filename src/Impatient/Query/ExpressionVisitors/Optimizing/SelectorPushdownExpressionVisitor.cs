using Impatient.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.Extensions.ReflectionExtensions;

namespace Impatient.Query.ExpressionVisitors.Optimizing
{
    public class SelectorPushdownExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            var expression = Visit(node.Expression);

            if (expression is MethodCallExpression methodCallExpression
                && methodCallExpression.Method.IsQueryableOrEnumerableMethod())
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
                                    methodCallExpression.Method.IsQueryableMethod()
                                        ? queryableWhere.MakeGenericMethod(targetSequenceType)
                                        : enumerableWhere.MakeGenericMethod(targetSequenceType),
                                    targetSequence,
                                    methodCallExpression.Arguments[1]);
                        }

                        var selectMethod = enumerableSelect;

                        var selector 
                            = (Expression)Expression.Lambda(
                                Expression.MakeMemberAccess(selectorParameter, node.Member),
                                selectorParameter);

                        if (methodCallExpression.Method.IsQueryableMethod())
                        {
                            selectMethod = queryableSelect;
                            selector = Expression.Quote(selector);
                        }

                        return Expression.Call(
                            targetMethod.MakeGenericMethod(node.Type),
                            Expression.Call(
                                selectMethod.MakeGenericMethod(targetSequenceType, node.Type),
                                targetSequence,
                                selector));
                    }

                    case nameof(Queryable.ElementAt):
                    case nameof(Queryable.ElementAtOrDefault):
                    {
                        var sequenceType = methodCallExpression.Arguments[0].Type.GetSequenceType();

                        var parameter = Expression.Parameter(sequenceType, "x");

                        var selectMethod = enumerableSelect;

                        var selector
                            = (Expression)Expression.Lambda(
                                Expression.MakeMemberAccess(parameter, node.Member),
                                parameter);

                        if (methodCallExpression.Method.IsQueryableMethod())
                        {
                            selectMethod = queryableSelect;
                            selector = Expression.Quote(selector);
                        }

                        return Expression.Call(
                            methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(node.Type),
                            Expression.Call(
                                selectMethod.MakeGenericMethod(sequenceType, node.Type),
                                methodCallExpression.Arguments[0],
                                selector),
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
