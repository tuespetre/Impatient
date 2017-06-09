using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SingleValueRelationalQueryExpression : RelationalQueryExpression
    {
        public static readonly SingleValueRelationalQueryExpression SelectOne
            = new SingleValueRelationalQueryExpression(
                new SelectExpression(
                    new ServerProjectionExpression(
                        Constant(1))));

        public SingleValueRelationalQueryExpression(SelectExpression selectExpression)
            : base(
                  selectExpression ?? throw new ArgumentNullException(nameof(selectExpression)),
                  selectExpression.Projection.Type)
        {
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var selectExpression = visitor.VisitAndConvert(SelectExpression, nameof(VisitChildren));

            if (selectExpression != SelectExpression)
            {
                return UpdateSelectExpression(selectExpression);
            }

            return this;
        }

        public SingleValueRelationalQueryExpression UpdateSelectExpression(SelectExpression selectExpression)
        {
            return new SingleValueRelationalQueryExpression(selectExpression);
        }
    }
}
