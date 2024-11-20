using Impatient.Extensions;
using System;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Normalizing;

public class EqualsMethodNormalizingExpressionVisitor : ExpressionVisitor
{
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var @object = Visit(node.Object);
        var arguments = Visit(node.Arguments);

        if (node.Method.Name == nameof(Equals)
            && (node.Method.IsStatic || arguments.Count == 1))
        {
            if (node.Method.DeclaringType == typeof(object)
                || node.Method.DeclaringType.IsGenericType(typeof(IEquatable<>)))
            {
                Expression left, right;

                if (node.Method.IsStatic)
                {
                    left = arguments[0];
                    right = arguments[1];
                }
                else
                {
                    left = @object;
                    right = arguments[0];
                }

                var type = node.Method.GetParameters()[0].ParameterType;

                if (left.Type != right.Type || left.Type == typeof(object))
                {
                    var leftType = left.UnwrapInnerExpression().Type.UnwrapNullableType();
                    var rightType = right.UnwrapInnerExpression().Type.UnwrapNullableType();

                    if (!leftType.IsAssignableFrom(rightType)
                        && !rightType.IsAssignableFrom(leftType))
                    {
                        return Expression.Constant(false);
                    }

                    if (left.Type != type)
                    {
                        left = Expression.Convert(left, type);
                    }

                    if (right.Type != type)
                    {
                        right = Expression.Convert(right, type);
                    }
                }

                return Expression.Equal(left, right);
            }
        }

        return node.Update(@object, arguments);
    }
}
