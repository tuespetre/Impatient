using Impatient.Query.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors
{
    public abstract class ProjectionExpressionVisitor : ExpressionVisitor
    {
        protected Stack<string> CurrentPath { get; } = new Stack<string>();

        protected bool InLeaf { get; private set; }

        protected virtual Expression VisitLeaf(Expression node) => base.Visit(node);

        protected virtual IEnumerable<string> GetNameParts()
        {
            return CurrentPath.Reverse().Where(n => n != null && !n.StartsWith("<>"));
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
                        CurrentPath.Push(newExpression.Members[i].GetPathSegmentName());

                        arguments[i] = Visit(newExpression.Arguments[i]);

                        CurrentPath.Pop();
                    }

                    return newExpression.Update(arguments);
                }

                case MemberInitExpression memberInitExpression:
                {
                    var bindings = new MemberBinding[memberInitExpression.Bindings.Count];

                    for (var i = 0; i < memberInitExpression.Bindings.Count; i++)
                    {
                        CurrentPath.Push(memberInitExpression.Bindings[i].Member.GetPathSegmentName());

                        bindings[i] = VisitMemberBinding(memberInitExpression.Bindings[i]);

                        CurrentPath.Pop();
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
