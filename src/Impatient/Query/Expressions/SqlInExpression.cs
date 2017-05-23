using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SqlInExpression : SqlExpression
    {
        public SqlInExpression(Expression value, Expression values)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Values = values ?? throw new ArgumentNullException(nameof(values));
        }

        public Expression Value { get; }

        public Expression Values { get; }

        public override Type Type => typeof(bool);

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var value = visitor.Visit(Value);
            var values = visitor.Visit(Values);

            if (value != Value || values != Values)
            {
                return new SqlInExpression(value, values);
            }

            return this;
        }
    }
}
