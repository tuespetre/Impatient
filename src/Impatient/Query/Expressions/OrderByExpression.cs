﻿using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class OrderByExpression : Expression, ISemanticHashCodeProvider
    {
        public OrderByExpression(Expression expression, bool descending)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Descending = descending;
        }

        public Expression Expression { get; }

        public bool Descending { get; }

        public override Type Type => Expression.Type;

        public override ExpressionType NodeType => ExpressionType.Extension;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = visitor.VisitAndConvert(Expression, nameof(VisitChildren));

            if (expression != Expression)
            {
                return new OrderByExpression(expression, Descending);
            }

            return this;
        }

        public virtual OrderByExpression Reverse()
        {
            return new OrderByExpression(Expression, !Descending);
        }

        public virtual IEnumerable<OrderByExpression> Iterate()
        {
            yield return this;
        }

        public virtual OrderByExpression Combine(OrderByExpression other)
        {
            if (other is null)
            {
                return this;
            }

            var result = this;

            foreach (var orderByExpression in other.Iterate().Reverse())
            {
                result = new ThenOrderByExpression(result, orderByExpression.Expression, orderByExpression.Descending);
            }

            return result;
        }

        public virtual int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            unchecked
            {
                var hash = comparer.GetHashCode(Expression);

                hash = (hash * 16777619) ^ Descending.GetHashCode();

                return hash;
            }
        }
    }
}
