using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class EnumerableRelationalQueryExpression : RelationalQueryExpression
    {
        public EnumerableRelationalQueryExpression(SelectExpression selectExpression)
            : this(
                  selectExpression,
                  typeof(IQueryable<>).MakeGenericType(selectExpression.Projection.Type))
        {
        }

        private EnumerableRelationalQueryExpression(SelectExpression selectExpression, Type type)
            : base(selectExpression, type)
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

        public EnumerableRelationalQueryExpression UpdateSelectExpression(SelectExpression selectExpression)
        {
            return new EnumerableRelationalQueryExpression(selectExpression);
        }

        public EnumerableRelationalQueryExpression AsUnordered()
        {
            return new EnumerableRelationalQueryExpression(SelectExpression);
        }

        public EnumerableRelationalQueryExpression AsOrdered()
        {
            var orderedType = typeof(IOrderedQueryable<>).MakeGenericType(SelectExpression.Type);

            return new EnumerableRelationalQueryExpression(SelectExpression, orderedType);
        }

        public virtual EnumerableRelationalQueryExpression AsArray()
        {
            var arrayType = SelectExpression.Type.MakeArrayType();

            return new EnumerableRelationalQueryExpression(SelectExpression, arrayType);
        }

        public virtual EnumerableRelationalQueryExpression AsList()
        {
            var listType = typeof(List<>).MakeGenericType(SelectExpression.Type);

            return new EnumerableRelationalQueryExpression(SelectExpression, listType);
        }
    }
}
