using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SqlExistsExpression : SqlExpression
    {
        public SqlExistsExpression(SelectExpression selectExpression)
        {
            SelectExpression = selectExpression ?? throw new ArgumentNullException(nameof(selectExpression));
            Type = typeof(bool);
        }

        public SelectExpression SelectExpression { get; }

        public override Type Type { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var selectExpression = visitor.VisitAndConvert(SelectExpression, nameof(VisitChildren));

            if (selectExpression != SelectExpression)
            {
                return new SqlExistsExpression(selectExpression);
            }

            return this;
        }
    }
}
