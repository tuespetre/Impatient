using Impatient.Query.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors
{
    public class DbFunctionRewritingExpressionVisitor : ExpressionVisitor
    {
        private static readonly MethodInfo likeMethodInfo
            = typeof(DbFunctionsExtensions).GetRuntimeMethod(
                nameof(DbFunctionsExtensions.Like),
                new[] { typeof(DbFunctions), typeof(string), typeof(string) });

        private static readonly MethodInfo likeWithEscapeMethodInfo
            = typeof(DbFunctionsExtensions).GetRuntimeMethod(
                nameof(DbFunctionsExtensions.Like),
                new[] { typeof(DbFunctions), typeof(string), typeof(string), typeof(string) });

        private readonly IModel model;

        public DbFunctionRewritingExpressionVisitor(IModel model)
        {
            this.model = model ?? throw new ArgumentNullException(nameof(model));
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);
            var arguments = Visit(node.Arguments);

            if (node.Method == likeMethodInfo || node.Method == likeWithEscapeMethodInfo)
            {
                return new SqlLikeExpression(arguments[1], arguments[2], arguments.ElementAtOrDefault(3));
            }
            
            var function = model.Relational().FindDbFunction(node.Method);

            if (function != null)
            {
                if (function.Translation != null)
                {
                    return function.Translation.Invoke(arguments);
                }
                else
                {
                    return new SqlFunctionExpression(function.Schema, function.FunctionName, node.Type, arguments);
                }
            }

            return node.Update(@object, arguments);
        }
    }
}
