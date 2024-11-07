using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using static Impatient.Extensions.ReflectionExtensions;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors;

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
            var entityType = (query.SelectExpression.Projection.Flatten().Body as EntityMaterializationExpression)?.EntityType;

            if (entityType is null)
            {
                entityType = model.FindFirstEntityType(query.SelectExpression.Type);
            }

            if (entityType is not null)
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

    private static IEnumerable<(Type, string)> GetOwnedTypeIncludePaths(IEntityType entityType)
    {
        foreach (var navigation in entityType.GetNavigations().Where(n => n.ForeignKey.IsOwnership && !n.IsOnDependent))
        {
            if (navigation.IsCollection && !navigation.TargetEntityType.IsMappedToJson())
            {
                var ownedType = navigation.TargetEntityType;

                if (ownedType.GetSchema() == entityType.GetSchema()
                    && ownedType.GetTableName() == entityType.GetTableName())
                {
                    continue;
                }

                var subpaths = GetOwnedTypeIncludePaths(ownedType).ToArray();

                if (subpaths.Length == 0)
                {
                    yield return (ownedType.ClrType, navigation.Name);
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
