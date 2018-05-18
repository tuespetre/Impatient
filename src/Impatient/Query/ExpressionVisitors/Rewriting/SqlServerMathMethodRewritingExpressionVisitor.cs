using Impatient.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
    public class SqlServerMathMethodRewritingExpressionVisitor : ExpressionVisitor
    {
        // This class was pretty much lifted from EFCore's SqlServerMathTranslator.

        private static readonly Dictionary<MethodInfo, string> generalMathMethods = new Dictionary<MethodInfo, string>
        {
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(decimal) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(double) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(float) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(int) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(long) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(sbyte) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), new[] { typeof(short) }), "ABS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(decimal) }), "CEILING" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), new[] { typeof(double) }), "CEILING" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(decimal) }), "FLOOR" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Floor), new[] { typeof(double) }), "FLOOR" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Pow), new[] { typeof(double), typeof(double) }), "POWER" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Exp), new[] { typeof(double) }), "EXP" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Log10), new[] { typeof(double) }), "LOG10" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double) }), "LOG" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Log), new[] { typeof(double), typeof(double) }), "LOG" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sqrt), new[] { typeof(double) }), "SQRT" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Acos), new[] { typeof(double) }), "ACOS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Asin), new[] { typeof(double) }), "ASIN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Atan), new[] { typeof(double) }), "ATAN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Atan2), new[] { typeof(double), typeof(double) }), "ATN2" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Cos), new[] { typeof(double) }), "COS" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sin), new[] { typeof(double) }), "SIN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Tan), new[] { typeof(double) }), "TAN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(decimal) }), "SIGN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(double) }), "SIGN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(float) }), "SIGN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(int) }), "SIGN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(long) }), "SIGN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(sbyte) }), "SIGN" },
            { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), new[] { typeof(short) }), "SIGN" }
        };

        private static readonly IEnumerable<MethodInfo> truncateMethods = new[]
        {
            typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), new[] { typeof(decimal) }),
            typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), new[] { typeof(double) })
        };

        private static readonly IEnumerable<MethodInfo> roundMethods = new[]
        {
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal) }),
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double) }),
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(decimal), typeof(int) }),
            typeof(Math).GetRuntimeMethod(nameof(Math.Round), new[] { typeof(double), typeof(int) })
        };

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            var @object = Visit(node.Object);
            var arguments = Visit(node.Arguments);

            if (generalMathMethods.TryGetValue(node.Method, out var sqlFunctionName))
            {
                return new SqlFunctionExpression(sqlFunctionName, node.Type, arguments.ToArray());
            }
            else if (truncateMethods.Contains(node.Method))
            {
                var firstArgument = arguments[0];

                if (firstArgument.NodeType == ExpressionType.Convert)
                {
                    firstArgument = new SqlCastExpression(firstArgument, firstArgument.Type);
                }

                return new SqlFunctionExpression(
                    "ROUND",
                    node.Type,
                    new[] { firstArgument, Expression.Constant(0), Expression.Constant(1) });
            }
            else if (roundMethods.Contains(node.Method))
            {
                var firstArgument = arguments[0];

                if (firstArgument.NodeType == ExpressionType.Convert)
                {
                    firstArgument = new SqlCastExpression(firstArgument, firstArgument.Type);
                }

                return new SqlFunctionExpression(
                    "ROUND",
                    node.Type,
                    arguments.Count == 1
                        ? new[] { firstArgument, Expression.Constant(0) }
                        : new[] { firstArgument, arguments[1] });
            }

            return node.Update(@object, arguments);
        }
    }
}
