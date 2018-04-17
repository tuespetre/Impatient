using Impatient.Extensions;
using Impatient.Query.ExpressionVisitors.Optimizing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    internal class EFCoreQueryableInliningExpressionVisitor : QueryableInliningExpressionVisitor
    {
        private readonly ICurrentDbContext currentDbContext;

        public EFCoreQueryableInliningExpressionVisitor(
            IQueryProvider provider,
            IDictionary<object, ParameterExpression> parameterMapping,
            ICurrentDbContext currentDbContext)
            : base(provider, parameterMapping)
        {
            this.currentDbContext = currentDbContext;
        }

        public override Expression Visit(Expression node)
        {
            var visited = base.Visit(node);

            if (visited is ConstantExpression constant && constant.IsEntityQueryable())
            {
                var queryable = (IQueryable)constant.Value;

                try
                {
                    var query = ModelHelper.CreateQueryExpression(queryable.ElementType, currentDbContext.Context.Model);

                    var repointer = new QueryFilterRepointingExpressionVisitor(currentDbContext);

                    query = repointer.Visit(query);

                    query = Visit(Reparameterize(query));

                    if (node.Type.GetSequenceType() != query.Type.GetSequenceType())
                    {
                        query 
                            = Expression.Call(
                                typeof(Queryable).GetMethod(nameof(Queryable.Cast))
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

            public QueryFilterRepointingExpressionVisitor(ICurrentDbContext currentDbContext)
            {
                this.currentDbContext = currentDbContext;
            }

            protected override Expression VisitConstant(ConstantExpression node)
            {
                if (node.Type.IsAssignableFrom(currentDbContext.Context.GetType()))
                {
                    return Expression.Constant(currentDbContext.Context);
                }

                return base.VisitConstant(node);
            }
        }
    }
}
