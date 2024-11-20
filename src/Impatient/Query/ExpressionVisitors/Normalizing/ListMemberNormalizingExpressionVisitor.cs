using Impatient.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.Extensions.ReflectionExtensions;

namespace Impatient.Query.ExpressionVisitors.Normalizing;

/// <summary>
/// <list type="bullet">
/// <item>Rewrites <see cref="List{T}.Contains(T)"/> as <see cref="Enumerable.Contains{T}(IEnumerable{T}, T)}"/>.</item>
/// <item>Rewrites <see cref="List{T}.Count"/> as <see cref="Enumerable.Count{T}(IEnumerable{T})}"/>.</item>
/// <item>Rewrites <see cref="List{T}.Exists(System.Predicate{T})"/> as <see cref="Enumerable.Any{T}(IEnumerable{T}, System.Func{T, bool})"/>.</item>
/// </list>
/// </summary>
public class ListMemberNormalizingExpressionVisitor : ExpressionVisitor
{
    private static readonly MethodInfo enumerableAnyMethodInfo
        = GetGenericMethodDefinition((IEnumerable<object> e) => e.Any(null));

    private static readonly MethodInfo enumerableContainsMethodInfo
        = GetGenericMethodDefinition((IEnumerable<object> e) => e.Contains(null));

    private static readonly MethodInfo enumerableCountMethodInfo
        = GetGenericMethodDefinition((IEnumerable<object> e) => e.Count());

    protected override Expression VisitMember(MemberExpression node)
    {
        var expression = Visit(node.Expression);

        var listType = node.Member.DeclaringType.FindGenericType(typeof(List<>));

        if (listType is not null && node.Member.Name is nameof(List<object>.Count))
        {
            return Expression.Call(
                enumerableCountMethodInfo.MakeGenericMethod(
                    listType.GetGenericArguments().Single()),
                [expression]);
        }

        return node.Update(expression);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var @object = Visit(node.Object);
        var arguments = Visit(node.Arguments);

        var listType = node.Method.DeclaringType.FindGenericType(typeof(List<>));

        if (listType is not null)
        {
            if (node.Method.Equals(listType.GetMethod(nameof(List<object>.Contains))))
            {
                return Expression.Call(
                    enumerableContainsMethodInfo.MakeGenericMethod(
                        listType.GetGenericArguments().Single()),
                    [@object, .. arguments]);
            }
            else if (node.Method.Equals(listType.GetMethod(nameof(List<object>.Exists)))
                && arguments[0] is LambdaExpression lambda)
            {
                // The lambda is recreated to get Func<T,bool> instead of Predicate<T>.
                return Expression.Call(
                    enumerableAnyMethodInfo.MakeGenericMethod(
                        listType.GetGenericArguments().Single()),
                    [@object, Expression.Lambda(lambda.Body, lambda.Parameters)]);
            }
        }

        return node.Update(@object, arguments);
    }
}
