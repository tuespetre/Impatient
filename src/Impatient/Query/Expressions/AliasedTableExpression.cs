using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public abstract class AliasedTableExpression : TableExpression
    {
        public AliasedTableExpression(string alias, Type type) : base(type)
        {
            Alias = alias ?? throw new ArgumentNullException(nameof(alias));
        }

        public string Alias { get; }

        protected override Expression VisitChildren(ExpressionVisitor visitor) => this;

        public override IEnumerable<AliasedTableExpression> Flatten()
        {
            return Enumerable.Repeat(this, 1);
        }
    }
}
