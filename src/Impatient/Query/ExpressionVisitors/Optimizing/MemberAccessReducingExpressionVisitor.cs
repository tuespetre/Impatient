using Impatient.Extensions;
using Impatient.Query.Expressions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Optimizing;

public class MemberAccessReducingExpressionVisitor : ExpressionVisitor
{
    public static readonly MemberAccessReducingExpressionVisitor Instance = new();

    protected override Expression VisitExtension(Expression node)
    {
        switch (node)
        {
            case ExtraPropertyAccessExpression extraPropertyAccessExpression:
            {
                var (expression, property) = extraPropertyAccessExpression;

                if (Visit(expression).TryResolvePath(property, out var resolved))
                {
                    Debug.Assert(node.Type.IsAssignableFrom(resolved.Type));

                    return Visit(resolved);
                }

                goto default;
            }

            default:
            {
                return base.VisitExtension(node);
            }
        }
    }

    protected override Expression VisitIndex(IndexExpression node)
    {
        var @object = Visit(node.Object);
        var arguments = Visit(node.Arguments);

        if (@object.UnwrapInnerExpression() is Expression expression)
        {
            if (expression is PolymorphicExpression polymorphicExpression)
            {
                foreach (var descriptor in polymorphicExpression.Descriptors)
                {
                    if (node.Indexer.DeclaringType.IsAssignableFrom(descriptor.Type))
                    {
                        return Visit(node.Update(descriptor.Materializer.ExpandParameters(polymorphicExpression.Row), arguments));
                    }
                }
            }
            else if (expression is ExtendedMemberInitExpression extendedMemberInitExpression
                && node.Indexer == extendedMemberInitExpression.Indexer
                && arguments.Single() is ConstantExpression constantExpression
                && constantExpression.Value is string indexerKey)
            {
                for (var i = 0; i < extendedMemberInitExpression.IndexerKeys.Count; i++)
                {
                    if (extendedMemberInitExpression.IndexerKeys[i] == indexerKey)
                    {
                        return Visit(extendedMemberInitExpression.IndexerValues[i]);
                    }
                }
            }
        }

        return node.Update(@object, arguments);
    }

    protected override Expression VisitMember(MemberExpression node)
    {
        var expression = Visit(node.Expression);

        switch (expression.UnwrapInnerExpression())
        {
            case ExtendedMemberInitExpression extendedMemberInitExpression
            when FindExpressionForMember(extendedMemberInitExpression, node.Member, out var foundExpression):
            {
                return Visit(foundExpression);
            }

            case ExtendedNewExpression extendedNewExpression
            when FindExpressionForMember(extendedNewExpression, node.Member, out var foundExpression):
            {
                return Visit(foundExpression);
            }

            case MemberInitExpression memberInitExpression
            when FindExpressionForMember(memberInitExpression, node.Member, out var foundExpression):
            {
                return Visit(foundExpression);
            }

            case NewExpression newExpression
            when FindExpressionForMember(newExpression, node.Member, out var foundExpression):
            {
                return Visit(foundExpression);
            }

            case GroupByResultExpression groupByResultExpression
            when node.Member == groupByResultExpression.Type.GetRuntimeProperty("Key"):
            {
                return Visit(groupByResultExpression.OuterKeySelector);
            }

            case GroupedRelationalQueryExpression groupedRelationalQueryExpression
            when node.Member == groupedRelationalQueryExpression.Type.GetRuntimeProperty("Key"):
            {
                return Visit(groupedRelationalQueryExpression.OuterKeySelector);
            }

            case PolymorphicExpression polymorphicExpression:
            {
                foreach (var descriptor in polymorphicExpression.Descriptors)
                {
                    if (node.Member.DeclaringType.IsAssignableFrom(descriptor.Type))
                    {
                        return Visit(node.Update(descriptor.Materializer.ExpandParameters(polymorphicExpression.Row)));
                    }
                }

                return node.Update(expression);
            }

            case ExtraPropertiesExpression extraPropertiesExpression:
            {
                return Visit(node.Update(extraPropertiesExpression.Expression));
            }

            default:
            {
                return node.Update(expression);
            }
        }
    }

    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        var @object = Visit(node.Object);
        var arguments = Visit(node.Arguments);

