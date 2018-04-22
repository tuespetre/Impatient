using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SqlCastExpression : SqlExpression
    {
        public SqlCastExpression(Expression expression, string sqlType, Type type) : this(expression, type)
        {
            SqlType = sqlType;
        }

        public SqlCastExpression(Expression expression, Type type)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public Expression Expression { get; }

        public string SqlType { get; }

        public override Type Type { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = visitor.Visit(Expression);

            if (expression != Expression)
            {
                return new SqlCastExpression(expression, SqlType, Type);
            }

            return this;
        }

        public override int GetSemanticHashCode() => (IsNullable, Type).GetHashCode();
    }
}
