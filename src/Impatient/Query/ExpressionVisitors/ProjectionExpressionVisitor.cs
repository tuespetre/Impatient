using Impatient.Query.Expressions;
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

        protected virtual Expression VisitLeaf(Expression node) => base.Visit(node);

        protected virtual IEnumerable<string> GetNameParts()
        {
            return CurrentPath.Select(m => m.GetPathSegmentName()).Where(n => n != null && !n.StartsWith("<>"));
        }

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case NewExpression newExpression
                when newExpression.Members != null:
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

                case MemberInitExpression memberInitExpression:
                {
                    var bindings = new MemberBinding[memberInitExpression.Bindings.Count];

                    for (var i = 0; i < memberInitExpression.Bindings.Count; i++)
                    {
                        memberStack.Push(memberInitExpression.Bindings[i].Member);

                        bindings[i] = VisitMemberBinding(memberInitExpression.Bindings[i]);

                        memberStack.Pop();
                    }

                    return memberInitExpression.Update(memberInitExpression.NewExpression, bindings);
                }

                case AnnotationExpression annotationExpression:
                {
                    return base.Visit(annotationExpression);
                }

                case Expression expression when !InLeaf:
                {
                    InLeaf = true;

                    var leaf = VisitLeaf(expression);

                    InLeaf = false;

                    return leaf;
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }
    }
}
