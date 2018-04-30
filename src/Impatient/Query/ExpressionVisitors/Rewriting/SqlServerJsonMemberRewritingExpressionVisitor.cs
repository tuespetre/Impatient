using Impatient.Extensions;
using Impatient.Query.Expressions;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Impatient.Query.ExpressionVisitors.Rewriting
{
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
            while (current != null);

            switch (root)
            {
                case SqlExpression sqlExpression
                when !sqlExpression.Type.IsScalarType():
                {
                    var function = node.Type.IsScalarType() ? "JSON_VALUE" : "JSON_QUERY";
                    var patharg = Expression.Constant($"$.{string.Join(".", path.Select(p => p.Name))}");

                    return new SqlFunctionExpression(function, node.Type, sqlExpression, patharg);
                }

                default:
                {
                    return base.VisitMember(node);
                }
            }
        }
    }
}
