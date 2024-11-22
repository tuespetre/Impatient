using Impatient.Extensions;
using Impatient.Query.Expressions;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Rewriting;

public class SqlServerJsonMemberRewritingExpressionVisitor : ExpressionVisitor
{
    protected override Expression VisitMember(MemberExpression node)
    {
        var path = new List<MemberInfo>();
        var root = default(Expression);
        var current = node;

        do
        {
            path.Insert(0, current.Member);
            root = current.Expression;
            current = root as MemberExpression;
        }
        while (current is not null);

        switch (root.UnwrapInnerExpression())
        {
            case SqlExpression sqlExpression
            when !sqlExpression.Type.IsScalarType():
            {
                if (node.Type.IsScalarType())
                {
                    var jsonValue = new SqlFunctionExpression(
                        "JSON_VALUE",
                        typeof(string),
                        sqlExpression,
                        Expression.Constant(GetJsonPath(path)));

                    if (node.Type == typeof(string))
                    {
                        return jsonValue;
                    }
                    else if (node.Type.IsBooleanType())
                    {
                        return Expression.Equal(jsonValue, Expression.Constant("true"));
                    }
                    else
                    {
                        return new SqlCastExpression(jsonValue, node.Type);
                    }
                }
                else
                {
                    return new SqlFunctionExpression(
                        "JSON_QUERY",
                        node.Type,
                        sqlExpression,
                        Expression.Constant(GetJsonPath(path)));
                }
            }

            default:
            {
                return base.VisitMember(node);
            }
        }
    }

    private static string GetJsonPath(List<MemberInfo> path)
    {
        return $"$.{string.Join(".", path.GetPropertyNamesForJson())}";
    }
}
