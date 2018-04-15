using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SqlParameterExpression : SqlExpression
    {
        public SqlParameterExpression(Expression expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public override Type Type => Expression.Type;

        public override bool IsNullable => Expression.Type.IsNullableType();

        public Expression Expression { get; }
    }
}
