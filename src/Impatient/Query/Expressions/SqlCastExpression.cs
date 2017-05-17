using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SqlCastExpression : SqlExpression
    {
        public SqlCastExpression(Expression expression, string sqlType, Type type)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            SqlType = sqlType ?? throw new ArgumentNullException(nameof(sqlType));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public Expression Expression { get; }

        public string SqlType { get; }

        public override Type Type { get; }
    }
}
