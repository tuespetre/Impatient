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
                case SqlColumnExpression sqlColumn:
                {
                    return sqlColumn;
                }

                case BaseTableExpression baseTable:
                {
                    return new BaseTableExpression(
                        baseTable.SchemaName,
                        baseTable.TableName,
                        baseTable.Alias,
                        baseTable.Type);
                }
            }

            return base.Visit(node);
        }
    }
}
