using Impatient.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class StringMemberRewritingExpressionVisitor : ExpressionVisitor
    {
        protected override Expression VisitMember(MemberExpression node)
        {
            var expression = Visit(node.Expression);

            if (node.Member.DeclaringType == typeof(string))
            {
                switch (node.Member.Name)
                {
                    case nameof(string.Length):
                    {
                        return new SqlFunctionExpression("LEN", typeof(int), expression);
                    }
                }
            }

            return node.Update(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);
            var arguments = Visit(node.Arguments);

            if (node.Method.DeclaringType == typeof(string))
            {
                switch (node.Method.Name)
                {
                    case nameof(string.Concat):
                    {
                        var segments = arguments.ToArray();
                        var parameters = node.Method.GetParameters();

                        // TODO: Support other overloads of string.Concat
                        if (parameters.Select(p => p.ParameterType).All(t => t == typeof(string)))
                        {
                            return new SqlConcatExpression(segments);
                        }
                        else if (parameters[0].ParameterType == typeof(string[]))
                        {
                            if (segments[0] is NewArrayExpression newArrayExpression)
                            {
                                return new SqlConcatExpression(newArrayExpression.Expressions);
                            }
                        }

                        break;
                    }

                    case nameof(string.Trim) when arguments.Count == 0:
                    {
                        return new SqlFunctionExpression(
                            "LTRIM",
                            typeof(string),
                            new SqlFunctionExpression(
                                "RTRIM",
                                typeof(string),
                                @object));
                    }
                }
            }

            return node.Update(@object, arguments);
        }
    }
}
