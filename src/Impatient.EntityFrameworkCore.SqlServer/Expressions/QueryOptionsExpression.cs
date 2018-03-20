using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.Expressions
{
    public class QueryOptionsExpression : AnnotationExpression
    {
        public QueryOptionsExpression(
            Expression expression,
            QueryTrackingBehavior queryTrackingBehavior,
            bool ignoreQueryFilters) : base(expression)
        {
            QueryTrackingBehavior = queryTrackingBehavior;
            IgnoreQueryFilters = ignoreQueryFilters;
        }

        public QueryTrackingBehavior QueryTrackingBehavior { get; }

        public bool IgnoreQueryFilters { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = visitor.Visit(Expression);

            if (expression != Expression)
            {
                return new QueryOptionsExpression(expression, QueryTrackingBehavior, IgnoreQueryFilters);
            }

            return this;
        }

        public override int GetAnnotationHashCode() => (QueryTrackingBehavior, IgnoreQueryFilters).GetHashCode();
    }
}
