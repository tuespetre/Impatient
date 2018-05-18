using Impatient.Extensions;
using Impatient.Metadata;
using Impatient.Query.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    public abstract class ProjectionExpressionVisitor : ExpressionVisitor
    {
        private Stack<MemberInfo> memberStack = new Stack<MemberInfo>();
        private Stack<string> nameStack = new Stack<string>();

        protected bool InLeaf { get; private set; }

        protected IEnumerable<MemberInfo> CurrentPath => memberStack.Reverse();

        protected virtual Expression VisitLeaf(Expression node) => node;

        protected virtual IEnumerable<string> GetNameParts()
        {
            return nameStack.Reverse().Where(n => n != null && !n.StartsWith("<>"));
        }

        protected bool IsNotLeaf(NewExpression node)
        {
            return node.Members != null;
        }

        protected bool IsNotLeaf(MemberInitExpression node)
        {
            return node.Bindings.Iterate().All(b => b is MemberAssignment)
                && (node.NewExpression.Arguments.Count == 0
                    || node.NewExpression.Members != null);
        }

        public override Expression Visit(Expression node)
        {
            if (InLeaf)
            {
                return base.Visit(node);
            }

            switch (node)
            {
                case NewExpression newExpression when IsNotLeaf(newExpression):
                {
                    var arguments = new Expression[newExpression.Arguments.Count];

                    for (var i = 0; i < newExpression.Arguments.Count; i++)
                    {
                        memberStack.Push(newExpression.Members[i]);
                        nameStack.Push(newExpression.Members[i].GetPathSegmentName());

                        arguments[i] = Visit(newExpression.Arguments[i]);

                        memberStack.Pop();
                        nameStack.Pop();
                    }

                    return newExpression.Update(arguments);
                }

                case MemberInitExpression memberInitExpression when IsNotLeaf(memberInitExpression):
                {
                    var newExpression = memberInitExpression.NewExpression;

                    var arguments = new Expression[newExpression.Arguments.Count];

                    for (var i = 0; i < newExpression.Arguments.Count; i++)
                    {
                        memberStack.Push(newExpression.Members[i]);
                        nameStack.Push(newExpression.Members[i].GetPathSegmentName());

                        arguments[i] = Visit(newExpression.Arguments[i]);

                        memberStack.Pop();
                        nameStack.Pop();
                    }

                    newExpression = newExpression.Update(arguments);

                    var bindings = new MemberBinding[memberInitExpression.Bindings.Count];

                    for (var i = 0; i < memberInitExpression.Bindings.Count; i++)
                    {
                        memberStack.Push(memberInitExpression.Bindings[i].Member);
                        nameStack.Push(memberInitExpression.Bindings[i].Member.GetPathSegmentName());

                        bindings[i] = VisitMemberBinding(memberInitExpression.Bindings[i]);

                        memberStack.Pop();
                        nameStack.Pop();
                    }

                    return memberInitExpression.Update(newExpression, bindings);
                }

                case ExtendedNewExpression newExpression:
                {
                    var arguments = new Expression[newExpression.Arguments.Count];

                    for (var i = 0; i < newExpression.Arguments.Count; i++)
                    {
                        memberStack.Push(newExpression.ReadableMembers[i]);
                        nameStack.Push(newExpression.ReadableMembers[i].GetPathSegmentName());

                        arguments[i] = Visit(newExpression.Arguments[i]);

                        memberStack.Pop();
                        nameStack.Pop();
                    }

                    return newExpression.Update(arguments);
                }

                case ExtendedMemberInitExpression memberInitExpression:
                {
                    var newExpression = memberInitExpression.NewExpression;

                    var arguments = new Expression[newExpression.Arguments.Count];

                    for (var i = 0; i < newExpression.Arguments.Count; i++)
                    {
                        memberStack.Push(newExpression.ReadableMembers[i]);
                        nameStack.Push(newExpression.ReadableMembers[i].GetPathSegmentName());

                        arguments[i] = Visit(newExpression.Arguments[i]);

                        memberStack.Pop();
                        nameStack.Pop();
                    }

                    newExpression = newExpression.Update(arguments);

                    arguments = new Expression[memberInitExpression.Arguments.Count];

                    for (var i = 0; i < memberInitExpression.Arguments.Count; i++)
                    {
                        memberStack.Push(memberInitExpression.ReadableMembers[i]);
                        nameStack.Push(memberInitExpression.ReadableMembers[i].GetPathSegmentName());

                        arguments[i] = Visit(memberInitExpression.Arguments[i]);

                        memberStack.Pop();
                        nameStack.Pop();
                    }

                    return memberInitExpression.Update(newExpression, arguments);
                }

                case NewArrayExpression newArrayExpression:
                {
                    var expressions = new Expression[newArrayExpression.Expressions.Count];

                    for (var i = 0; i < newArrayExpression.Expressions.Count; i++)
                    {
                        nameStack.Push($"${i}");

                        expressions[i] = Visit(newArrayExpression.Expressions[i]);

                        nameStack.Pop();
                    }

                    return newArrayExpression.Update(expressions);
                }

                case PolymorphicExpression polymorphicExpression:
                {
                    var row = Visit(polymorphicExpression.Row);
                    var descriptors = polymorphicExpression.Descriptors.ToArray();

                    for (var i = 0; i < descriptors.Length; i++)
                    {
                        var descriptor = descriptors[i];

                        descriptors[i]
                            = new PolymorphicTypeDescriptor(
                                descriptor.Type,
                                descriptor.Test,
                                Expression.Lambda(
                                    descriptor.Materializer.Body,
                                    descriptor.Materializer.Parameters));
                    }

                    return new PolymorphicExpression(
                        polymorphicExpression.Type,
                        row,
                        descriptors);
                }

                case UnaryExpression unaryExpression
                when unaryExpression.NodeType == ExpressionType.Convert:
                {
                    return unaryExpression.Update(Visit(unaryExpression.Operand));
                }

                case ExtraPropertiesExpression extraPropertiesExpression:
                {
                    var properties = new Expression[extraPropertiesExpression.Properties.Count];

                    for (var i = 0; i < extraPropertiesExpression.Properties.Count; i++)
                    {
                        nameStack.Push(extraPropertiesExpression.Names[i]);

                        properties[i] = Visit(extraPropertiesExpression.Properties[i]);

                        nameStack.Pop();
                    }

                    var expression = Visit(extraPropertiesExpression.Expression);

                    return extraPropertiesExpression.Update(expression, properties);
                }

                case LateBoundProjectionLeafExpression _:
                {
                    return node;
                }

                case AnnotationExpression annotationExpression:
                {
                    return base.Visit(annotationExpression);
                }

                default:
                {
                    InLeaf = true;

                    node = VisitLeaf(node);

                    InLeaf = false;

                    return node;
                }
            }
        }
    }
}
