using Impatient.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class TableValuedExpressionTableExpression : AliasedTableExpression
    {
        public TableValuedExpressionTableExpression(Expression expression, string alias, Type type) : base(alias, type)
        {
            if (expression == null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (!expression.Type.IsSequenceType() || expression.Type == typeof(string))
            {
                throw new ArgumentOutOfRangeException(nameof(expression));
            }

            Expression = expression;
        }

        public Expression Expression { get; set; }

        public override IEnumerable<AliasedTableExpression> Flatten()
        {
            yield return this;
        }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = visitor.Visit(Expression);

            if (expression != Expression)
            {
                return new TableValuedExpressionTableExpression(expression, Alias, Type);
            }

            return base.VisitChildren(visitor);
        }
    }
}
