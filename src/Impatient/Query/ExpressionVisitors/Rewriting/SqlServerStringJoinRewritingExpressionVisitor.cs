using Impatient.Extensions;
using Impatient.Query.Expressions;
using Impatient.Query.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class SqlServerStringJoinRewritingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo[] supportedMethods = new[]
        {
            ReflectionExtensions.GetMethodInfo(() => string.Join(default(string), new string[0])),
            ReflectionExtensions.GetMethodInfo(() => string.Join(default(string), default(IEnumerable<string>))),
            ReflectionExtensions.GetMethodInfo(() => string.Join(default(string), default(IEnumerable<object>))).GetGenericMethodDefinition(),
        };

        private readonly QueryProcessingContext context;

        public SqlServerStringJoinRewritingExpressionVisitor(QueryProcessingContext context)
        {
            this.context = context ?? throw new ArgumentNullException(nameof(context));
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);
            var arguments = Visit(node.Arguments);

            if (context.Compatibility >= ImpatientCompatibility.SqlServer2017)
            {
                var method = node.Method;

                if (method.IsGenericMethod)
                {
                    method = method.GetGenericMethodDefinition();
                }

                if (supportedMethods.Contains(method)
                    && arguments[1] is EnumerableRelationalQueryExpression query
                    && query.SelectExpression.Projection is ServerProjectionExpression)
                {
                    // TODO: Consider COALESCE for the expression for parity
                    // between SQL Server's behavior and C#'s behavior.

                    return new SingleValueRelationalQueryExpression(
                        query.SelectExpression.UpdateProjection(
                            new ServerProjectionExpression(
                                Expression.Coalesce(
                                    new SqlFunctionExpression(
                                        "STRING_AGG",
                                        typeof(string),
                                        query.SelectExpression.Projection.Flatten().Body,
                                        arguments[0]),
                                    Expression.Constant(string.Empty)))));
                }
            }

            return node.Update(@object, arguments);
        }
    }
}
