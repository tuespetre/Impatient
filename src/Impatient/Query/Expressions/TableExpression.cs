using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public abstract class TableExpression : Expression
    {
        public TableExpression(Type type)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public override Type Type { get; }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public abstract IEnumerable<AliasedTableExpression> Flatten();
    }
}
