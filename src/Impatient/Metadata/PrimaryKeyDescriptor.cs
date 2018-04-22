using System;
using System.Linq.Expressions;

namespace Impatient.Metadata
{
    public sealed class PrimaryKeyDescriptor
    {
        public PrimaryKeyDescriptor(
            Type targetType, 
            LambdaExpression keySelector)
        {
            TargetType = targetType;
            KeySelector = keySelector;
        }

        public Type TargetType { get; }

        public LambdaExpression KeySelector { get; }
    }
}
