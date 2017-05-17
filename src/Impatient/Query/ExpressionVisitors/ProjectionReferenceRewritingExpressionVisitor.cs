using Impatient.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors
{
    public class ProjectionReferenceRewritingExpressionVisitor : ExpressionVisitor
    {
        private readonly Stack<MemberInfo> memberStack = new Stack<MemberInfo>();
        private readonly AliasedTableExpression targetTable;

        public ProjectionReferenceRewritingExpressionVisitor(AliasedTableExpression targetTable)
        {
            this.targetTable = targetTable;
        }

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case NewExpression newExpression:
                {
                    if (newExpression.Members == null)
                    {
                        return newExpression;
                    }

                    var arguments = newExpression.Arguments.ToArray();

                    for (var i = 0; i < newExpression.Members.Count; i++)
                    {
                        memberStack.Push(newExpression.Members[i]);
                        arguments[i] = Visit(arguments[i]);
                        memberStack.Pop();
                    }

                    return newExpression.Update(arguments);
                }

                case MemberInitExpression memberInitExpression:
                {
                    var newExpression = (NewExpression)Visit(memberInitExpression.NewExpression);
                    var bindings = memberInitExpression.Bindings.ToArray();

                    for (var i = 0; i < bindings.Length; i++)
                    {
                        memberStack.Push(bindings[i].Member);
                        bindings[i] = VisitMemberBinding(bindings[i]);
                        memberStack.Pop();
                    }

                    return memberInitExpression.Update(newExpression, bindings);
                }

                case AnnotationExpression annotationExpression:
                {
                    return base.Visit(annotationExpression);
                }

                default:
                {
                    var type = default(Type);

                    switch (memberStack.First())
                    {
                        case PropertyInfo propertyInfo:
                        {
                            type = propertyInfo.PropertyType;
                            break;
                        }

                        case FieldInfo fieldInfo:
                        {
                            type = fieldInfo.FieldType;
                            break;
                        }
                    }

                    var alias = string.Join(".", memberStack.Reverse().Select(m => m.Name));

                    return new SqlColumnExpression(targetTable, alias, type);
                }
            }
        }
    }
}
