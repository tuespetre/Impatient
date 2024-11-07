using Impatient.EntityFrameworkCore.SqlServer.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Impatient.EntityFrameworkCore.SqlServer.ExpressionVisitors;

public class AsSplitQueryAnnotatingExpressionVisitor : ExpressionVisitor
{
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        if (node.Method.DeclaringType == typeof(RelationalQueryableExtensions)
            && node.Method.Name == nameof(RelationalQueryableExtensions.AsSplitQuery))
        {
            return new AsSplitQueryExpression(Visit(node.Arguments[0]));
        }

        return base.VisitMethodCall(node);
    }
}
