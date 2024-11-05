using Impatient.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.Extensions.ReflectionExtensions;

namespace Impatient.Query.ExpressionVisitors.Rewriting;

/// <summary>
/// Rewrites <see cref="ICollection{T}.Contains(T)"/> calls as <see cref="Enumerable.Contains{T}(IEnumerable{T}, T)}"/> calls.
/// </summary>
public class CollectionContainsRewritingExpressionVisitor : ExpressionVisitor
{
    private static readonly MethodInfo enumerableContainsMethodInfo
        = GetGenericMethodDefinition((IEnumerable<object> e) => e.Contains(null));

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var @object = Visit(node.Object);
        var arguments = Visit(node.Arguments);

        var collectionType = node.Method.DeclaringType.FindGenericType(typeof(ICollection<>));

        if (collectionType is not null && @object.Type.GetSequenceType().IsScalarType())
        {
            // I'm not actually sure what this next if/else is about or why it was needed.
            // I should have added comments at the time, lol

            var canRewriteMethod = false;

            if (node.Method.DeclaringType == collectionType)
            {
                canRewriteMethod = true;
            }
            else
            {
                var map = node.Method.DeclaringType.GetTypeInfo().GetRuntimeInterfaceMap(collectionType);

                var index = map.InterfaceMethods.ToList().FindIndex(m => m.Name == nameof(ICollection<int>.Contains));

                canRewriteMethod = node.Method == map.TargetMethods[index];
            }

            if (canRewriteMethod)
            {
                return Expression.Call(
                    enumerableContainsMethodInfo.MakeGenericMethod(@object.Type.GetSequenceType()),
                    [@object, .. arguments]);
            }
        }

        return node.Update(@object, arguments);
    }
}
