using Impatient.Query.Expressions;
using Impatient.Query.ExpressionVisitors.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    // TODO: Inner expressions/arguments HAVE to be checked for translatability first.
    public class StringMemberRewritingExpressionVisitor : ExpressionVisitor
    {
        private readonly TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor;

        public StringMemberRewritingExpressionVisitor(TranslatabilityAnalyzingExpressionVisitor translatabilityAnalyzingExpressionVisitor)
        {
            this.translatabilityAnalyzingExpressionVisitor = translatabilityAnalyzingExpressionVisitor;
        }

        private bool IsTranslatable(Expression node) => translatabilityAnalyzingExpressionVisitor.Visit(node) is TranslatableExpression;

        protected override Expression VisitMember(MemberExpression node)
        {
            var expression = Visit(node.Expression);

            if (IsTranslatable(expression) && node.Member.DeclaringType == typeof(string))
            {
                switch (node.Member.Name)
                {
                    case nameof(string.Length):
                    {
                        return new SqlCastExpression(
                            new SqlFunctionExpression("LEN", typeof(long), expression),
                            "int",
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
                            if (segments.All(IsTranslatable))
                            {
                                return new SqlConcatExpression(segments);
                            }
                        }
                        else if (parameters[0].ParameterType == typeof(string[]))
                        {
                            if (segments[0] is NewArrayExpression newArrayExpression
                                && newArrayExpression.Expressions.All(IsTranslatable))
                            {
                                return new SqlConcatExpression(newArrayExpression.Expressions);
                            }
                        }

                        break;
                    }

                    case nameof(string.Trim) 
                    when arguments.Count == 0 && IsTranslatable(@object):
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
                    when ValidateTrimArguments(arguments) && IsTranslatable(@object):
                    {
                        return new SqlFunctionExpression("LTRIM", typeof(string), @object);
                    }

                    case nameof(string.TrimEnd) 
                    when ValidateTrimArguments(arguments) && IsTranslatable(@object):
                    {
                        return new SqlFunctionExpression("RTRIM", typeof(string), @object);
                    }

                    case nameof(string.ToUpper) 
                    when IsTranslatable(@object):
                    {
                        return new SqlFunctionExpression("UPPER", typeof(string), @object);
                    }

                    case nameof(string.ToLower) 
                    when IsTranslatable(@object):
                    {
                        return new SqlFunctionExpression("LOWER", typeof(string), @object);
                    }

                    case nameof(string.Substring) 
                    when IsTranslatable(@object) && arguments.All(IsTranslatable):
                    {
                        var startIndex = GetStartIndex(arguments[0]);

                        var length
                            = arguments.ElementAtOrDefault(1)
                                ?? new SqlFunctionExpression("LEN", typeof(int), @object);

                        return new SqlFunctionExpression("SUBSTRING", typeof(string), @object, startIndex, length);
                    }

                    case nameof(string.Replace) 
                    when IsTranslatable(@object) && arguments.All(IsTranslatable):
                    {
                        return new SqlFunctionExpression("REPLACE", typeof(string), @object, arguments[0], arguments[1]);
                    }

                    case nameof(string.Contains) 
                    when IsTranslatable(@object) && arguments.All(IsTranslatable):
                    {
                        return Expression.GreaterThan(
                            new SqlFunctionExpression("CHARINDEX", typeof(int), arguments[0], @object),
                            Expression.Constant(0));
                    }

                    case nameof(string.IndexOf)
                    when arguments.Last().Type != typeof(StringComparison) 
                        && IsTranslatable(@object) 
                        && arguments.All(IsTranslatable):
                    {
                        var newArguments = new List<Expression>
                        {
                            arguments[0],
                            @object,
                            GetStartIndex(arguments.ElementAtOrDefault(1))
                        };

                        if (arguments.Count == 3)
                        {
                            newArguments.Add(arguments[2]);
                        }

                        return new SqlFunctionExpression("CHARINDEX", typeof(int), newArguments.ToArray());
                    }

                    case nameof(string.StartsWith) 
                    when arguments.Count == 1
                        && IsTranslatable(@object)
                        && arguments.All(IsTranslatable):
                    {
                        // TODO: consider LIKE for StartsWith instead

                        // TODO: consider char argument

                        return Expression.Equal(
                            new SqlFunctionExpression(
                                "LEFT",
                                typeof(string),
                                @object,
                                new SqlFunctionExpression("LEN", typeof(int), arguments[0])),
                            arguments[0]);
                    }

                    case nameof(string.EndsWith) 
                    when arguments.Count == 1
                        && IsTranslatable(@object)
                        && arguments.All(IsTranslatable):
                    {
                        // Note: 
                        // Don't bother using LIKE, because patterns that contain wildcards
                        // anywhere except the end cannot use an index anyways.
                        
                        // TODO: consider char argument

                        return Expression.Equal(
                            new SqlFunctionExpression(
                                "RIGHT",
                                typeof(string),
                                @object,
                                new SqlFunctionExpression("LEN", typeof(int), arguments[0])),
                            arguments[0]);
                    }

                    case nameof(string.IsNullOrEmpty)
                    when arguments.All(IsTranslatable):
                    {
                        return Expression.OrElse(
                            Expression.Equal(
                                arguments[0],
                                Expression.Constant(null, arguments[0].Type)),
                            Expression.Equal(
                                new SqlFunctionExpression("DATALENGTH", typeof(int), arguments[0]),
                                Expression.Constant(0)));
                    }

                    case nameof(string.IsNullOrWhiteSpace)
                    when arguments.All(IsTranslatable):
                    {
                        var argument = arguments[0];

                        if (argument.UnwrapAnnotationsAndConversions() is SqlColumnExpression sqlColumnExpression)
                        {
                            argument
                                = new SqlColumnExpression(
                                    sqlColumnExpression.Table,
                                    sqlColumnExpression.ColumnName,
                                    sqlColumnExpression.Type,
                                    false);
                        }

                        return Expression.OrElse(
                            Expression.Equal(
                                arguments[0],
                                Expression.Constant(null, arguments[0].Type)),
                            Expression.Equal(
                                // The superfluous convert currently guards against
                                // unnecessary null checking.
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
