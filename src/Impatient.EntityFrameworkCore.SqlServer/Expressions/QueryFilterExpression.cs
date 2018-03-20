using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer
{
    /// <summary>
    /// An <see cref="AnnotationExpression"/> that wraps expressions
    /// that represent query filters defined on the model. This annotation
    /// allows us to build our base query expressions without knowledge
    /// of whether or not to include the filters up front.
    /// </summary>
    public class QueryFilterExpression : AnnotationExpression
    {
        public QueryFilterExpression(Expression expression) : base(expression)
        {
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = visitor.Visit(Expression);

            if (expression != Expression)
            {
                return new QueryFilterExpression(expression);
            }

            return this;
        }

        public override int GetAnnotationHashCode() => typeof(QueryFilterExpression).GetHashCode();
    }
}
