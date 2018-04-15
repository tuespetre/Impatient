using Impatient.Query.Infrastructure;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

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

        public EnumerableRelationalQueryExpression(
            SelectExpression selectExpression,
            MethodInfo transformationMethod)
            : this(
                  selectExpression,
                  transformationMethod.ReturnType)
        {
            TransformationMethod = transformationMethod ?? throw new ArgumentNullException(nameof(transformationMethod));
        }

        private EnumerableRelationalQueryExpression(SelectExpression selectExpression, Type type)
            : base(selectExpression, type)
        {
        }

        public MethodInfo TransformationMethod { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var selectExpression = visitor.VisitAndConvert(SelectExpression, nameof(VisitChildren));

            if (selectExpression != SelectExpression)
            {
                if (TransformationMethod != null)
                {
                    return new EnumerableRelationalQueryExpression(selectExpression, TransformationMethod);
                }
                else
                {
                    return new EnumerableRelationalQueryExpression(selectExpression, Type);
                }
            }

            return this;
        }

        public EnumerableRelationalQueryExpression UpdateSelectExpression(SelectExpression selectExpression)
        {
            return new EnumerableRelationalQueryExpression(selectExpression);
        }

        public EnumerableRelationalQueryExpression WithTransformationMethod(MethodInfo transformationMethod)
        {
            return new EnumerableRelationalQueryExpression(SelectExpression, transformationMethod);
        }

        public EnumerableRelationalQueryExpression AsOrdered()
        {
            return new EnumerableRelationalQueryExpression(
                SelectExpression, 
                typeof(IOrderedQueryableEnumerable<>).MakeGenericType(SelectExpression.Type));
        }
    }
}
