using Impatient.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Extensions.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors
{
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
                var propertyName = (string)constantExpression.Value;

                if (arguments[0].TryResolvePath(propertyName, out var resolved))
                {
                    if (resolved.Type != node.Type)
                    {
                        resolved = Expression.Convert(resolved, node.Type);
                    }

                    return resolved;
                }

                var entityType = model.GetEntityTypes().SingleOrDefault(t => t.ClrType == arguments[0].Type);

                if (entityType != null)
                {
                    var result = default(Expression);

                    var property = entityType.FindProperty(propertyName);

                    if (property != null && !property.IsShadowProperty)
                    {
                        result = Expression.MakeMemberAccess(arguments[0], property.GetReadableMemberInfo());
                    }

                    var navigation = entityType.FindNavigation(propertyName);

                    if (navigation != null && !navigation.IsShadowProperty)
                    {
                        result = Expression.MakeMemberAccess(arguments[0], navigation.GetReadableMemberInfo());
                    }

                    if (result != null)
                    {
                        if (result.Type != node.Type 
                            && result.Type.UnwrapNullableType() == node.Type.UnwrapNullableType())
                        {
                            result = Expression.Convert(result, node.Type);
                        }

                        return result;
                    }
                }
            }

            return node.Update(@object, arguments);
        }
    }
}
