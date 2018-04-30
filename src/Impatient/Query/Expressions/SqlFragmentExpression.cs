using Impatient.Query.Infrastructure;
using System;

namespace Impatient.Query.Expressions
{
    public class SqlFragmentExpression : SqlExpression
    {
        public SqlFragmentExpression(string fragment) : this(fragment, typeof(void))
        {
        }

        public SqlFragmentExpression(string fragment, Type type)
        {
            Fragment = fragment ?? throw new ArgumentNullException(nameof(fragment));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public string Fragment { get; }

        public override Type Type { get; }

        public override int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            unchecked
            {
                var hash = Fragment.GetHashCode();
                
                hash = (hash * 16777619) ^ IsNullable.GetHashCode();

                return hash;
            }
        }
    }
}
