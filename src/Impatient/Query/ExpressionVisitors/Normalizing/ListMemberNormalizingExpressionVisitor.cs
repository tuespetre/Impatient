using Impatient.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.Extensions.ReflectionExtensions;

namespace Impatient.Query.ExpressionVisitors.Normalizing;

/// <summary>
/// Rewrites <see cref="List{T}.Contains(T)"/> calls as <see cref="Enumerable.Contains{T}(IEnumerable{T}, T)}"/> calls.
/// </summary>
public class ListMemberNormalizingExpressionVisitor : ExpressionVisitor
{
    private static readonly MethodInfo enumerableContainsMethodInfo
        = GetGenericMethodDefinition((IEnumerable<object> e) => e.Contains(null));

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var @object = Visit(node.Object);
        var arguments = Visit(node.Arguments);

        var listType = node.Method.DeclaringType.FindGenericType(typeof(List<>));

        if (listType is not null && node.Method.Equals(listType.GetMethod(nameof(List<object>.Contains))))
        {
            return Expression.Call(
                enumerableContainsMethodInfo.MakeGenericMethod(listType.GetGenericArguments().Single()),
                [@object, .. arguments]);
        }

        return node.Update(@object, arguments);
    }
}
