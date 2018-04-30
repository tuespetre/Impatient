using Impatient.Query.Infrastructure;
using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class BaseTableExpression : AliasedTableExpression
    {
        public BaseTableExpression(string schemaName, string tableName, string alias, Type type) : base(alias, type)
        {
            SchemaName = schemaName;
            TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        }

        public string SchemaName { get; }

        public string TableName { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public BaseTableExpression Clone() => new BaseTableExpression(SchemaName, TableName, Alias, Type);

        public override int GetSemanticHashCode(ExpressionEqualityComparer comparer)
        {
            unchecked
            {
                var hash = TableName.GetHashCode();

                hash = (hash * 16777619) ^ Alias.GetHashCode();
                hash = (hash * 16777619) ^ SchemaName?.GetHashCode() ?? 0;

                return hash;
            }
        }
    }
}
