using Impatient.Query.Expressions;
using System;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    public class TableUpdatingExpressionVisitor : ExpressionVisitor
    {
        private AliasedTableExpression[] oldTables;
        private AliasedTableExpression[] newTables;

        public TableUpdatingExpressionVisitor(
            AliasedTableExpression[] oldTables,
            AliasedTableExpression[] newTables)
        {
            this.oldTables = oldTables;
            this.newTables = newTables;
        }

        public override Expression Visit(Expression node)
        {
            switch (node)
            {
                case SqlColumnExpression sqlColumnExpression:
                {
                    var index = Array.IndexOf(oldTables, sqlColumnExpression.Table);

                    if (index > -1)
                    {
                        return new SqlColumnExpression(
                            newTables[index],
                            sqlColumnExpression.ColumnName,
                            sqlColumnExpression.Type,
                            sqlColumnExpression.IsNullable);
                    }

                    return sqlColumnExpression;
                }

                case AliasedTableExpression aliasedTableExpression:
                {
                    var index = Array.IndexOf(oldTables, aliasedTableExpression);

                    if (index > -1)
                    {
                        return newTables[index];
                    }

                    return aliasedTableExpression;
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }
    }
}
