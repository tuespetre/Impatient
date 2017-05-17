using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SqlFragmentExpression : SqlExpression
    {
        public SqlFragmentExpression(string fragment, Type type)
        {
            Fragment = fragment ?? throw new ArgumentNullException(nameof(fragment));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public string Fragment { get; }

        public override Type Type { get; }
    }
}
