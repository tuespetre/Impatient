using Impatient.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors;

public class ShadowPropertyRewritingExpressionVisitor : ExpressionVisitor
{
    private readonly IModel model;

    public ShadowPropertyRewritingExpressionVisitor(IModel model)
    {
        this.model = model ?? throw new ArgumentNullException(nameof(model));
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var @object = Visit(node.Object);
        var arguments = Visit(node.Arguments);

        if (node.Method.IsEFPropertyMethod()
            && arguments[1] is ConstantExpression constantExpression)
        {
            var entityExpression = arguments[0];
            var propertyName = (string)constantExpression.Value;

            if (entityExpression.TryResolvePath(propertyName, out var resolved))
            {
                if (resolved.Type != node.Type)
                {
                    resolved = Expression.Convert(resolved, node.Type);
                }

                return resolved;
            }

            // TODO: used to be SingleOrDefault. something is probably wrong here (as in, with using FirstOrDefault)
            var entityType = model.FindFirstEntityType(entityExpression.Type);

            if (entityType is not null)
            {
                var result = default(Expression);

                var property = entityType.FindProperty(propertyName);

                if (property is not null && !property.IsShadowProperty())
                {
                    result = Expression.MakeMemberAccess(entityExpression, property.GetSemanticReadableMemberInfo());
                }

                var navigation = entityType.FindNavigation(propertyName);

                if (navigation is not null && !navigation.IsShadowProperty())
                {
                    result = Expression.MakeMemberAccess(entityExpression, navigation.GetSemanticReadableMemberInfo());
                }

                if (result is not null)
                {
                    if (result.Type != node.Type)
                    {
                        try
                        {
                            result = Expression.Convert(result, node.Type);
                        }
                        catch { } // TODO: think this through a little more.
                    }

                    if (node.Type.IsAssignableFrom(result.Type))
                    {
                        return result;
                    }
                }
            }
        }

        return node.Update(@object, arguments);
    }
}
