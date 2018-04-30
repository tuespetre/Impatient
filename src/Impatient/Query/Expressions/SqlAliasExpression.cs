using Impatient.Query.Infrastructure;
using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SqlAliasExpression : SqlExpression
    {
        public SqlAliasExpression(Expression expression, string alias)
        {
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
            Alias = alias ?? throw new ArgumentNullException(nameof(alias));
        }

        public Expression Expression { get; }

        public string Alias { get; }

        public override Type Type => Expression.Type;

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var expression = visitor.VisitAndConvert(Expression, nameof(VisitChildren));

            if (expression != Expression)
            {
                return new SqlAliasExpression(expression, Alias);
            }

            return this;
        }

        public override int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            unchecked
            {
                var hash = Alias.GetHashCode();
                
                hash = (hash * 16777619) ^ IsNullable.GetHashCode();

                return hash;
            }
        }
    }
}
