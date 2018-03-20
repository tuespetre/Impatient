using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    /// <summary>
    /// An <see cref="ExpressionVisitor"/> that can be used to ensure that
    /// a projection expression has some referenceable alias before being
    /// pushed down into a subquery. For example, a select expression with 
    /// a projection expression consisting of a single subquery 'column' with 
    /// no alias is not valid for pushing down into a subquery until that 
    /// single subquery 'column' is given an alias, allowing it to be referred
    /// to outside of the subquery.
    /// </summary>
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
