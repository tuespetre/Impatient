using Impatient.Query.Infrastructure;
using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions;

public class SqlAggregateExpression : SqlExpression
{
    public SqlAggregateExpression(string functionName, Expression expression, Type type, bool isDistinct = false, bool hasOpaqueType = false)
    {
        FunctionName = functionName ?? throw new ArgumentNullException(nameof(functionName));
        Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        Type = type ?? throw new ArgumentNullException(nameof(type));
        IsDistinct = isDistinct;
        HasOpaqueType = hasOpaqueType;
    }

    public string FunctionName { get; }

    public bool IsDistinct { get; }

    public bool HasOpaqueType { get; }

    public Expression Expression { get; }

    public override Type Type { get; }

    public SqlAggregateExpression AsDistinct()
    {
        return new SqlAggregateExpression(FunctionName, Expression, Type, true);
    }

    protected override Expression VisitChildren(ExpressionVisitor visitor)
    {
        var expression = visitor.Visit(Expression);

        if (expression != Expression)
        {
            return new SqlAggregateExpression(FunctionName, expression, Type, IsDistinct, HasOpaqueType);
        }

        return this;
    }

    public override int GetSemanticHashCode(ExpressionEqualityComparer comparer)
    {
        unchecked
        {
            var hash = FunctionName.GetHashCode();

            hash = (hash * 16777619) ^ IsDistinct.GetHashCode();
            hash = (hash * 16777619) ^ HasOpaqueType.GetHashCode();
            hash = (hash * 16777619) ^ IsNullable.GetHashCode();

            return hash;
        }
    }
}
