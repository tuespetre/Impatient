using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.Infrastructure
{
    internal class EmptyRecord
    {
        private EmptyRecord(string empty)
        {
            Empty = empty;
        }

        [PathSegmentName("$empty")]
        public readonly string Empty;

        public static readonly FieldInfo EmptyFieldInfo
            = typeof(EmptyRecord).GetRuntimeField(nameof(Empty));

        public static readonly NewExpression NewExpression
            = Expression.New(
                typeof(EmptyRecord).GetTypeInfo().DeclaredConstructors.Single(c => !c.IsStatic),
                new[] { Expression.Constant(null, typeof(string)) },
                new[] { EmptyFieldInfo });
    }
}
