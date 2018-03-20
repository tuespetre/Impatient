using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class SqlColumnExpression : SqlExpression
    {
        public SqlColumnExpression(AliasedTableExpression table, string columnName, Type type, bool isNullable = false)
        {
            Table = table ?? throw new ArgumentNullException(nameof(table));
            ColumnName = columnName ?? throw new ArgumentNullException(nameof(columnName));
            Type = type ?? throw new ArgumentNullException(nameof(type));
            IsNullable = isNullable;
        }

        public AliasedTableExpression Table { get; }

        public string ColumnName { get; }

        public override Type Type { get; }

        public override bool IsNullable { get; }

        // The SqlColumnExpression holds a reference to a AliasedTableExpression only in
        // a semantic sense, not a structural one -- that is to say, the table is not a proper
        // 'child node' of the column and thus should not be visited.
        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;
    }
}
