using Impatient.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.Extensions.ReflectionExtensions;

namespace Impatient.Query.ExpressionVisitors.Normalizing;

/// <summary>
/// Rewrites <see cref="ICollection{T}.Contains(T)"/> calls as <see cref="Enumerable.Contains{T}(IEnumerable{T}, T)}"/> calls.
/// </summary>
public class CollectionMemberNormalizingExpressionVisitor : ExpressionVisitor
{
    private static readonly MethodInfo enumerableContainsMethodInfo
        = GetGenericMethodDefinition((IEnumerable<object> e) => e.Contains(null));

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var @object = Visit(node.Object);
        var arguments = Visit(node.Arguments);

        var collectionType = node.Method.DeclaringType.FindGenericType(typeof(ICollection<>));

        if (collectionType is not null && node.Method.Name is nameof(ICollection<object>.Contains))
        {
            return Expression.Call(
                enumerableContainsMethodInfo.MakeGenericMethod(@object.Type.GetSequenceType()),
                [@object, .. arguments]);
        }

        return node.Update(@object, arguments);
    }
}
