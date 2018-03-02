using System;
using System.Linq.Expressions;

namespace Impatient.Metadata
{
    public class PrimaryKeyDescriptor
    {
        public Type TargetType;
        public LambdaExpression KeySelector;
    }
}
