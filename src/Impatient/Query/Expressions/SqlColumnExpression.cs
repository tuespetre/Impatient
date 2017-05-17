using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SqlColumnExpression : SqlExpression
    {
        public SqlColumnExpression(AliasedTableExpression table, string columnName, Type type)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public AliasedTableExpression Table { get; }

        public string ColumnName { get; }

        public override Type Type { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor)
        {
            var table = visitor.VisitAndConvert(Table, nameof(VisitChildren));

            if (table != Table)
            {
                return new SqlColumnExpression(table, ColumnName, Type);
            }

            return this;
        }
    }
}
