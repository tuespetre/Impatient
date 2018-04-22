using Impatient.Extensions;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.Expressions
{
    public class SqlParameterExpression : SqlExpression
    {
        public SqlParameterExpression(Expression expression)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            IsNullable = expression.Type.IsNullableType() || !expression.Type.GetTypeInfo().IsValueType;
        }

        public SqlParameterExpression(Expression expression, bool isNullable)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            IsNullable = isNullable;
        }

        public override Type Type => Expression.Type;

        public override bool IsNullable { get; }

        public Expression Expression { get; }
    }
}
