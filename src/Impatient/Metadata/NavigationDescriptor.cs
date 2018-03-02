using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Metadata
{
    public class NavigationDescriptor
    {
        public Type Type;
        public MemberInfo Member;
        public LambdaExpression OuterKeySelector;
        public LambdaExpression InnerKeySelector;
        public bool IsNullable;
        public Expression Expansion;
    }
}
