using System;
using System.Linq.Expressions;

namespace Impatient.Metadata
{
    public sealed class PolymorphicTypeDescriptor : IEquatable<PolymorphicTypeDescriptor>
    {
        public PolymorphicTypeDescriptor(Type type, LambdaExpression test, LambdaExpression materializer)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Test = test ?? throw new ArgumentNullException(nameof(test));
            Materializer = materializer ?? throw new ArgumentNullException(nameof(materializer));
        }

        public Type Type { get; }

        public LambdaExpression Test { get; }

        public LambdaExpression Materializer { get; }

        public bool Equals(PolymorphicTypeDescriptor other)
        {
            return other is not null
                && other.Type == Type
                && other.Test == Test
                && other.Materializer == Materializer;
        }
    }
}
