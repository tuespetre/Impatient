using Impatient.Extensions;
using Impatient.Query.Expressions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Optimizing
{
    public class MemberAccessReducingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitExtension(Expression node)
        {
            switch (node)
            {
                case ExtraPropertyAccessExpression extraPropertyAccessExpression:
                {
                    var (expression, property) = extraPropertyAccessExpression;

                    if (Visit(expression).TryResolvePath(property, out var resolved))
                    {
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

        protected override Expression VisitMember(MemberExpression node)
        {
            var expression = Visit(node.Expression);

            switch (expression)
            {
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

                    return node;
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
                case ExpressionType.TypeAs
                when operand is PolymorphicExpression polymorphicExpression:
                {
                    if (polymorphicExpression.Type.IsAssignableFrom(node.Type))
                    {
                        return polymorphicExpression.Filter(node.Type);
                    }
                    else
                    {
                        return polymorphicExpression.Upcast(node.Type);
                    }
                }

                case ExpressionType.Convert
                when operand is UnaryExpression unaryExpression
                    && unaryExpression.NodeType == ExpressionType.Convert
                    && node.Type.IsAssignableFrom(unaryExpression.Operand.Type)
                    && node.Type != typeof(object)
                    && node.Type != typeof(Enum):
                {
                    return unaryExpression.Operand;
                }

                default:
                {
                    return node.Update(operand);
                }
            }
        }

        private static bool FindExpressionForMember(MemberInitExpression memberInitExpression, MemberInfo memberInfo, out Expression expression)
        {
            var memberBinding
                = memberInitExpression.Bindings
                    .OfType<MemberAssignment>()
                    .SingleOrDefault(b =>
                        b.Member.DeclaringType == memberInfo.DeclaringType
                            && b.Member.Name == memberInfo.Name);

            if (memberBinding != null)
            {
                expression = memberBinding.Expression;

                return true;
            }

            return FindExpressionForMember(memberInitExpression.NewExpression, memberInfo, out expression);
        }

        private static bool FindExpressionForMember(NewExpression newExpression, MemberInfo memberInfo, out Expression expression)
        {
            if (newExpression.Members != null)
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
    }
}
