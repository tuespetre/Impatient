using Impatient.Extensions;
using System;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Normalizing;

public class NullableMemberNormalizingExpressionVisitor : ExpressionVisitor
{
    protected override Expression VisitMember(MemberExpression node)
    {
        var expression = Visit(node.Expression);

        if (expression is not null && expression.Type.IsNullableType())
        {
            switch (node.Member.Name)
            {
                case nameof(Nullable<int>.Value):
                {
                    return Expression.Convert(
                        expression,
                        Nullable.GetUnderlyingType(
                            expression.Type));
                }

                case nameof(Nullable<int>.HasValue):
                {
                    return Expression.NotEqual(
                        expression,
                        Expression.Constant(
                            null,
                            expression.Type));
                }
            }
        }

        return node.Update(expression);
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var @object = Visit(node.Object);
        var arguments = Visit(node.Arguments);

        if (@object is not null && @object.Type.IsNullableType())
        {
            switch (node.Method.Name)
            {
                case nameof(Nullable<int>.GetValueOrDefault):
                {
                    if (arguments.Count == 1)
                    {
                        return Expression.Coalesce(
                            @object,
                            arguments[0]);
                    }
                    else
                    {
                        return Expression.Coalesce(
                            @object,
                            Expression.Constant(
                                Activator.CreateInstance(
                                    Nullable.GetUnderlyingType(
                                        @object.Type))));
                    }
                }
            }
        }

        return node.Update(@object, arguments);
    }
}
