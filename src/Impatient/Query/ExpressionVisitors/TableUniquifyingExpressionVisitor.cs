using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors
{
    public class TableUniquifyingExpressionVisitor : ExpressionVisitor
    {
        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case SqlColumnExpression sqlColumnExpression:
                {
                    return sqlColumnExpression;
                }

                case BaseTableExpression baseTableExpression:
                {
                    return new BaseTableExpression(
                        baseTableExpression.SchemaName,
                        baseTableExpression.TableName,
                        baseTableExpression.Alias,
                        baseTableExpression.Type);
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }
    }
}
