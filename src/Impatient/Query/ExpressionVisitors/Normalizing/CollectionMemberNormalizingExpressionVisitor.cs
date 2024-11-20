using Impatient.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.Extensions.ReflectionExtensions;

namespace Impatient.Query.ExpressionVisitors.Normalizing;

/// <summary>
/// <list type="bullet">
/// <item>Rewrites <see cref="ICollection{T}.Contains(T)"/> as <see cref="Enumerable.Contains{T}(IEnumerable{T}, T)}"/>.</item>
/// <item>Rewrites <see cref="ICollection{T}.Count"/> as <see cref="Enumerable.Count{T}(IEnumerable{T})}"/>.</item>
/// </list>
/// </summary>
public class CollectionMemberNormalizingExpressionVisitor : ExpressionVisitor
{
    private static readonly MethodInfo enumerableContainsMethodInfo
        = GetGenericMethodDefinition((IEnumerable<object> e) => e.Contains(null));

    private static readonly MethodInfo enumerableCountMethodInfo
        = GetGenericMethodDefinition((IEnumerable<object> e) => e.Count());

    protected override Expression VisitMember(MemberExpression node)
    {
        var expression = Visit(node.Expression);

        var collectionType = node.Member.DeclaringType.FindGenericType(typeof(ICollection<>));

        if (collectionType is not null && node.Member.Name is nameof(ICollection<object>.Count))
        {
            return Expression.Call(
                enumerableCountMethodInfo.MakeGenericMethod(
                    collectionType.GetGenericArguments().Single()),
                [expression]);
        }

        return node.Update(expression);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var @object = Visit(node.Object);
        var arguments = Visit(node.Arguments);

        var collectionType = node.Method.DeclaringType.FindGenericType(typeof(ICollection<>));

        if (collectionType is not null && node.Method.Name is nameof(ICollection<object>.Contains))
        {
            return Expression.Call(
                enumerableContainsMethodInfo.MakeGenericMethod(
                    collectionType.GetGenericArguments().Single()),
                [@object, .. arguments]);
        }

        return node.Update(@object, arguments);
    }
}
