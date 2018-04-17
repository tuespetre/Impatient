using Impatient.Extensions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class EqualsMethodRewritingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(object.Equals))
            {
                if (node.Method.DeclaringType == typeof(object)
                    || node.Method.DeclaringType.IsGenericType(typeof(IEquatable<>)))
                {
                    Expression left, right;

                    if (node.Method.IsStatic)
                    {
                        left = node.Arguments[0];
                        right = node.Arguments[1];
                    }
                    else
                    {
                        left = node.Object;
                        right = node.Arguments.Single();
                    }

                    var type = node.Method.GetParameters()[0].ParameterType;

                    if (left.Type != right.Type)
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

            return base.VisitMethodCall(node);
        }
    }
}
