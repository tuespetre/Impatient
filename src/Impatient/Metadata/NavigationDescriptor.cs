using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Metadata;

// TODO: add some constraint checking in the constructor. null guards, expression type validation, etc.

public sealed class NavigationDescriptor
{
    public NavigationDescriptor(
        MemberInfo member,
        bool isNullable,
        Expression expansion,
        LambdaExpression outerKeySelector,
        LambdaExpression innerKeySelector,
        LambdaExpression resultSelector)
    {
        Member = member;
        IsNullable = isNullable;
        Expansion = expansion;
        OuterKeySelector = outerKeySelector;
        InnerKeySelector = innerKeySelector;
        ResultSelector = resultSelector;
    }

    public MemberInfo Member { get; }

    public bool IsNullable { get; }

    public Expression Expansion { get; }

    public LambdaExpression OuterKeySelector { get; }

    public LambdaExpression InnerKeySelector { get; }

    public LambdaExpression ResultSelector { get; }
}
