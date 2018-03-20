using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class BaseTableExpression : AliasedTableExpression
    {
        public BaseTableExpression(string schemaName, string tableName, string alias, Type type) : base(alias, type)
        {
            SchemaName = schemaName ?? throw new ArgumentNullException(nameof(schemaName));
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        public string SchemaName { get; }

        public string TableName { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public BaseTableExpression Clone() => new BaseTableExpression(SchemaName, TableName, Alias, Type);
    }
}
