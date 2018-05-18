using Impatient.Query.Infrastructure;
using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SqlLikeExpression : SqlExpression
    {
        public SqlLikeExpression(Expression target, Expression pattern, Expression escape = null)
        {
            Target = target ?? throw new ArgumentNullException(nameof(target));
            Pattern = pattern ?? throw new ArgumentNullException(nameof(pattern));
            Escape = escape;
        }

        public Expression Target { get; }

        public Expression Pattern { get; }

        public Expression Escape { get; }

        public override Type Type => typeof(bool);

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var target = visitor.Visit(Target);
            var pattern = visitor.Visit(Pattern);
            var escape = visitor.Visit(Escape);

            if (target != Target || pattern != Pattern || escape != Escape)
            {
                return new SqlLikeExpression(target, pattern, escape);
            }

            return this;
        }

        public override int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            unchecked
            {
                var hash = Target.GetHashCode();

                hash = (hash * 16777619) ^ Pattern.GetHashCode();
                hash = (hash * 16777619) ^ (Escape?.GetHashCode() ?? 0);

                return hash;
            }
        }
    }
}
