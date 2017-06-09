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
    }
}
