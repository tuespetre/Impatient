using System;
using System.Linq.Expressions;

namespace Impatient.Query.Expressions
{
    public class MetaAliasExpression : AnnotationExpression
    {
        public MetaAliasExpression(Expression expression, SqlAliasExpression aliasExpression) : base(expression)
        {
            AliasExpression = aliasExpression ?? throw new ArgumentNullException(nameof(aliasExpression));
        }

        public SqlAliasExpression AliasExpression { get; }

        protected override AnnotationExpression Recreate(Expression expression)
        {
            return new MetaAliasExpression(expression, AliasExpression);
        }
    }
}
