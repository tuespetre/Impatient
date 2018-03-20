using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    public class SubqueryAliasDecoratingExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case null:
                case NewExpression _:
                case MemberInitExpression _:
                case GroupByResultExpression _:
                case GroupedRelationalQueryExpression _:
                case PolymorphicExpression _:
                case SqlColumnExpression _:
                case SqlAliasExpression _:
                case DefaultIfEmptyExpression _:
                {
                    return node;
                }

                case AnnotationExpression _:
                {
                    return base.Visit(node);
                }

                default:
                {
                    return new SqlAliasExpression(node, "$c");
                }
            }
        }
    }
}
