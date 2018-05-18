using Impatient.Query.Expressions;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    public class SqlColumnNullabilityExpressionVisitor : ProjectionExpressionVisitor
    {
        private readonly bool nullable;

        public SqlColumnNullabilityExpressionVisitor(bool nullable)
        {
            this.nullable = nullable;
        }

        protected override Expression VisitLeaf(Expression node)
        {
            switch (node)
            {
                case SqlColumnExpression sqlColumnExpression:
                {
                    return new SqlColumnExpression(
                        sqlColumnExpression.Table,
                        sqlColumnExpression.ColumnName,
                        sqlColumnExpression.Type,
                        isNullable: nullable,
                        typeMapping: sqlColumnExpression.TypeMapping);
                }

                default:
                {
                    return node;
                }
            }
        }
    }
}
