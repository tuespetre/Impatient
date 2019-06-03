using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
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

        protected override Expression InlineQueryable(IQueryable queryable)
        {
            if (queryable.Expression.Type.IsGenericType(typeof(EntityQueryable<>)))
            {
                var key = queryable.ElementType.TypeHandle.Value;

                if (!modelQueryExpressionCache.Lookup.ContainsKey(key))
                {
                    modelQueryExpressionCache.Lookup.TryAdd(key, modelExpressionProvider.CreateQueryExpression(queryable.ElementType, currentDbContext.Context));
                }

                modelQueryExpressionCache.Lookup.TryGetValue(key, out Expression query);

                // This block is moreso for types with defining queries than types that
                // just happen to have query filters. The defining queries need to be inlined.

                if (!(query is RelationalQueryExpression))
                {
                    var repointer = new QueryFilterRepointingExpressionVisitor(dbContextParameter);

                    var repointed = repointer.Visit(query);

                    query = Visit(Reparameterize(repointed));
                }

                return query;
            }

            return base.InlineQueryable(queryable);
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (node.Value is IQueryable queryable && queryable.Provider is IAsyncQueryProvider)
            {
                if (ReferenceEquals(queryable.Provider, queryProvider))
                {
                    return InlineQueryable(queryable);
                }

                throw new InvalidOperationException(CoreStrings.ErrorInvalidQueryable);
            }

            return node;
        }
    }
}