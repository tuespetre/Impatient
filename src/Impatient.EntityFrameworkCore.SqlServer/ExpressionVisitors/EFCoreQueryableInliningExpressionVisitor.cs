using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Microsoft.EntityFrameworkCore.Internal;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    internal class EFCoreQueryableInliningExpressionVisitor : QueryableInliningExpressionVisitor
    {
        private readonly ICurrentDbContext currentDbContext;
        private readonly ParameterExpression dbContextParameter;

        public EFCoreQueryableInliningExpressionVisitor(
            IQueryProvider provider,
            IDictionary<object, ParameterExpression> parameterMapping,
            ICurrentDbContext currentDbContext)
            : base(provider, parameterMapping)
        {
            this.currentDbContext = currentDbContext;
            dbContextParameter = parameterMapping[currentDbContext.Context];
        }

        public override Expression Visit(Expression node)
        {
            var visited = base.Visit(node);

            if (visited is ConstantExpression constant && constant.IsEntityQueryable())
            {
                var queryable = (IQueryable)constant.Value;

                try
                {
                    var query 
                        = ModelHelper.CreateQueryExpression(
                            queryable.ElementType, 
                            currentDbContext.Context.Model);

                    if (!(query is RelationalQueryExpression))
                    {
                        var repointer
                            = new QueryFilterRepointingExpressionVisitor(
                                currentDbContext,
                                dbContextParameter);

                        var repointed = repointer.Visit(query);

                        query = Visit(Reparameterize(repointed));
                    }

                    if (node.Type.GetSequenceType() != query.Type.GetSequenceType())
                    {
                        query 
                            = Expression.Call(
                                typeof(Queryable)
                                    .GetMethod(nameof(Queryable.Cast))
                                    .MakeGenericMethod(node.Type.GetSequenceType()),
                                query);
                    }

                    return query;
                }
                catch
                {
                    PropagateExceptions();

                    throw;
                }
            }

            return visited;
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