        if (node.Method.IsSpecialName && @object.UnwrapInnerExpression() is Expression expression)
        {
            if (expression is PolymorphicExpression polymorphicExpression)
            {
                foreach (var descriptor in polymorphicExpression.Descriptors)
                {
                    if (node.Method.DeclaringType.IsAssignableFrom(descriptor.Type))
                    {
                        return Visit(node.Update(descriptor.Materializer.ExpandParameters(polymorphicExpression.Row), arguments));
                    }
                }
            }
            else if (expression is ExtendedMemberInitExpression extendedMemberInitExpression
                && node.Method == extendedMemberInitExpression.Indexer?.GetMethod
                && arguments.Single() is ConstantExpression constantExpression
                && constantExpression.Value is string indexerKey)
            {
                for (var i = 0; i < extendedMemberInitExpression.IndexerKeys.Count; i++)
                {
                    if (extendedMemberInitExpression.IndexerKeys[i] == indexerKey)
                    {
                        return Visit(extendedMemberInitExpression.IndexerValues[i]);
                    }
                }
            }
        }

        return node.Update(@object, arguments);
    }

    protected override Expression VisitUnary(UnaryExpression node)
    {
        var operand = Visit(node.Operand);

        if (operand is UnaryExpression unnecessaryConvert
            && operand.NodeType == ExpressionType.Convert
            && operand.Type == unnecessaryConvert.Operand.Type)
        {
            operand = unnecessaryConvert.Operand;
        }

        switch (node.NodeType)
        {
            case ExpressionType.Convert
            when operand is UnaryExpression unaryExpression
                && unaryExpression.NodeType == ExpressionType.Convert
                && node.Type.IsAssignableFrom(unaryExpression.Operand.Type)
                && node.Type != typeof(object)
                && node.Type != typeof(Enum):
            {
                return unaryExpression.Operand;
            }

            case ExpressionType.TypeAs:
            case ExpressionType.Convert:
            {
                if (operand is PolymorphicExpression polymorphicExpression)
                {
                    if (polymorphicExpression.Type.IsAssignableFrom(node.Type))
                    {
                        return polymorphicExpression.Filter(node.Type);
                    }
                    else if (node.Type != typeof(object))
                    {
                        return polymorphicExpression.Upcast(node.Type);
                    }
                }

                goto default;
            }

            default:
            {
                return node.Update(operand);
            }
        }
    }

    private static bool FindExpressionForMember(MemberInitExpression memberInitExpression, MemberInfo memberInfo, out Expression expression)
    {
        for (var i = 0; i < memberInitExpression.Bindings.Count; i++)
        {
            var binding = memberInitExpression.Bindings[i];

            if (binding is MemberAssignment assignment
                && binding.Member.DeclaringType == memberInfo.DeclaringType
                && binding.Member.Name == memberInfo.Name)
            {
                expression = assignment.Expression;

                return true;
            }
        }

        return FindExpressionForMember(memberInitExpression.NewExpression, memberInfo, out expression);
    }

    private static bool FindExpressionForMember(NewExpression newExpression, MemberInfo memberInfo, out Expression expression)
    {
        if (newExpression.Members is not null)
        {
            for (var i = 0; i < newExpression.Members.Count; i++)
            {
                var member = newExpression.Members[i];

                if (member.DeclaringType == memberInfo.DeclaringType
                    && member.Name == memberInfo.Name)
                {
                    expression = newExpression.Arguments[i];

                    return true;
                }
            }
        }

        expression = null;

        return false;
    }

    private static bool FindExpressionForMember(ExtendedMemberInitExpression extendedMemberInitExpression, MemberInfo memberInfo, out Expression expression)
    {
        for (var i = 0; i < extendedMemberInitExpression.ReadableMembers.Count; i++)
        {
            var member = extendedMemberInitExpression.ReadableMembers[i];

            if (member.DeclaringType == memberInfo.DeclaringType
                && member.Name == memberInfo.Name)
            {
                expression = extendedMemberInitExpression.Arguments[i];

                return true;
            }
        }

        return FindExpressionForMember(extendedMemberInitExpression.NewExpression, memberInfo, out expression);
    }

    private static bool FindExpressionForMember(ExtendedNewExpression extendedNewExpression, MemberInfo memberInfo, out Expression expression)
    {
        for (var i = 0; i < extendedNewExpression.ReadableMembers.Count; i++)
        {
            var member = extendedNewExpression.ReadableMembers[i];

            if (member is not null
                && member.DeclaringType == memberInfo.DeclaringType
                && member.Name == memberInfo.Name)
            {
                expression = extendedNewExpression.Arguments[i];

                return true;
            }
        }

        expression = null;

        return false;
    }
}
