using Impatient.EntityFrameworkCore.SqlServer.Infrastructure;
using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    internal class EFCoreQueryableInliningExpressionVisitor : QueryableInliningExpressionVisitor
    {
        private readonly ICurrentDbContext currentDbContext;
        private readonly ModelQueryExpressionCache modelQueryExpressionCache;
        private readonly ParameterExpression dbContextParameter;

        public EFCoreQueryableInliningExpressionVisitor(
            IQueryProvider provider,
            IDictionary<object, ParameterExpression> parameterMapping,
            ICurrentDbContext currentDbContext,
            ModelQueryExpressionCache modelQueryExpressionCache)
            : base(provider, parameterMapping)
        {
            this.currentDbContext = currentDbContext;
            this.modelQueryExpressionCache = modelQueryExpressionCache;
            dbContextParameter = parameterMapping[currentDbContext.Context];
        }

        protected override Expression InlineQueryable(IQueryable queryable)
        {
            if (queryable.Expression.Type.IsGenericType(typeof(EntityQueryable<>)))
            {
                var key = queryable.ElementType.TypeHandle.Value;

                if (!modelQueryExpressionCache.Lookup.TryGetValue(key, out var query))
                {
                    query 
                        = ModelHelper.CreateQueryExpression(
                            queryable.ElementType, 
                            currentDbContext.Context.Model);

                    modelQueryExpressionCache.Lookup[key] = query;
                }

                if (!(query is RelationalQueryExpression))
                {
                    var repointer
                        = new QueryFilterRepointingExpressionVisitor(
                            currentDbContext,
                            dbContextParameter);

                    var repointed = repointer.Visit(query);

                    query = Visit(Reparameterize(repointed));
                }

                if (queryable.ElementType != query.Type.GetSequenceType())
                {
                    query 
                        = Expression.Call(
                            typeof(Queryable)
                                .GetMethod(nameof(Queryable.Cast))
                                .MakeGenericMethod(queryable.ElementType),
                            query);
                }

                return query;
            }

            return base.InlineQueryable(queryable);
        }

        private class QueryFilterRepointingExpressionVisitor : ExpressionVisitor
        {
            private readonly ICurrentDbContext currentDbContext;
            private readonly ParameterExpression dbContextParameter;

            public QueryFilterRepointingExpressionVisitor(
                ICurrentDbContext currentDbContext,
                ParameterExpression dbContextParameter)
            {
                this.currentDbContext = currentDbContext;
                this.dbContextParameter = dbContextParameter;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Type.IsAssignableFrom(currentDbContext.Context.GetType()))
                {
                    return dbContextParameter;
                }

                return base.VisitConstant(node);
            }
        }
    }
}
