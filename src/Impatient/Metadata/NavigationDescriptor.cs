using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Metadata
{
    public sealed class NavigationDescriptor
    {
        public NavigationDescriptor(
            Type type,
            MemberInfo member,
            LambdaExpression outerKeySelector,
            LambdaExpression innerKeySelector,
            bool isNullable,
            Expression expansion)
        {
            Type = type;
            Member = member;
            OuterKeySelector = outerKeySelector;
            InnerKeySelector = innerKeySelector;
            IsNullable = isNullable;
            Expansion = expansion;
        }

        public Type Type { get; }

        public MemberInfo Member { get; }

        public LambdaExpression OuterKeySelector { get; }

        public LambdaExpression InnerKeySelector { get; }

        public bool IsNullable { get; }

        public Expression Expansion { get; }
    }
}
