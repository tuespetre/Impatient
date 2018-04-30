using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class ThenOrderByExpression : OrderByExpression
    {
        public ThenOrderByExpression(OrderByExpression previous, Expression expression, bool descending)
            : base(expression, descending)
        {
            Previous = previous ?? throw new ArgumentNullException(nameof(previous));
        }

        public OrderByExpression Previous { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var previous = visitor.VisitAndConvert(Previous, nameof(VisitChildren));
            var expression = visitor.VisitAndConvert(Expression, nameof(VisitChildren));

            if (previous != Previous || expression != Expression)
            {
                return new ThenOrderByExpression(previous, expression, Descending);
            }

            return this;
        }

        public override OrderByExpression Reverse()
        {
            return new ThenOrderByExpression(Previous.Reverse(), Expression, !Descending);
        }

        public override IEnumerable<OrderByExpression> Iterate()
        {
            var current = this as OrderByExpression;

            while (current is ThenOrderByExpression thenOrderByExpression)
            {
                yield return thenOrderByExpression;

                current = thenOrderByExpression.Previous;
            }

            yield return current;
        }

        public override int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            unchecked
            {
                var hash = comparer.GetHashCode(Previous);

                hash = (hash * 16777619) ^ comparer.GetHashCode(Expression);
                hash = (hash * 16777619) ^ Descending.GetHashCode();

                return hash;
            }
        }
    }
}
