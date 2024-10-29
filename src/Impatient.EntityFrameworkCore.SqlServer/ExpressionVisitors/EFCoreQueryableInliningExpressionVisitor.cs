using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer;

internal class EFCoreQueryableInliningExpressionVisitor : QueryableInliningExpressionVisitor
{
    private readonly ModelExpressionProvider modelExpressionProvider;
    private readonly ModelQueryExpressionCache modelQueryExpressionCache;
    private readonly ICurrentDbContext currentDbContext;
    private readonly ParameterExpression dbContextParameter;

    public EFCoreQueryableInliningExpressionVisitor(
        IQueryProvider provider,
        IDictionary<object, ParameterExpression> parameterMapping,
        ModelExpressionProvider modelExpressionProvider,
        ModelQueryExpressionCache modelQueryExpressionCache,
        ICurrentDbContext currentDbContext)
        : base(provider, parameterMapping)
    {
        this.modelExpressionProvider = modelExpressionProvider ?? throw new ArgumentNullException(nameof(modelExpressionProvider));
        this.modelQueryExpressionCache = modelQueryExpressionCache ?? throw new ArgumentNullException(nameof(modelQueryExpressionCache));
        this.currentDbContext = currentDbContext ?? throw new ArgumentNullException(nameof(currentDbContext));
        dbContextParameter = parameterMapping[currentDbContext.Context];
    }

    protected override Expression VisitExtension(Expression node)
    {
        node = base.VisitExtension(node);

        switch (node)
        {
            case FromSqlQueryRootExpression fromSqlQueryRoot:
            {
                // TODO: more details?
                throw new NotSupportedException("Impatient for EF Core does not support ad-hoc SQL query building.");
            }

            case EntityQueryRootExpression entityQueryRoot:
            {
                var entityType = entityQueryRoot.EntityType;
                var elementType = entityQueryRoot.ElementType;
                var key = elementType.TypeHandle.Value;

                var query
                    = modelQueryExpressionCache.Lookup.GetOrAdd(
                        entityType,
                        (entityType, arg) =>
                            arg.modelExpressionProvider.CreateQueryExpression(
                                entityType,
                                arg.currentDbContext.Context),
                        (modelExpressionProvider, currentDbContext));

                // This block is moreso for types with defining queries than types that
                // just happen to have query filters. The defining queries need to be inlined.

                if (query is not RelationalQueryExpression)
                {
                    var repointer = new QueryFilterRepointingExpressionVisitor(dbContextParameter);

                    var repointed = repointer.Visit(query);

                    query = Visit(Reparameterize(repointed));
                }

                return query;
            }

            case QueryRootExpression queryRoot:
            {
                throw new NotSupportedException($"Impatient for EF Core does not support QueryRootExpression of type {queryRoot.GetType().Name}");
            }

            default:
            {
                return node;
            }
        }
    }

    protected override Expression VisitConstant(ConstantExpression node)
    {
        if (node.Value is IQueryable queryable)
        {
            if (ReferenceEquals(queryable.Provider, queryProvider))
            {
                return Visit(queryable.Expression);
            }

            throw new InvalidOperationException(CoreStrings.ErrorInvalidQueryable);
        }

        return node;
    }
}
