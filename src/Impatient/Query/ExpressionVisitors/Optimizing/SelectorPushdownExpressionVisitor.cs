using Impatient.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.Extensions.ReflectionExtensions;

namespace Impatient.Query.ExpressionVisitors.Optimizing;

public class SelectorPushdownExpressionVisitor : ExpressionVisitor
{
    protected override Expression VisitIndex(IndexExpression node)
    {
        var @object = Visit(node.Object);
        var arguments = Visit(node.Arguments);

        if (HandlePushdown(@object, p => node.Update(p, arguments), out var result))
        {
            return result;
        }

        return node.Update(@object, arguments);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        var expression = Visit(node.Expression);

        if (HandlePushdown(expression, node.Update, out var result))
        {
            return result;
        }

        return node.Update(expression);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var @object = Visit(node.Object);
        var arguments = Visit(node.Arguments);

        if (HandlePushdown(@object, p => node.Update(p, arguments), out var result))
        {
            return result;
        }

        return node.Update(@object, arguments);
    }

    protected override Expression VisitTypeBinary(TypeBinaryExpression node)
    {
        var expression = Visit(node.Expression);

        if (HandlePushdown(expression, node.Update, out var result))
        {
            return result;
        }

        return node.Update(expression);
    }

    private static bool HandlePushdown(Expression expression, Func<ParameterExpression, Expression> makeSelector, out Expression result)
    {
        result = default;

        if (!(expression is MethodCallExpression methodCallExpression
            && methodCallExpression.Method.IsQueryableOrEnumerableMethod()))
        {
            return false;
        }

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
                var parameter = Expression.Parameter(targetSequenceType, "x");

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

                Expression selector
                    = Expression.Lambda(
                        makeSelector(parameter),
                        parameter);

                var type = ((LambdaExpression)selector).ReturnType;

                if (methodCallExpression.Method.IsQueryableMethod())
                {
                    selectMethod = queryableSelect;
                    selector = Expression.Quote(selector);
                }

                result 
                    = Expression.Call(
                        targetMethod.MakeGenericMethod(type),
                        Expression.Call(
                            selectMethod.MakeGenericMethod(targetSequenceType, type),
                            targetSequence,
                            selector));

                return true;
            }

            case nameof(Queryable.ElementAt):
            case nameof(Queryable.ElementAtOrDefault):
            {
                var sequenceType = methodCallExpression.Arguments[0].Type.GetSequenceType();

                var parameter = Expression.Parameter(sequenceType, "x");

                var selectMethod = enumerableSelect;

                Expression selector
                    = Expression.Lambda(
                        makeSelector(parameter),
                        parameter);

                var type = ((LambdaExpression)selector).ReturnType;

                if (methodCallExpression.Method.IsQueryableMethod())
                {
                    selectMethod = queryableSelect;
                    selector = Expression.Quote(selector);
                }

                result
                    = Expression.Call(
                        methodCallExpression.Method.GetGenericMethodDefinition().MakeGenericMethod(type),
                        Expression.Call(
                            selectMethod.MakeGenericMethod(sequenceType, type),
                            methodCallExpression.Arguments[0],
                            selector),
                        methodCallExpression.Arguments[1]);

                return true;
            }
        }

        return false;
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
