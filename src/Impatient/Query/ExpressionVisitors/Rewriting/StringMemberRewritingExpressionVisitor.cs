using Impatient.Query.Expressions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;

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

                    case nameof(string.TrimStart) when ValidateTrimArguments(arguments):
                    {
                        return new SqlFunctionExpression("LTRIM", typeof(string), @object);
                    }

                    case nameof(string.TrimEnd) when ValidateTrimArguments(arguments):
                    {
                        return new SqlFunctionExpression("RTRIM", typeof(string), @object);
                    }

                    case nameof(string.ToUpper):
                    {
                        return new SqlFunctionExpression("UPPER", typeof(string), @object);
                    }

                    case nameof(string.ToLower):
                    {
                        return new SqlFunctionExpression("LOWER", typeof(string), @object);
                    }

                    case nameof(string.Substring):
                    {
                        var startIndex
                            = arguments[0] is ConstantExpression constantStartIndex
                                ? Expression.Constant((int)constantStartIndex.Value + 1)
                                : Expression.Add(arguments[0], Expression.Constant(1)) as Expression;

                        var length
                            = arguments.ElementAtOrDefault(1)
                                ?? new SqlFunctionExpression("LEN", typeof(int), @object);

                        return new SqlFunctionExpression("SUBSTRING", typeof(string), @object, startIndex, length);
                    }

                    case nameof(string.Replace):
                    {
                        return new SqlFunctionExpression("REPLACE", typeof(string), @object, arguments[0], arguments[1]);
                    }

                    case nameof(string.Contains):
                    {
                        return Expression.GreaterThan(
                            new SqlFunctionExpression("CHARINDEX", typeof(int), arguments[0], @object),
                            Expression.Constant(0));
                    }

                    case nameof(string.StartsWith) when arguments.Count == 1:
                    {
                        // TODO: Use LIKE for StartsWith instead

                        return Expression.Equal(
                            new SqlFunctionExpression(
                                "LEFT",
                                typeof(string),
                                @object,
                                new SqlFunctionExpression("LEN", typeof(int), arguments[0])),
                            arguments[0]);
                    }

                    case nameof(string.EndsWith) when arguments.Count == 1:
                    {
                        // Note: 
                        // Don't bother using LIKE, because patterns that contain wildcards
                        // anywhere except the end cannot use an index anyways.

                        return Expression.Equal(
                            new SqlFunctionExpression(
                                "RIGHT",
                                typeof(string),
                                @object,
                                new SqlFunctionExpression("LEN", typeof(int), arguments[0])),
                            arguments[0]);
                    }

                    case nameof(string.IsNullOrEmpty):
                    {
                        return Expression.OrElse(
                            Expression.Equal(
                                arguments[0],
                                Expression.Constant(null, arguments[0].Type)),
                            Expression.Equal(
                                new SqlFunctionExpression("DATALENGTH", typeof(int), arguments[0]),
                                Expression.Constant(0)));
                    }

                    case nameof(string.IsNullOrWhiteSpace):
                    {
                        return Expression.OrElse(
                            Expression.Equal(
                                arguments[0],
                                Expression.Constant(null, arguments[0].Type)),
                            Expression.Equal(
                                // The superfluous convert currently guards against
                                // unnecessary null checking.
                                Expression.Convert(arguments[0], typeof(string)),
                                Expression.Constant(string.Empty)));
                    }
                }
            }

            return node.Update(@object, arguments);
        }

        private static bool ValidateTrimArguments(IList<Expression> arguments)
        {
            return arguments.Count == 1
                && arguments[0] is ConstantExpression constantArgument
                && constantArgument.Value is char[] paramsArgument
                && paramsArgument.Length == 0;
        }
    }
}
