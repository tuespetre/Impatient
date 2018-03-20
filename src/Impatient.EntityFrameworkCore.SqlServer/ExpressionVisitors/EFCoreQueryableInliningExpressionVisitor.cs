using Impatient.Query.ExpressionVisitors.Optimizing;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    internal class EFCoreQueryableInliningExpressionVisitor : QueryableInliningExpressionVisitor
    {
        private readonly IModel model;

        public EFCoreQueryableInliningExpressionVisitor(
            IQueryProvider provider,
            IReadOnlyDictionary<object, ParameterExpression> mapping,
            IModel model)
            : base(provider, mapping)
        {
            this.model = model;
        }

        public override Expression Visit(Expression node)
        {
            var visited = base.Visit(node);

            if (visited is ConstantExpression constant && constant.IsEntityQueryable())
            {
                var queryable = (IQueryable)constant.Value;

                return ModelHelper.CreateQueryable(queryable.ElementType, model);
            }

            return visited;
        }
    }
}
