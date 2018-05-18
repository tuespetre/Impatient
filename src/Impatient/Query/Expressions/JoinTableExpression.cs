using System;
using System.Collections.Generic;

namespace Impatient.Query.Expressions
{
    public abstract class JoinTableExpression : TableExpression
    {
        public JoinTableExpression(TableExpression outerTable, AliasedTableExpression innerTable, Type type) : base(type)
        {
            OuterTable = outerTable ?? throw new ArgumentNullException(nameof(outerTable));
            InnerTable = innerTable ?? throw new ArgumentNullException(nameof(innerTable));
        }

        public TableExpression OuterTable { get; }

        public AliasedTableExpression InnerTable { get; }

        public override IEnumerable<AliasedTableExpression> Flatten()
        {
            foreach (var table in OuterTable.Flatten())
            {
                yield return table;
            }

            yield return InnerTable;
        }
    }
}
