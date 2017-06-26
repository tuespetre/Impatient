using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors
{
    public abstract class ProjectionExpressionVisitor : ExpressionVisitor
    {
        private Stack<MemberInfo> memberStack = new Stack<MemberInfo>();

        protected bool InLeaf { get; private set; }

        protected IEnumerable<MemberInfo> CurrentPath => memberStack.Reverse();

        protected virtual Expression VisitLeaf(Expression node) => node;

        protected virtual IEnumerable<string> GetNameParts()
        {
            return CurrentPath.Select(m => m.GetPathSegmentName()).Where(n => n != null && !n.StartsWith("<>"));
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

                        arguments[i] = Visit(newExpression.Arguments[i]);

                        memberStack.Pop();
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

                        arguments[i] = Visit(newExpression.Arguments[i]);

                        memberStack.Pop();
                    }

                    newExpression = newExpression.Update(arguments);

                    var bindings = new MemberBinding[memberInitExpression.Bindings.Count];

                    for (var i = 0; i < memberInitExpression.Bindings.Count; i++)
                    {
                        memberStack.Push(memberInitExpression.Bindings[i].Member);

                        bindings[i] = VisitMemberBinding(memberInitExpression.Bindings[i]);

                        memberStack.Pop();
                    }

                    return memberInitExpression.Update(newExpression, bindings);
                }

                case PolymorphicExpression polymorphicExpression:
                {
                    var row = Visit(polymorphicExpression.Row);

                    return new PolymorphicExpression(
                        polymorphicExpression.Type,
                        polymorphicExpression.Row,
                        polymorphicExpression.Descriptors);
                }

                case DefaultIfEmptyExpression defaultIfEmptyExpression
                when defaultIfEmptyExpression.Flag != null:
                {
                    memberStack.Push(EmptyRecord.EmptyFieldInfo);

                    InLeaf = true;

                    var flag = VisitLeaf(defaultIfEmptyExpression.Flag);

                    InLeaf = false;

                    memberStack.Pop();

                    var expression = Visit(defaultIfEmptyExpression.Expression);

                    return defaultIfEmptyExpression.Update(expression, flag);
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
