using Impatient.Extensions;
using Impatient.Query.Expressions;
using System;
using System.Collections.Generic;
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
                        return new SqlCastExpression(
                            new SqlFunctionExpression("LEN", typeof(long), expression),
                            typeof(int));
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

                    case nameof(string.Trim)
                    when arguments.Count == 0:
                    {
                        return new SqlFunctionExpression(
                            "LTRIM",
                            typeof(string),
                            new SqlFunctionExpression(
                                "RTRIM",
                                typeof(string),
                                @object));
                    }

                    case nameof(string.TrimStart)
                    when ValidateTrimArguments(arguments):
                    {
                        return new SqlFunctionExpression("LTRIM", typeof(string), @object);
                    }

                    case nameof(string.TrimEnd)
                    when ValidateTrimArguments(arguments):
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
                        var startIndex = GetStartIndex(arguments[0]);

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
                        Expression result
                            = Expression.GreaterThan(
                                new SqlFunctionExpression("CHARINDEX", typeof(int), arguments[0], @object),
                                Expression.Constant(0));

                        if (arguments[0] is ConstantExpression constant)
                        {
                            if (constant.Value is null)
                            {
                                result
                                    = Expression.Equal(
                                        new SqlFragmentExpression("NULL", typeof(string)),
                                        new SqlFragmentExpression("NULL", typeof(string)));
                            }
                            else if (constant.Value.Equals(string.Empty))
                            {
                                result = Expression.Constant(true);
                            }
                        }
                        else
                        {
                            result
                                = Expression.OrElse(
                                    Expression.Equal(
                                        new SqlFunctionExpression("LEN", typeof(int), arguments[0]),
                                        Expression.Constant(0)),
                                    result);
                        }

                        return result;
                    }

                    case nameof(string.IndexOf)
                    when arguments.Last().Type != typeof(StringComparison):
                    {
                        var value = arguments[0];

                        if (value is ConstantExpression constant)
                        {
                            switch (constant.Value)
                            {
                                case "":
                                {
                                    return Expression.Constant(0);
                                }

                                case null:
                                {
                                    return Expression.Constant(-1);
                                }
                            }
                        }

                        var newArguments = new List<Expression>
                        {
                            value,
                            @object,
                            GetStartIndex(arguments.ElementAtOrDefault(1))
                        };

                        if (arguments.Count == 3)
                        {
                            newArguments.Add(arguments[2]);
                        }

                        return Expression.Subtract(
                            new SqlFunctionExpression("CHARINDEX", typeof(int), newArguments.ToArray()),
                            Expression.Constant(1));
                    }

                    case nameof(string.StartsWith)
                    when arguments.Count == 1:
                    {
                        // TODO: consider LIKE for StartsWith instead

                        // TODO: consider char argument

                        Expression result
                            = Expression.Equal(
                                new SqlFunctionExpression(
                                    "LEFT",
                                    typeof(string),
                                    @object,
                                    new SqlFunctionExpression("LEN", typeof(int), arguments[0])),
                                arguments[0]);

                        if (arguments[0] is ConstantExpression constant)
                        {
                            if (constant.Value is null)
                            {
                                result
                                    = Expression.Equal(
                                        new SqlFragmentExpression("NULL", typeof(string)),
                                        new SqlFragmentExpression("NULL", typeof(string)));
                            }
                            else if (constant.Value.Equals(string.Empty))
                            {
                                result = Expression.Constant(true);
                            }
                        }
                        else
                        {
                            result
                                = Expression.OrElse(
                                    Expression.Equal(
                                        new SqlFunctionExpression("LEN", typeof(int), arguments[0]),
                                        Expression.Constant(0)),
                                    result);
                        }

                        return result;
                    }

                    case nameof(string.EndsWith)
                    when arguments.Count == 1:
                    {
                        // Note: 
                        // Don't bother using LIKE, because patterns that contain wildcards
                        // anywhere except the end cannot use an index anyways.

                        // TODO: consider char argument

                        Expression result
                            = Expression.Equal(
                                new SqlFunctionExpression(
                                    "RIGHT",
                                    typeof(string),
                                    @object,
                                    new SqlFunctionExpression("LEN", typeof(int), arguments[0])),
                                arguments[0]);

                        if (arguments[0] is ConstantExpression constant)
                        {
                            if (constant.Value is null)
                            {
                                result
                                    = Expression.Equal(
                                        new SqlFragmentExpression("NULL", typeof(string)),
                                        new SqlFragmentExpression("NULL", typeof(string)));
                            }
                            else if (constant.Value.Equals(string.Empty))
                            {
                                result = Expression.Constant(true);
                            }
                        }
                        else
                        {
                            result
                                = Expression.OrElse(
                                    Expression.Equal(
                                        new SqlFunctionExpression("LEN", typeof(int), arguments[0]),
                                        Expression.Constant(0)),
                                    result);
                        }

                        return result;
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
                        var argument = arguments[0];

                        if (argument.UnwrapInnerExpression() is SqlColumnExpression sqlColumnExpression)
                        {
                            argument
                                = new SqlColumnExpression(
                                    sqlColumnExpression.Table,
                                    sqlColumnExpression.ColumnName,
                                    sqlColumnExpression.Type,
                                    false,
                                    null);
                        }

                        return Expression.OrElse(
                            Expression.Equal(
                                arguments[0],
                                Expression.Constant(null, arguments[0].Type)),
                            Expression.Equal(
                                argument,
                                Expression.Constant(string.Empty)));
                    }
                }
            }

            return node.Update(@object, arguments);
        }

        private static Expression GetStartIndex(Expression argument)
        {
            return argument == null
                ? Expression.Constant(1)
                : argument is ConstantExpression constantStartIndex
                    ? Expression.Constant((int)constantStartIndex.Value + 1)
                    : argument is SqlParameterExpression sqlParameterExpression
                        ? Expression.Add(argument, Expression.Constant(1))
                        : argument is SqlExpression sqlExpression
                            ? sqlExpression
                            : Expression.Add(argument, Expression.Constant(1)) as Expression;
        }

        private static bool ValidateTrimArguments(IList<Expression> arguments)
        {
            if (arguments.Count == 1
                && arguments[0] is ConstantExpression constantArgument
                && constantArgument.Value is char[] paramsArgument
                && paramsArgument.Length == 0)
            {
                return true;
            }
            else if (arguments.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
