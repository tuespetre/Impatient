using Impatient.Query.Expressions;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.ExpressionVisitors.Utility
{
    /// <summary>
    /// An <see cref="ExpressionVisitor"/> that, given an old table and a new table,
    /// updates all references to the old table (and its sub-tables) to point instead
    /// to the new table (or its sub-tables.)
    /// </summary>
    public class TableUpdatingExpressionVisitor : ExpressionVisitor
    {
        private readonly AliasedTableExpression[] oldTables;
        private readonly AliasedTableExpression[] newTables;

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
                            sqlColumnExpression.IsNullable,
                            sqlColumnExpression.TypeMapping);
                    }

                    return sqlColumnExpression;
                }

                default:
                {
                    return base.Visit(node);
                }
            }
        }
    }
}
