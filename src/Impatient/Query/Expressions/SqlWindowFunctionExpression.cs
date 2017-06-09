using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SqlWindowFunctionExpression : SqlExpression
    {
        public SqlWindowFunctionExpression(SqlFunctionExpression function, OrderByExpression ordering)
        {
            Function = function ?? throw new ArgumentNullException(nameof(function));
            Ordering = ordering ?? throw new ArgumentNullException(nameof(ordering));
        }

        public SqlFunctionExpression Function { get; }

        public OrderByExpression Ordering { get; }

        public override Type Type => Function.Type;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var function = visitor.VisitAndConvert(Function, nameof(VisitChildren));
            var ordering = visitor.VisitAndConvert(Ordering, nameof(VisitChildren));

            if (function != Function || ordering != Ordering)
            {
                return new SqlWindowFunctionExpression(function, ordering);
            }

            return this;
        }
    }
}
