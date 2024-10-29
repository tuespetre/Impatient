using Impatient.Query.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Rewriting;

public class SqlServerMathMethodRewritingExpressionVisitor : ExpressionVisitor
{
    // This class was pretty much lifted from EFCore's SqlServerMathTranslator.

    private static readonly Dictionary<MethodInfo, string> generalMathMethods = new Dictionary<MethodInfo, string>
    {
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(decimal)]), "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(double)]), "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(float)]), "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(int)]), "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(long)]), "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(sbyte)]), "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Abs), [typeof(short)]), "ABS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), [typeof(decimal)]), "CEILING" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Ceiling), [typeof(double)]), "CEILING" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Floor), [typeof(decimal)]), "FLOOR" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Floor), [typeof(double)]), "FLOOR" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Pow), [typeof(double), typeof(double)]), "POWER" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Exp), [typeof(double)]), "EXP" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Log10), [typeof(double)]), "LOG10" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Log), [typeof(double)]), "LOG" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Log), [typeof(double), typeof(double)]), "LOG" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sqrt), [typeof(double)]), "SQRT" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Acos), [typeof(double)]), "ACOS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Asin), [typeof(double)]), "ASIN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Atan), [typeof(double)]), "ATAN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Atan2), [typeof(double), typeof(double)]), "ATN2" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Cos), [typeof(double)]), "COS" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sin), [typeof(double)]), "SIN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Tan), [typeof(double)]), "TAN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(decimal)]), "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(double)]), "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(float)]), "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(int)]), "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(long)]), "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(sbyte)]), "SIGN" },
        { typeof(Math).GetRuntimeMethod(nameof(Math.Sign), [typeof(short)]), "SIGN" }
    };

    private static readonly IEnumerable<MethodInfo> truncateMethods = new[]
    {
        typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), [typeof(decimal)]),
        typeof(Math).GetRuntimeMethod(nameof(Math.Truncate), [typeof(double)])
    };

    private static readonly IEnumerable<MethodInfo> roundMethods = new[]
    {
        typeof(Math).GetRuntimeMethod(nameof(Math.Round), [typeof(decimal)]),
        typeof(Math).GetRuntimeMethod(nameof(Math.Round), [typeof(double)]),
        typeof(Math).GetRuntimeMethod(nameof(Math.Round), [typeof(decimal), typeof(int)]),
        typeof(Math).GetRuntimeMethod(nameof(Math.Round), [typeof(double), typeof(int)])
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
                [firstArgument, Expression.Constant(0), Expression.Constant(1)]);
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
                    ? [firstArgument, Expression.Constant(0)]
                    : [firstArgument, arguments[1]]);
        }

        return node.Update(@object, arguments);
    }
}
