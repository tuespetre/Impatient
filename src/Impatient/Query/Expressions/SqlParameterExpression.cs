using Impatient.Extensions;
using Impatient.Query.Infrastructure;
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

        public SqlParameterExpression(Expression expression, bool isNullable, ITypeMapping typeMapping)
            : this(expression, isNullable)
        {
            TypeMapping = typeMapping;
        }

        public override Type Type => Expression.Type;

        public override bool IsNullable { get; }

        public Expression Expression { get; }

        public ITypeMapping TypeMapping { get; }

        public override bool CanReduce => true;

        public override Expression Reduce() => Expression;

        public SqlParameterExpression WithMapping(ITypeMapping mapping)
        {
            if (mapping is null)
            {
                throw new ArgumentNullException(nameof(mapping));
            }

            return new SqlParameterExpression(Expression, IsNullable, mapping);
        }

        public override int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            unchecked
            {
                var hash = IsNullable.GetHashCode();

                hash = (hash * 16777619) ^ comparer.GetHashCode(Expression);
                hash = (hash * 16777619) ^ (TypeMapping?.GetHashCode() ?? 0);

                return hash;
            }
        }
    }
}
