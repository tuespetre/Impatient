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
        }

        public override Type Type => Expression.Type;

        public override bool IsNullable => Expression.Type.IsNullableType() || !Expression.Type.GetTypeInfo().IsValueType;

        public Expression Expression { get; }
    }
}
