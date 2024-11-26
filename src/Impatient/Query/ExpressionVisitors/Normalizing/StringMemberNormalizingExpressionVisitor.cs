using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Normalizing;

public class StringMemberNormalizingExpressionVisitor : ExpressionVisitor
{
    protected override Expression VisitBinary(BinaryExpression node)
    {
        var left = Visit(node.Left);
        var right = Visit(node.Right);

        if (node.NodeType is ExpressionType.Add 
            && node.Method?.DeclaringType == typeof(string)
            && node.Method.Name == nameof(string.Concat))
        {
            return Expression.Call(node.Method, left, right);
        }

        if (NormalizeStringCompare(node.NodeType, left, right, out var result) || NormalizeStringCompare(node.NodeType, right, left, out result))
        {
            return result;
        }

        return node.Update(left, node.Conversion, right);
    }

    private static bool NormalizeStringCompare(ExpressionType nodeType, Expression left, Expression right, out Expression result)
    {
        result = default;

        if (nodeType is not (ExpressionType.Equal or ExpressionType.NotEqual))
        {
            return false;
        }

        if (!(left is MethodCallExpression methodCallExpression
            && methodCallExpression.Method.Name == nameof(string.CompareTo)
            && methodCallExpression.Method.DeclaringType == typeof(string)
            && methodCallExpression.Method.GetParameters().Length == 1))
        {
            return false;
        }

        if (!(right is ConstantExpression constantExpression
            && 0.Equals(constantExpression.Value)))
        {
            return false;
        }

        result = Expression.MakeBinary(nodeType, methodCallExpression.Object, methodCallExpression.Arguments[0]);

        return true;
    }
}
