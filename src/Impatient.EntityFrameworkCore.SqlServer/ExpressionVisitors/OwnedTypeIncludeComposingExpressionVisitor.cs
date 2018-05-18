using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.Extensions.ReflectionExtensions;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors
{
    public class OwnedTypeIncludeComposingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo includeStringMethodInfo
            = GetGenericMethodDefinition<IQueryable<object>, object>(q => q.Include(""));

        private readonly IModel model;

        public OwnedTypeIncludeComposingExpressionVisitor(IModel model)
        {
            this.model = model;
        }

        protected override Expression VisitExtension(Expression node)
        {
            node = base.VisitExtension(node);

            if (node is EnumerableRelationalQueryExpression query)
            {
                var entityType = model.FindEntityType(query.SelectExpression.Type);

                if (entityType != null)
                {
                    var method = includeStringMethodInfo.MakeGenericMethod(query.SelectExpression.Type);

                    foreach (var (type, path) in GetOwnedTypeIncludePaths(entityType))
                    {
                        node = Expression.Call(method, node, Expression.Constant(path));
                    }
                }
            }

            return node;
        }

        private IEnumerable<(Type, string)> GetOwnedTypeIncludePaths(IEntityType entityType)
        {
            foreach (var navigation in entityType.GetNavigations())
            {
                if (navigation.ForeignKey.IsOwnership && !navigation.IsDependentToPrincipal())
                {
                    var targetType = navigation.GetTargetType();

                    if (targetType.Relational().Schema == entityType.Relational().Schema
                        && targetType.Relational().TableName == entityType.Relational().TableName)
                    {
                        continue;
                    }

                    var subpaths = GetOwnedTypeIncludePaths(targetType).ToArray();

                    if (subpaths.Length == 0)
                    {
                        yield return (targetType.ClrType, navigation.Name);
                    }
                    else
                    {
                        foreach (var (subtype, subpath) in subpaths)
                        {
                            yield return (subtype, $"{navigation.Name}.{subpath}");
                        }
                    }
                }
            }
        }
    }
}
